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
using System.IO;
using System.Security;
using System.Security.Principal;
using CommandLineHelp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable CheckNamespace
namespace TestCommandLineHelper
// ReSharper restore CheckNamespace
{
    [TestClass]
    public class CommandLineHelperTests
    {
        #region Positive Tests
        [TestMethod]
        public void HelperInstantiationTest()
        {
            var helper = new CommandLineHelper();

            Assert.IsNotNull(helper);

            helper.SetTool(new GetAcls());
        }

        [TestMethod]
        public void HelperSetupTest()
        {
            var helper = new CommandLineHelper();
            helper.SetTool(new GetAcls());

            // ReSharper disable RedundantExplicitArrayCreation
            var args = new string[]
            {
                "--app=\"C:\\windows\\system32\\icacls.exe\"", 
                "--path=C:\\",
                "--log=\"C:\\temp\\out.log\"",
                "--appendToLogFile"
            };
            // ReSharper restore RedundantExplicitArrayCreation

            // positive test
            helper.Setup(args);

            // negative test
            try
            {
                helper.Setup(null);
                Assert.Fail("Null arguments should not be allowed.");
            }
            catch (Exception) { }
        }

        [TestMethod]
        public void HelperDoWorkTest()
        {
            var helper = new CommandLineHelper();
            helper.SetTool(new GetAcls());

            // ReSharper disable RedundantExplicitArrayCreation
            var args = new string[]
            {
                "--app=\"C:\\windows\\system32\\icacls.exe\"", 
                "--path=C:\\",
                "--log=\"C:\\temp\\out.log\"",
                "--appendToLogFile"
            };
            // ReSharper restore RedundantExplicitArrayCreation

            helper.Setup(args);
            helper.DoWork();

            ICommandLineTool tool = helper.GetTool();
            Assert.IsTrue(File.Exists(tool.GetLogFilePath()));
            File.Delete(tool.GetLogFilePath());
        }

        [TestMethod]
        [DeploymentItem("CommandLineHelp.dll")]
        public void HelperCheckSecurityTest()
        {
            var target = new CommandLineHelper_Accessor();
            WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
            target.CheckSecurity(windowsIdentity);

            // check user-level security, if desired
            //windowsIdentity = new WindowsIdentity("user@domain.com");
            //target.CheckSecurity(windowsIdentity);

            //// negative test
            //windowsIdentity = new WindowsIdentity("baduser@domain.com");
            //try
            //{
            //    target.CheckSecurity(windowsIdentity);
            //    Assert.Fail("Security check passed for unauthorized account.");
            //}
            //catch(SecurityException) { }
        }

        [TestMethod]
        [DeploymentItem("CommandLineHelp.dll")]
        public void HelperGetBatchFilePathTest()
        {
            var target = new CommandLineHelper_Accessor();

            string path = target.GetBatchFileName();
            Assert.IsFalse(String.IsNullOrEmpty(path));

            string newPath = target.GetBatchFileName();
            Assert.AreNotSame(path, newPath);
        }

        [TestMethod]
        [DeploymentItem("CommandLineHelp.dll")]
        public void HelperWriteCommandsToIsolatedStorageTest()
        {
            var target = new CommandLineHelper_Accessor();

            const string commands = "@ECHO OFF\r\nSET HOME=C:\\\r\nECHO %HOME%\r\n";
            string batchFilePath = target.WriteCommandsToIsolatedStorage(commands);

            // verify the file exists and that its contents match the commands array
            Assert.IsTrue(File.Exists(batchFilePath));
            using (var reader = new StreamReader(batchFilePath))
            {
                string input = reader.ReadToEnd();
                Assert.AreEqual(commands, input);
            }
            File.Delete(batchFilePath);
        }

        [TestMethod]
        [DeploymentItem("CommandLineHelp.dll")]
        public void HelperExecuteFileTest()
        {
            var target = new CommandLineHelper_Accessor();

            const string commands = "@ECHO OFF\r\nSET HOME=C:\\\r\nECHO %HOME%\r\n";
            string batchFilePath = String.Format("C:\\temp\\run_{0}.cmd", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            string logFilePath = String.Format("C:\\temp\\run_{0}.log", DateTime.Now.ToString("yyyyMMdd_HHmmss"));

            // write the commands to the batch file
            using (var writer = new StreamWriter(batchFilePath))
            {
                writer.Write(commands);
            }

            // run the batch file and ensure the log file has contents
            target.ExecuteFile(batchFilePath, logFilePath, false);

            Assert.IsTrue(File.Exists(logFilePath));
            using (var reader = new StreamReader(logFilePath))
            {
                string input = reader.ReadToEnd();
                Assert.AreEqual("C:\\\r\n", input);
            }

            // execute again and ensure that the log file was overwritten
            target.ExecuteFile(batchFilePath, logFilePath, false);

            Assert.IsTrue(File.Exists(logFilePath));
            using (var reader = new StreamReader(logFilePath))
            {
                string input = reader.ReadToEnd();
                Assert.AreEqual("C:\\\r\n", input);
            }

            // execute again and ensure that the log file was appended
            target.ExecuteFile(batchFilePath, logFilePath, true);

            Assert.IsTrue(File.Exists(logFilePath));
            using (var reader = new StreamReader(logFilePath))
            {
                string input = reader.ReadToEnd();
                Assert.AreEqual("C:\\\r\nC:\\\r\n", input);
            }
            // clean up
            File.Delete(logFilePath);

            // execute with a null log file, which is acceptable
            logFilePath = null;
            target.ExecuteFile(batchFilePath, logFilePath, false);

            // execute with an empty log file, which is acceptable
            logFilePath = String.Empty;
            target.ExecuteFile(batchFilePath, logFilePath, false);

            // clean up
            File.Delete(batchFilePath);
        }
        #endregion

        #region Negative Tests
        [TestMethod]
        public void HelperNegativeInstantiationTest()
        {
            var helper = new CommandLineHelper();
            try
            {
                helper.SetTool(null);
                Assert.Fail("Setting of a null ICommandLineTool should not be allowed");
            }
            catch (NullReferenceException) { }
        }

        [TestMethod]
        public void HelperNegativeSetupTest()
        {
            var helper = new CommandLineHelper();

            try
            {
                helper.Setup(new string[] { });
                Assert.Fail("Configuration of arguments prior to setting a tool should not be allowed.");
            }
            catch (InvalidOperationException) { }

            helper.SetTool(new GetAcls());
            try
            {
                helper.Setup(null);
                Assert.Fail("Configuration of null arguments should not be allowed.");
            }
            catch (ArgumentException) { }

            // ReSharper disable RedundantExplicitArrayCreation
            var args = new string[]
            {
                "--app=\"C:\\windows\\system32\\icacls.exe\"", 
                "--path=C:\\",
                "--log=\"C:\\temp\\\"",
                "--appendToLogFile"
            };
            // ReSharper restore RedundantExplicitArrayCreation

            try
            {
                helper.Setup(args);
                Assert.Fail("Missing log file name should not be allowed.");
            }
            catch (ArgumentException) { }

            // ReSharper disable RedundantExplicitArrayCreation
            args = new string[]
            {
                "--app=\"C:\\windows\\system32\\icacls.exe\"", 
                "--path=C:\\",
                "--log=\"C:\\invaliddirectoryname\\out.log\"",
                "--appendToLogFile"
            };
            // ReSharper restore RedundantExplicitArrayCreation

            try
            {
                helper.Setup(args);
                Assert.Fail("Invalid log file directory name should not be allowed.");
            }
            catch (ArgumentException) { }
        }

        [TestMethod]
        [DeploymentItem("CommandLineHelp.dll")]
        public void HelperNegativeWriteCommandsToIsolatedStorageTest()
        {
            var target = new CommandLineHelper_Accessor();

            try
            {
                target.WriteCommandsToIsolatedStorage(null);
                Assert.Fail("Null command string should not be allowed.");
            }
            catch (ArgumentException) { }

            try
            {
                target.WriteCommandsToIsolatedStorage(String.Empty);
                Assert.Fail("Empty command string should not be allowed.");
            }
            catch (ArgumentException) { }
        }

        [TestMethod]
        [DeploymentItem("CommandLineHelp.dll")]
        public void HelperNegativeExecuteFileTest()
        {
            var target = new CommandLineHelper_Accessor();

            string batchFilePath = null;
            string logFilePath = null;
            try
            {
                target.ExecuteFile(batchFilePath, logFilePath, false);
                Assert.Fail("Null file path should not be accepted.");
            }
            catch (ArgumentException) { }

            batchFilePath = String.Empty;
            logFilePath = String.Empty;
            try
            {
                target.ExecuteFile(batchFilePath, logFilePath, false);
                Assert.Fail("Empty file path should not be accepted.");
            }
            catch (ArgumentException) { }

            batchFilePath = @"C:\pagefile.sys"; // valid file
            logFilePath = @"C:\invaliddirectoryname\out.log";
            try
            {
                target.ExecuteFile(batchFilePath, logFilePath, false);
                Assert.Fail("Invalid log file directory should not be accepted.");
            }
            catch (ArgumentException) { }
        }
        #endregion
    }
}
