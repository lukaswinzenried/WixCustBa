//-------------------------------------------------------------------------------------------------
// <copyright file="PSCompiler.cs" company="Outercurve Foundation">
//   Copyright (c) 2004, Outercurve Foundation.
//   This software is released under Microsoft Reciprocal License (MS-RL).
//   The license and further copyright text can be found in the file
//   LICENSE.TXT at the root directory of the distribution.
// </copyright>
// 
// <summary>
// The compiler for the WiX Toolset PowerShell Extension.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace WixToolset.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Xml.Linq;

    /// <summary>
    /// The compiler for the WiX Toolset Internet Information Services Extension.
    /// </summary>
    public sealed class PSCompiler : CompilerExtension
    {
        private const string KeyFormat = @"SOFTWARE\Microsoft\PowerShell\{0}\PowerShellSnapIns\{1}";
        private const string VarPrefix = "PSVersionMajor";

        /// <summary>
        /// Instantiate a new PSCompiler.
        /// </summary>
        public PSCompiler()
        {
            this.Namespace = "http://wixtoolset.org/schemas/v4/wxs/powershell";
        }

        /// <summary>
        /// Processes an attribute for the Compiler.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line number for the parent element.</param>
        /// <param name="parentElement">Parent element of attribute.</param>
        /// <param name="attribute">Attribute to process.</param>
        public override void ParseAttribute(XElement parentElement, XAttribute attribute, IDictionary<string, string> context)
        {
            SourceLineNumber sourceLineNumbers = Preprocessor.GetSourceLineNumbers(attribute.Parent);

            string requiredVersion = null;
            switch (attribute.Name.LocalName)
            {
                case "RequiredVersion":
                    requiredVersion = this.Core.GetAttributeValue(sourceLineNumbers, attribute);
                    break;

                default:
                    this.Core.UnexpectedAttribute(sourceLineNumbers, attribute);
                    break;
            }

            if (!String.IsNullOrEmpty(requiredVersion))
            {
                this.Core.VerifyRequiredVersion(sourceLineNumbers, requiredVersion);
            }
        }

        /// <summary>
        /// Processes an element for the Compiler.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line number for the parent element.</param>
        /// <param name="parentElement">Parent element of element to process.</param>
        /// <param name="element">Element to process.</param>
        /// <param name="contextValues">Extra information about the context in which this element is being parsed.</param>
        public override void ParseElement(XElement parentElement, XElement element, IDictionary<string, string> context)
        {
            switch (parentElement.Name.LocalName)
            {
                case "File":
                    string fileId = context["FileId"];
                    string componentId = context["ComponentId"];

                    switch (element.Name.LocalName)
                    {
                        case "FormatsFile":
                            this.ParseExtensionsFile(element, "Formats", fileId, componentId);
                            break;

                        case "SnapIn":
                            this.ParseSnapInElement(element, fileId, componentId);
                            break;

                        case "TypesFile":
                            this.ParseExtensionsFile(element, "Types", fileId, componentId);
                            break;

                        default:
                            this.Core.UnexpectedElement(parentElement, element);
                            break;
                    }
                    break;

                default:
                    this.Core.UnexpectedElement(parentElement, element);
                    break;
            }
        }

        /// <summary>
        /// Parses a SnapIn element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="fileId">Identifier for parent file.</param>
        /// <param name="componentId">Identifier for parent component.</param>
        private void ParseSnapInElement(XElement node, string fileId, string componentId)
        {
            SourceLineNumber sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string assemblyName = null;
            string customSnapInType = null;
            string description = null;
            string descriptionIndirect = null;
            Version requiredPowerShellVersion = CompilerCore.IllegalVersion;
            string vendor = null;
            string vendorIndirect = null;
            string version = null;

            foreach (XAttribute attrib in node.Attributes())
            {
                if (String.IsNullOrEmpty(attrib.Name.NamespaceName) || this.Namespace == attrib.Name.Namespace)
                {
                    switch (attrib.Name.LocalName)
                    {
                        case "Id":
                            id = this.Core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;

                        case "CustomSnapInType":
                            customSnapInType = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;

                        case "Description":
                            description = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;

                        case "DescriptionIndirect":
                            descriptionIndirect = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;

                        case "RequiredPowerShellVersion":
                            string ver = this.Core.GetAttributeVersionValue(sourceLineNumbers, attrib, false);
                            requiredPowerShellVersion = new Version(ver);
                            break;

                        case "Vendor":
                            vendor = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;

                        case "VendorIndirect":
                            vendorIndirect = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;

                        case "Version":
                            version = this.Core.GetAttributeVersionValue(sourceLineNumbers, attrib, false);
                            break;

                        default:
                            this.Core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.Core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.Core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name.LocalName, "Id"));
            }

            // Default to require PowerShell 1.0.
            if (CompilerCore.IllegalVersion == requiredPowerShellVersion)
            {
                requiredPowerShellVersion = new Version(1, 0);
            }

            // If the snap-in version isn't explicitly specified, get it
            // from the assembly version at bind time.
            if (null == version)
            {
                version = String.Format("!(bind.assemblyVersion.{0})", fileId);
            }

            foreach (XElement child in node.Elements())
            {
                if (this.Namespace == child.Name.Namespace)
                {
                    switch (child.Name.LocalName)
                    {
                        case "FormatsFile":
                            this.ParseExtensionsFile(child, "Formats", id, componentId);
                            break;
                        case "TypesFile":
                            this.ParseExtensionsFile(child, "Types", id, componentId);
                            break;
                        default:
                            this.Core.UnexpectedElement(node, child);
                            break;
                    }
                }
                else
                {
                    this.Core.ParseExtensionElement(node, child);
                }
            }

            // Get the major part of the required PowerShell version which is
            // needed for the registry key, and put that into a WiX variable
            // for use in Formats and Types files. PowerShell v2 still uses 1.
            int major = (2 == requiredPowerShellVersion.Major) ? 1 : requiredPowerShellVersion.Major;

            WixVariableRow wixVariableRow = (WixVariableRow)this.Core.CreateRow(sourceLineNumbers, "WixVariable");
            wixVariableRow.Id = String.Format(CultureInfo.InvariantCulture, "{0}_{1}", VarPrefix, id);
            wixVariableRow.Value = major.ToString(CultureInfo.InvariantCulture);
            wixVariableRow.Overridable = false;

            int registryRoot = 2; // HKLM
            string registryKey = String.Format(CultureInfo.InvariantCulture, KeyFormat, major, id);

            this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, registryKey, "ApplicationBase", String.Format(CultureInfo.InvariantCulture, "[${0}]", componentId), componentId, false);

            // set the assembly name automatically when binding.
            // processorArchitecture is not handled correctly by PowerShell v1.0
            // so format the assembly name explicitly.
            assemblyName = String.Format(CultureInfo.InvariantCulture, "!(bind.assemblyName.{0}), Version=!(bind.assemblyVersion.{0}), Culture=!(bind.assemblyCulture.{0}), PublicKeyToken=!(bind.assemblyPublicKeyToken.{0})", fileId);
            this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, registryKey, "AssemblyName", assemblyName, componentId, false);

            if (null != customSnapInType)
            {
                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, registryKey, "CustomPSSnapInType", customSnapInType, componentId, false);
            }

            if (null != description)
            {
                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, registryKey, "Description", description, componentId, false);
            }

            if (null != descriptionIndirect)
            {
                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, registryKey, "DescriptionIndirect", descriptionIndirect, componentId, false);
            }

            this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, registryKey, "ModuleName", String.Format(CultureInfo.InvariantCulture, "[#{0}]", fileId), componentId, false);

            this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, registryKey, "PowerShellVersion", requiredPowerShellVersion.ToString(2), componentId, false);

            if (null != vendor)
            {
                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, registryKey, "Vendor", vendor, componentId, false);
            }

            if (null != vendorIndirect)
            {
                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, registryKey, "VendorIndirect", vendorIndirect, componentId, false);
            }

            if (null != version)
            {
                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, registryKey, "Version", version, componentId, false);
            }
        }

        /// <summary>
        /// Parses a FormatsFile and TypesFile element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="valueName">Registry value name.</param>
        /// <param name="id">Idendifier for parent file or snap-in.</param>
        /// <param name="componentId">Identifier for parent component.</param>
        private void ParseExtensionsFile(XElement node, string valueName, string id, string componentId)
        {
            SourceLineNumber sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string fileId = null;
            string snapIn = null;

            foreach (XAttribute attrib in node.Attributes())
            {
                if (String.IsNullOrEmpty(attrib.Name.NamespaceName) || this.Namespace == attrib.Name.Namespace)
                {
                    switch (attrib.Name.LocalName)
                    {
                        case "FileId":
                            fileId = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            snapIn = id;
                            break;

                        case "SnapIn":
                            fileId = id;
                            snapIn = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;

                        default:
                            this.Core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.Core.ParseExtensionAttribute(node, attrib);
                }
            }

            if (null == fileId && null == snapIn)
            {
                this.Core.OnMessage(PSErrors.NeitherIdSpecified(sourceLineNumbers, valueName));
            }

            this.Core.ParseForExtensionElements(node);

            int registryRoot = 2; // HKLM
            string registryKey = String.Format(CultureInfo.InvariantCulture, KeyFormat, String.Format(CultureInfo.InvariantCulture, "!(wix.{0}_{1})", VarPrefix, snapIn), snapIn);

            this.Core.CreateWixSimpleReferenceRow(sourceLineNumbers, "File", fileId);
            this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, registryKey, valueName, String.Format(CultureInfo.InvariantCulture, "[~][#{0}]", fileId), componentId, false);
        }
    }
}
