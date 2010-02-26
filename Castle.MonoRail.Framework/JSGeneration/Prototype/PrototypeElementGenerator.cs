﻿// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.Framework.JSGeneration.Prototype
{
	using System;

	/// <summary>
	/// Implementation for the <see cref="IJSElementGenerator"/>
	/// </summary>
	public class PrototypeElementGenerator : IJSElementGenerator
	{
		private readonly PrototypeGenerator generator;

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="PrototypeElementGenerator"/> class.
		/// </summary>
		/// <param name="generator">The generator.</param>
		/// <param name="root">The root.</param>
		public PrototypeElementGenerator(PrototypeGenerator generator, String root)
		{
			this.generator = generator;

			generator.CodeGenerator.Record("$('" + root + "')");
		}

		#endregion

		/// <summary>
		/// Gets the parent generator.
		/// </summary>
		/// <value>The parent generator.</value>
		public IJSGenerator ParentGenerator
		{
			get { return generator; }
		}

		#region Dispatchable operations

		/// <summary>
		/// Replaces the content of the element.
		/// </summary>
		/// <param name="renderOptions">Defines what to render</param>
		/// <example>
		/// The following example uses nvelocity syntax:
		/// <code>
		/// $page.el('elementid').ReplaceHtml("%{partial='shared/newmessage.vm'}")
		/// </code>
		/// </example>
		public void ReplaceHtml(object renderOptions)
		{
			generator.CodeGenerator.ReplaceTailByPeriod();

			generator.CodeGenerator.Call("update", generator.Render(renderOptions));
		}

		/// <summary>
		/// Replaces the entire element's content -- and not only its innerHTML --
		/// by the content evaluated.
		/// </summary>
		/// <param name="renderOptions">Defines what to render</param>
		/// <example>
		/// The following example uses nvelocity syntax:
		/// <code>
		/// $page.el('messagediv').Replace("%{partial='shared/newmessage.vm'}")
		/// </code>
		/// </example>
		public void Replace(object renderOptions)
		{
			generator.CodeGenerator.ReplaceTailByPeriod();

			generator.CodeGenerator.Call("replace", generator.Render(renderOptions));
		}

		#endregion
	}
}
