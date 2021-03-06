// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.MonoRail.Framework
{
	using System;

	/// <summary>
	/// Declares that the Controller should enable a DefaultAction method 
	/// for request processing if no action can be found with the supplied name
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple=false), Serializable]
	public class DefaultActionAttribute : Attribute
	{
		private static readonly String DEFAULT_ACTION = "DefaultAction";

		private String defaultAction;

		/// <summary>
		/// Constructs a <see cref="DefaultActionAttribute"/>
		/// using <c>DefaultAction</c>
		/// as the default action name
		/// </summary>
		public DefaultActionAttribute()
		{
			defaultAction = DEFAULT_ACTION;
		}

		/// <summary>
		/// Constructs a <see cref="DefaultActionAttribute"/>
		/// using the supplied value as the default action name
		/// </summary>
		public DefaultActionAttribute(String action)
		{
			defaultAction = action;
		}

		/// <summary>
		/// Gets the default action name
		/// </summary>
		public String DefaultAction
		{
			get { return defaultAction; }
		}
	}
}
