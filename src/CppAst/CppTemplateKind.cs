// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CppAst
{
	/// <summary>
	/// Type of a template
	/// </summary>
	public enum CppTemplateKind
	{
		/// <summary>
		/// not a template class, just a normal class
		/// </summary>
		NormalClass,
		/// <summary>
		/// A class template
		/// </summary>
		TemplateClass,
		/// <summary>
		/// A partial template class
		/// </summary>
		PartialTemplateClass,
		/// <summary>
		/// A class with full template specialized
		/// </summary>
		TemplateSpecializedClass,
	}


}

