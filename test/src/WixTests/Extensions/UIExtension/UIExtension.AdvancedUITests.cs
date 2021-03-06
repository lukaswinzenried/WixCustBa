//-----------------------------------------------------------------------
// <copyright file="UIExtension.AdvancedUITests.cs" company="Outercurve Foundation">
//   Copyright (c) 2004, Outercurve Foundation.
//   This software is released under Microsoft Reciprocal License (MS-RL).
//   The license and further copyright text can be found in the file
//   LICENSE.TXT at the root directory of the distribution.
// </copyright>
// <summary>UI Extension AdvancedUI tests</summary>
//-----------------------------------------------------------------------

namespace WixTest.Tests.Extensions.UIExtension
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using WixTest;
    using WixTest.Verifiers;

    /// <summary>
    /// NetFX extension AdvancedUI element tests
    /// </summary>
    [TestClass]
    public class AdvancedUITests : WixTests
    {
        private static readonly string TestDataDirectory = Environment.ExpandEnvironmentVariables(@"%WIX_ROOT%\test\data\Extensions\UIExtension\AdvancedUITests");

        [TestMethod]
        [Description("Verify that the CustomAction Table is created in the MSI and has the expected data.")]
        [Priority(1)]
        public void AdvancedUI_VerifyMSITableData()
        {
            string sourceFile = Path.Combine(AdvancedUITests.TestDataDirectory, @"InstallforAllUser.wxs");
            string msiFile = Builder.BuildPackage(sourceFile, "test.msi", "WixUIExtension");

            Verifier.VerifyCustomActionTableData(msiFile,
                new CustomActionTableData("WixUIValidatePath", 65, "WixUIWixca", "ValidatePath"),
                new CustomActionTableData("WixUIPrintEula", 65, "WixUIWixca", "PrintEula"),
                new CustomActionTableData("WixSetDefaultPerUserFolder", 51, "WixPerUserFolder", @"[LocalAppDataFolder]Apps\[ApplicationFolderName]"),
                new CustomActionTableData("WixSetDefaultPerMachineFolder", 51, "WixPerMachineFolder", "[ProgramFilesFolder][ApplicationFolderName]"),
                new CustomActionTableData("WixSetPerUserFolder", 51, "APPLICATIONFOLDER", "[WixPerUserFolder]"),
                new CustomActionTableData("WixSetPerMachineFolder", 51, "APPLICATIONFOLDER", "[WixPerMachineFolder]"));
        }

        [TestMethod]
        [Description("Verify using the msilog that the correct actions was executed.")]
        [Priority(2)]
        [TestProperty("IsRuntimeTest", "false")]
        [Ignore]
        public void AdvancedUI_InstallforAllUser()
        {
        }
      
        [TestMethod]
        [Description("Verify using the msilog that the correct actions was executed.")]
        [Priority(2)]
        [TestProperty("IsRuntimeTest", "false")]
        [Ignore]
        public void AdvancedUI_InstallJustForYou()
        {
        }
     }
}
