//-------------------------------------------------------------------------------------------------
// <copyright file="CompilerExtension.cs" company="Outercurve Foundation">
//   Copyright (c) 2004, Outercurve Foundation.
//   This software is released under Microsoft Reciprocal License (MS-RL).
//   The license and further copyright text can be found in the file
//   LICENSE.TXT at the root directory of the distribution.
// </copyright>
// 
// <summary>
// The base compiler extension.  Any of these methods can be overridden to change
// the behavior of the compiler.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace WixToolset
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;

    /// <summary>
    /// Base class for creating a compiler extension.
    /// </summary>
    public abstract class CompilerExtension
    {
        /// <summary>
        /// Gets or sets the compiler core for the extension.
        /// </summary>
        /// <value>Compiler core for the extension.</value>
        public CompilerCore Core { get; set; }

        /// <summary>
        /// Gets the schema namespace for this extension.
        /// </summary>
        /// <value>Schema namespace supported by this extension.</value>
        public XNamespace Namespace { get; protected set; }

        /// <summary>
        /// Called at the beginning of the compilation of a source file.
        /// </summary>
        public virtual void InitializeCompile()
        {
        }

        /// <summary>
        /// Processes an attribute for the Compiler.
        /// </summary>
        /// <param name="parentElement">Parent element of attribute.</param>
        /// <param name="attribute">Attribute to process.</param>
        /// <param name="contextValues">Extra information about the context in which this element is being parsed.</param>
        public virtual void ParseAttribute(XElement parentElement, XAttribute attribute, IDictionary<string, string> contextValues)
        {
            this.Core.UnexpectedAttribute(parentElement, attribute);
        }

        /// <summary>
        /// Processes an element for the Compiler.
        /// </summary>
        /// <param name="parentElement">Parent element of element to process.</param>
        /// <param name="element">Element to process.</param>
        /// <param name="contextValues">Extra information about the context in which this element is being parsed.</param>
        public virtual void ParseElement(XElement parentElement, XElement element, IDictionary<string, string> context)
        {
            this.Core.UnexpectedElement(parentElement, element);
        }

        /// <summary>
        /// Processes an element for the Compiler, with the ability to supply a component keypath.
        /// </summary>
        /// <param name="parentElement">Parent element of element to process.</param>
        /// <param name="element">Element to process.</param>
        /// <param name="keyPath">Explicit key path.</param>
        /// <param name="contextValues">Extra information about the context in which this element is being parsed.</param>
        public virtual ComponentKeyPath ParsePossibleKeyPathElement(XElement parentElement, XElement element, IDictionary<string, string> context)
        {
            this.ParseElement(parentElement, element, context);
            return null;
        }

        /// <summary>
        /// Called at the end of the compilation of a source file.
        /// </summary>
        public virtual void FinalizeCompile()
        {
        }
    }

    public enum ComponentKeyPathType
    {
        None,
        File,
        Directory,
        OdbcDataSource,
        Registry,
        RegistryFormatted
    }

    public class ComponentKeyPath
    {
        public string Id { get; set; }

        public bool Explicit { get; set; }

        public ComponentKeyPathType Type { get; set; }
    }
}
