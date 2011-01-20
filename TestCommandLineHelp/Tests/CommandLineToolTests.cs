// Microsoft Public License (Ms-PL)
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.

using System;
using CommandLine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable CheckNamespace
namespace TestCommandLineHelper
// ReSharper restore CheckNamespace
{
    [TestClass]
    public class CommandLineToolTests
    {
        [TestMethod]
        public void ToolInstantiationTest()
        {
            var tool = new GetAcls();

            Assert.IsNotNull(tool);
        }

        [TestMethod]
        public void ToolGetUsageTest()
        {
            var tool = new GetAcls();
            string usage = tool.GetUsage();

            Assert.IsFalse(String.IsNullOrEmpty(usage));
        }

        [TestMethod]
        public void ToolParseMultipleLongArgumentsTest()
        {
            var tool = new GetAcls();

            // test the long name versions of the parameters
            // ReSharper disable RedundantExplicitArrayCreation
            var args = new string[]
            {
                "--app=\"C:\\windows\\system32\\icacls.exe\"", 
                "--path=C:\\",
                "--log=\"C:\\temp\\out.log\"",
                "--appendToLogFile"
            };
            // ReSharper restore RedundantExplicitArrayCreation

            var parser = new CommandLineParser(new CommandLineParserSettings(Console.Error));
            bool parsed = parser.ParseArguments(args, tool);

            Assert.IsTrue(parsed);
            Assert.IsTrue(tool.ApplicationPath.Length != 0);
            Assert.AreEqual(tool.ApplicationPath, "C:\\windows\\system32\\icacls.exe");
            Assert.IsTrue(tool.PermissionsPath.Length != 0);
            Assert.AreEqual(tool.PermissionsPath, "C:\\");
            Assert.IsTrue(tool.LogFilePath.Length != 0);
            Assert.AreEqual(tool.LogFilePath, "C:\\temp\\out.log");
            Assert.IsTrue(tool.AppendToLogFile);
        }

        [TestMethod]
        public void ToolParseMultipleShortArgumentsTest()
        {
            var tool = new GetAcls();

            // test the short name versions of the parameters
            // ReSharper disable RedundantExplicitArrayCreation
            var args = new string[]
            {
                "-a\"C:\\windows\\system32\\icacls.exe\"", 
                "-pC:\\",
                "-l\"C:\\temp\\out.log\""
            };
            // ReSharper restore RedundantExplicitArrayCreation

            var parser = new CommandLineParser(new CommandLineParserSettings(Console.Error));
            bool parsed = parser.ParseArguments(args, tool);

            Assert.IsTrue(parsed);
            Assert.IsTrue(tool.ApplicationPath.Length != 0);
            Assert.AreEqual(tool.ApplicationPath, "C:\\windows\\system32\\icacls.exe");
            Assert.IsTrue(tool.PermissionsPath.Length != 0);
            Assert.AreEqual(tool.PermissionsPath, "C:\\");
            Assert.IsTrue(tool.LogFilePath.Length != 0);
            Assert.AreEqual(tool.LogFilePath, "C:\\temp\\out.log");
            Assert.IsFalse(tool.AppendToLogFile);
        }

        [TestMethod]
        public void ToolNegativeParseArgumentsTest()
        {
            var tool = new GetAcls();

            // ReSharper disable RedundantExplicitArrayCreation
            var args = new string[]
            {
                "--app=\"C:\\win dows\\syst-em32\\ica=cls.exe\"", 
                "--path=C:\\e=mc2--done.txt",
            };
            // ReSharper restore RedundantExplicitArrayCreation

            var parser = new CommandLineParser(new CommandLineParserSettings(Console.Error));
            bool parsed = parser.ParseArguments(args, tool);

            Assert.IsTrue(parsed);
            Assert.IsTrue(tool.ApplicationPath.Length != 0);
            Assert.AreEqual(tool.ApplicationPath, "C:\\win dows\\syst-em32\\ica=cls.exe");
            Assert.IsTrue(tool.PermissionsPath.Length != 0);
            Assert.AreEqual(tool.PermissionsPath, "C:\\e=mc2--done.txt");

            // ReSharper disable RedundantExplicitArrayCreation
            args = new string[] // missing required path parameter
            {
                "--app=\"C:\\win dows\\syst-em32\\ica=cls.exe\"", 
            };

            parsed = parser.ParseArguments(args, tool);
            Assert.IsFalse(parsed);
            // ReSharper restore RedundantExplicitArrayCreation
        }

        [TestMethod]
        public void ToolGetPropertiesTest()
        {
            var tool = new GetAcls();

            // test parse of command line arguments
            // ReSharper disable RedundantExplicitArrayCreation
            var args = new string[]
            {
                "--app=\"C:\\windows\\system32\\icacls.exe\"", 
                "--path=C:\\",
                "--log=\"C:\\temp\\out.log\"",
                "--appendToLogFile"
            };
            // ReSharper restore RedundantExplicitArrayCreation

            var parser = new CommandLineParser(new CommandLineParserSettings(Console.Error));
            bool parsed = parser.ParseArguments(args, tool);

            Assert.IsTrue(parsed);
            Assert.IsFalse(String.IsNullOrEmpty(tool.GetCommands()));
            Assert.IsFalse(String.IsNullOrEmpty(tool.GetLogFilePath()));
            Assert.IsTrue(tool.GetAppendToLogFile());
        }
    }
}
