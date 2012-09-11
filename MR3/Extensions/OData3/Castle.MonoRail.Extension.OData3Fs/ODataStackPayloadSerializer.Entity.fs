﻿//  Copyright 2004-2012 Castle Project - http://www.castleproject.org/
//  Hamilton Verissimo de Oliveira and individual contributors as indicated. 
//  See the committers.txt/contributors.txt in the distribution for a 
//  full listing of individual contributors.
// 
//  This is free software; you can redistribute it and/or modify it
//  under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 3 of
//  the License, or (at your option) any later version.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this software; if not, write to the Free
//  Software Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA
//  02110-1301 USA, or see the FSF site: http://www.fsf.org.

namespace Castle.MonoRail.OData.Internal

    open System
    open System.Collections
    open System.Collections.Specialized
    open System.Collections.Generic
    open System.IO
    open System.Text
    open System.Linq
    open System.Linq.Expressions
    open System.Web
    open Castle.MonoRail
    open Microsoft.Data.OData
    open Microsoft.Data.Edm
    open Microsoft.Data.Edm.Library
    open Microsoft.Data.OData.Atom

    

    // Feed/Entry
    type EntitySerializer(odataMsgWriter:ODataMessageWriter) = 

        let rec build_odataprop element (edmProp:IEdmProperty) = 
            let prop = edmProp |> box :?> TypedEdmStructuralProperty
            let name = edmProp.Name
            let value = prop.GetValue(element)

            if edmProp.Type.IsEnum() then 
                // I dont think the OData lib supports writing enums yet
                // ODataProperty(Name = name, Value = value)
                null

            elif edmProp.Type.IsComplex() then
                if value = null then 
                    ODataProperty(Name = name, Value = null)
                else
                    let complexType = edmProp.Type.Definition :?> IEdmComplexType

                    let props = 
                        complexType.Properties() 
                        |> Seq.map (fun p -> build_odataprop value p)
                    let complexVal = ODataComplexValue(TypeName = edmProp.Type.FullName(), Properties = props)

                    ODataProperty(Name = name, Value = complexVal)

            elif edmProp.Type.IsCollection() then
                let collVal = ODataCollectionValue(TypeName = edmProp.Type.FullName())
                
                if value <> null then
                    let collType = edmProp.Type.Definition :?> IEdmCollectionType
                    let elRefType = collType.ElementType

                    let items = 
                        (value :?> IEnumerable) 
                        |> Seq.cast<obj> 
                        |> Seq.map (fun e -> if elRefType.IsComplex() then
                                                let props = 
                                                    (elRefType.Definition :?> IEdmComplexType).Properties() 
                                                    |> Seq.map (fun p -> build_odataprop e p)
                                                ODataComplexValue(TypeName = elRefType.FullName(), Properties = props)
                                             else
                                                failwithf "Element type not support for collection" )

                    collVal.Items <- items

                ODataProperty(Name = name, Value = collVal)

            else 
                ODataProperty(Name = name, Value = value)

        and build_odatanavigation (writer:ODataWriter) element (edmProp:IEdmProperty) = 
            let prop = edmProp |> box :?> TypedEdmNavigationProperty
            let name = edmProp.Name
            // let value = prop.GetValue(element)
            // ODataProperty(Name = name, Value = value)
            let navLink = ODataNavigationLink(Name = name, IsCollection = Nullable(edmProp.Type.IsCollection()) )
            navLink.Url <- Uri("testing", UriKind.Relative)
            
            writer.WriteStart(navLink)

            // if expand
            if edmProp.Type.IsCollection() then
                let value = prop.GetValue(element)

                if value = null then
                    writer.WriteStart(ODataFeed())
                    writer.WriteEnd()
                else
                    let elements = Queryable.AsQueryable(value |> box :?> IEnumerable)
                    System.Diagnostics.Debug.Assert( prop.Type.IsCollection() )
                    let collType = prop.Type.Definition :?> IEdmCollectionType
                    System.Diagnostics.Debug.Assert( collType.ElementType.IsEntity() )
                    write_feed_items writer elements (collType.ElementType.Definition :?> IEdmEntityType) name
            else 
                let value = prop.GetValue(element)
                if value = null then
                    writer.WriteStart(null :> ODataEntry)
                    writer.WriteEnd()
                else
                    write_entry writer value (prop.Type.Definition :?> IEdmEntityType)

            writer.WriteEnd()

        and get_properties (element:obj) (edmType:IEdmEntityType) = 
            edmType.Properties()
            |> Seq.filter (fun p -> p.PropertyKind = EdmPropertyKind.Structural)
            |> Seq.map (fun p -> build_odataprop element p) 
            |> Seq.filter (fun p -> p <> null) // hack for enums

        and write_navigations (writer:ODataWriter) (element:obj) (edmType:IEdmEntityType) = 
            edmType.Properties()
            |> Seq.filter (fun p -> p.PropertyKind = EdmPropertyKind.Navigation)
            |> Seq.iter (fun p -> build_odatanavigation writer element p)
            
        and write_feed_items (writer:ODataWriter) (elements:IQueryable) (edmType:IEdmEntityType) title = 
            let feed = ODataFeed()
            feed.Id <- "testingId"
            let annotation = AtomFeedMetadata()
            annotation.Title <- new AtomTextConstruct(Text = title)
            annotation.SelfLink <- new AtomLinkMetadata(Href = Uri("relId1", UriKind.Relative), Title = title)
            feed.SetAnnotation(annotation);

            writer.WriteStart(feed)

            for e in elements do
                write_entry writer e edmType

            writer.WriteEnd()

        and write_entry  (writer:ODataWriter) (element:obj) (edmType:IEdmEntityType) = 
            let entry = ODataEntry()
            let annotation = AtomEntryMetadata()
            entry.SetAnnotation(annotation);

            // Uri id;
            // Uri idAndEditLink = Serializer.GetIdAndEditLink(element, actualResourceType, this.Provider, this.CurrentContainer, this.AbsoluteServiceUri, out id);
            // Uri relativeUri = new Uri(idAndEditLink.AbsoluteUri.Substring(this.AbsoluteServiceUri.AbsoluteUri.Length), UriKind.Relative);

            let name = edmType.FName

            entry.TypeName  <- name // actualResourceType.FullName
            entry.Id        <- "testing"  // id.AbsoluteUri
            // entry.EditLink  <- relativeUri
            annotation.EditLink <- AtomLinkMetadata(Title = name);

            // let etagValue = GetETagValue(element, actualResourceType)
            // entry.ETag = etagValue

            // hypertext support
            // PopulateODataOperations(element, resourceInstanceInFeed, entry, actualResourceType)

            writer.WriteStart (entry)
            write_navigations writer element edmType
            entry.Properties <- get_properties element (edmType)
            writer.WriteEnd()

        member x.WriteFeed  (edmEntSet:IEdmEntitySet, elements:IQueryable, elType:IEdmEntityType) = 
            let writer = odataMsgWriter.CreateODataFeedWriter(edmEntSet, elType)
            let title = "test"
            write_feed_items writer elements elType title
            ()

        member x.WriteEntry (edmEntSet:IEdmEntitySet, element:obj, elType:IEdmEntityType) = 
            let writer = odataMsgWriter.CreateODataEntryWriter(edmEntSet, elType)
            write_entry writer element elType

