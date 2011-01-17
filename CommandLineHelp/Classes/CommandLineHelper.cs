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
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Security;
using System.Security.Principal;
using CommandLine;

// ReSharper disable CheckNamespace
namespace CommandLineHelp
// ReSharper restore CheckNamespace
{
    public class CommandLineHelper
    {
        #region Implementation of Strategy Pattern
        private ICommandLineTool _tool;
        private string _batchFileName;

        public void SetTool(ICommandLineTool tool)
        {
            if (tool == null)
                throw new NullReferenceException("ICommandLineTool value should not be null.");

            _tool = tool;
        }

        public ICommandLineTool GetTool()
        {
            return _tool;
        }
        #endregion

        #region Public Methods
        public void Setup(string[] args)
        {
            if (_tool == null)
                throw new InvalidOperationException(
                    "Attempt to parse arguments prior to the assignment of an ICommandLineTool.  " +
                    "Please call SetTool() prior to calling this method.");

            // parse command line arguments
            var parser = new CommandLineParser(new CommandLineParserSettings(null));
            bool parsed = parser.ParseArguments(args, _tool);
            if (!parsed)
                throw new ArgumentException(_tool.GetUsage());

            // ensure destination directory for log file exists
            string logFilePath = _tool.GetLogFilePath();
            if (logFilePath.Length != 0)
            {
                if (!Directory.Exists(Path.GetDirectoryName(logFilePath)))
                    throw new ArgumentException(
                        String.Format("Invalid directory: {0}", Path.GetDirectoryName(logFilePath)));

                if (Path.GetFileName(logFilePath).Length == 0)
                    throw new ArgumentException(
                        String.Format("Missing log file name.\r\n\r\n{0}", _tool.GetUsage()));
            }

            // implement group membership checks
            CheckSecurity(WindowsIdentity.GetCurrent());
        }

        public void DoWork()
        {
            string batchFilePath = WriteCommandsToIsolatedStorage(_tool.GetCommands());

            ExecuteFile(batchFilePath, _tool.GetLogFilePath(), _tool.GetAppendToLogFile());
            
            Cleanup();
        }
        #endregion

        #region Private Methods
        // ReSharper disable MemberCanBeMadeStatic.Local
        private void CheckSecurity(WindowsIdentity windowsIdentity)
        // ReSharper restore MemberCanBeMadeStatic.Local
        {
            if (windowsIdentity == null || !windowsIdentity.IsAuthenticated)
                throw new SecurityException(
                    "User is not current authenticated.  Unable to check security credentials.");

            // check group-level security, if desired
            //var windowsPrincipal = new WindowsPrincipal(windowsIdentity);

            //if (windowsPrincipal == null || !windowsPrincipal.IsInRole(@"Domain\Group"))
            //    throw new SecurityException(
            //        String.Format("User: {0} is not a member of Domain\\Group.",
            //                      windowsIdentity.Name));
        }

        private string WriteCommandsToIsolatedStorage(string commands)
        {
            if (String.IsNullOrEmpty(commands))
                throw new ArgumentException("No commands found to save.");

            _batchFileName = GetBatchFileName();
            string isolatedStoragePath;

            using (var isolatedStorageFile = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                // write commands to a batch file in isolated storage
                using (var stream = new IsolatedStorageFileStream(_batchFileName, FileMode.Create, isolatedStorageFile))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(commands);
                    }
                }

                // get the path to isolated storage to return to the caller - primarily for unit testing
                Type isolatedStorageType = isolatedStorageFile.GetType();
                PropertyInfo rootDirectory = isolatedStorageType.GetProperty("RootDirectory", BindingFlags.NonPublic | BindingFlags.Instance);
                isolatedStoragePath = rootDirectory.GetValue(isolatedStorageFile, null).ToString();
            }

            return Path.Combine(isolatedStoragePath, _batchFileName);
        }

        // ReSharper disable MemberCanBeMadeStatic.Local
        private string GetBatchFileName()
        // ReSharper restore MemberCanBeMadeStatic.Local
        {
            return String.Format("run_{0}.cmd", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        }

        // ReSharper disable MemberCanBeMadeStatic.Local
        private void ExecuteFile(string filePath, string logFilePath = null, bool appendToLogFile = false)
        // ReSharper restore MemberCanBeMadeStatic.Local
        {
            if (String.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty.");

            if (!File.Exists(filePath))
                throw new ArgumentException(
                    String.Format("Invalid file path: {0}", filePath));

            if (!String.IsNullOrEmpty(logFilePath))
            {
                if (!Directory.Exists(Path.GetDirectoryName(logFilePath)))
                    throw new ArgumentException(
                        String.Format("Invalid directory: {0}", Path.GetDirectoryName(logFilePath)));

                if (Path.GetFileName(logFilePath).Length == 0)
                    throw new ArgumentException("Missing log file name.");
            }

            using (var process = new Process())
            {
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.FileName = filePath;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();

                if (!String.IsNullOrEmpty(logFilePath))
                {
                    using (var writer = new StreamWriter(logFilePath, appendToLogFile))
                    {
                        writer.Write(process.StandardOutput.ReadToEnd());
                        writer.Write(process.StandardError.ReadToEnd());
                    }
                }
                process.WaitForExit();
            }
        }

        private void Cleanup()
        {
            using (var isolatedStoragFile = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                if (isolatedStoragFile.FileExists(_batchFileName))
                    isolatedStoragFile.DeleteFile(_batchFileName);
            }
        }
        #endregion
    }
}