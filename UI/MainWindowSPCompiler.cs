using MahApps.Metro.Controls.Dialogs;
using Spedit.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;

namespace Spedit.UI
{
    public partial class MainWindow
    {
        private readonly List<string> _compiledFiles = new List<string>();
        private readonly List<string> _nonUploadedFiles = new List<string>();
        private readonly List<string> _compiledFileNames = new List<string>();
        private bool _inCompiling;

        public bool ServerIsRunning;
        public Process ServerProcess;
        public Thread ServerCheckThread;

        private async void Compile_SPScripts(bool All = true)
        {
            if (_inCompiling)
                return;

            var spCompFound = false;
            var pressedEscape = false;
            var c = Program.Configs[Program.SelectedConfig];
            FileInfo spCompInfo = null;

            Command_SaveAll();
            _inCompiling = true;
            _compiledFiles.Clear();
            _compiledFileNames.Clear();
            _nonUploadedFiles.Clear();           

            foreach (var dir in c.SMDirectories)
            {
                spCompInfo = new FileInfo(Path.Combine(dir, "spcomp.exe"));

                if (!spCompInfo.Exists)
                    continue;

                spCompFound = true;
                break;
            }

            if (spCompFound)
            {
                var filesToCompile = new List<string>();

                if (All)
                {
                    var editors = GetAllEditorElements();

                    if (editors == null)
                    {
                        _inCompiling = false;
                        return;
                    }

                    filesToCompile.AddRange(from e in editors where e.CompileBox.IsChecked != null && e.CompileBox.IsChecked.Value select e.FullFilePath);
                }
                else
                {
                    var ee = GetCurrentEditorElement();

                    if (ee == null)
                    {
                        _inCompiling = false;
                        return;
                    }

                    /*
                    ** I've struggled a bit here. Should i check, if the CompileBox is checked 
                    ** and only compile if it's checked or should it be ignored and compiled anyway?
                    ** I decided, to compile anyway but give me feedback/opinions.
                    */
                    if (ee.FullFilePath.EndsWith(".sp"))
                        filesToCompile.Add(ee.FullFilePath);
                }

                var compileCount = filesToCompile.Count;

                if (compileCount > 0)
                {
                    ErrorResultGrid.Items.Clear();
                    var progressTask = await this.ShowProgressAsync(Program.Translations.Compiling, "", false,
                        MetroDialogOptions);
                    progressTask.SetProgress(0.0);
                    var stringOutput = new StringBuilder();
                    var errorFilterRegex =
                        new Regex(
                            @"^(?<file>.+?)\((?<line>[0-9]+(\s*--\s*[0-9]+)?)\)\s*:\s*(?<type>[a-zA-Z]+\s+([a-zA-Z]+\s+)?[0-9]+)\s*:(?<details>.+)",
                            RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);

                    for (var i = 0; i < compileCount; ++i)
                    {
                        if (!_inCompiling) //pressed escape
                        {
                            pressedEscape = true;
                            break;
                        }

                        var file = filesToCompile[i];
                        progressTask.SetMessage(file);
                        ProcessUITasks();
                        var fileInfo = new FileInfo(file);
                        stringOutput.AppendLine(fileInfo.Name);

                        if (!fileInfo.Exists)
                            continue;

                        using (var process = new Process())
                        {
                            if (fileInfo.DirectoryName != null)
                            {
                                process.StartInfo.WorkingDirectory = fileInfo.DirectoryName;
                                process.StartInfo.UseShellExecute = true;
                                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                process.StartInfo.CreateNoWindow = true;
                                process.StartInfo.FileName = spCompInfo.FullName;
                                var destinationFileName = ShortenScriptFileName(fileInfo.Name) + ".smx";
                                var outFile = Path.Combine(fileInfo.DirectoryName, destinationFileName);
                                if (File.Exists(outFile))
                                    File.Delete(outFile);
                                var errorFile = Environment.CurrentDirectory + @"\sourcepawn\errorfiles\error_" +
                                                Environment.TickCount + "_" +
                                                file.GetHashCode().ToString("X") + "_" + i + ".txt";
                                if (File.Exists(errorFile))
                                    File.Delete(errorFile);

                                var includeDirectories = new StringBuilder();

                                foreach (var dir in c.SMDirectories)
                                    includeDirectories.Append(" -i=\"" + dir + "\"");

                                var includeStr = includeDirectories.ToString();

                                process.StartInfo.Arguments = "\"" + fileInfo.FullName + "\" -o=\"" + outFile +
                                                              "\" -e=\"" + errorFile + "\"" + includeStr + " -O=" +
                                                              c.OptimizeLevel + " -v=" +
                                                              c.VerboseLevel;

                                progressTask.SetProgress((i + 1 - 0.5d) / compileCount);

                                var execResult = ExecuteCommandLine(c.PreCmd, fileInfo.DirectoryName, c.CopyDirectory,
                                    fileInfo.FullName, fileInfo.Name, outFile, destinationFileName);

                                if (!string.IsNullOrWhiteSpace(execResult))
                                    stringOutput.AppendLine(execResult.Trim('\n', '\r'));

                                ProcessUITasks();

                                try
                                {
                                    process.Start();
                                    process.WaitForExit();
                                }
                                catch (Exception)
                                {
                                    _inCompiling = false;
                                }

                                if (!_inCompiling) //cannot await in catch
                                {
                                    await progressTask.CloseAsync();
                                    await this.ShowMessageAsync(Program.Translations.SPCompNotStarted,
                                        Program.Translations.Error, MessageDialogStyle.Affirmative,
                                        MetroDialogOptions);
                                    return;
                                }
                                if (File.Exists(errorFile))
                                {
                                    var errorStr = File.ReadAllText(errorFile);
                                    stringOutput.AppendLine(errorStr.Trim('\n', '\r'));
                                    var mc = errorFilterRegex.Matches(errorStr);

                                    for (var j = 0; j < mc.Count; ++j)
                                    {
                                        ErrorResultGrid.Items.Add(new ErrorDataGridRow()
                                        {
                                            File = mc[j].Groups["file"].Value.Trim(),
                                            Line = mc[j].Groups["line"].Value.Trim(),
                                            Type = mc[j].Groups["type"].Value.Trim(),
                                            Details = mc[j].Groups["details"].Value.Trim()
                                        });
                                    }

                                    File.Delete(errorFile);
                                }

                                stringOutput.AppendLine(Program.Translations.Done);

                                if (File.Exists(outFile))
                                {
                                    _compiledFiles.Add(outFile);
                                    _nonUploadedFiles.Add(outFile);
                                    _compiledFileNames.Add(destinationFileName);
                                }

                                var execResultPost = ExecuteCommandLine(c.PostCmd, fileInfo.DirectoryName,
                                    c.CopyDirectory, fileInfo.FullName, fileInfo.Name, outFile, destinationFileName);

                                if (!string.IsNullOrWhiteSpace(execResultPost))
                                    stringOutput.AppendLine(execResultPost.Trim('\n', '\r'));
                            }

                            stringOutput.AppendLine();
                            progressTask.SetProgress((double) (i + 1) / compileCount);
                            ProcessUITasks();
                        }
                    }

                    if (!pressedEscape)
                    {
                        progressTask.SetProgress(1.0);
                        CompileOutput.Text = stringOutput.ToString();

                        if (c.AutoCopy)
                            Copy_Plugins(true);

                        if (CompileOutputRow.Height.Value < 11.0)
                            CompileOutputRow.Height = new GridLength(200.0);
                    }

                    await progressTask.CloseAsync();
                }
            }
            else
                await this.ShowMessageAsync(Program.Translations.SPCompNotFound, Program.Translations.Error,
                    MessageDialogStyle.Affirmative, MetroDialogOptions);

            _inCompiling = false;
        }

        public void Copy_Plugins(bool overtakeOutString = false)
        {
            if (_compiledFiles.Count <= 0)
                return;

            _nonUploadedFiles.Clear();

            var copyCount = 0;
            var c = Program.Configs[Program.SelectedConfig];
            var stringOutput = new StringBuilder();

            if (string.IsNullOrWhiteSpace(c.CopyDirectory))
                return;

            foreach (var str in _compiledFiles)
            {
                try
                {
                    var destFile = new FileInfo(str);

                    if (!destFile.Exists)
                        continue;

                    var destinationFileName = destFile.Name;
                    var copyFileDestination = Path.Combine(c.CopyDirectory, destinationFileName);

                    File.Copy(str, copyFileDestination, true);
                    _nonUploadedFiles.Add(copyFileDestination);
                    stringOutput.AppendLine($"{Program.Translations.Copied}: " + str);
                    ++copyCount;

                    if (!c.DeleteAfterCopy)
                        continue;

                    File.Delete(str);
                    stringOutput.AppendLine($"{Program.Translations.Deleted}: " + str);
                }
                catch (Exception)
                {
                    stringOutput.AppendLine($"{Program.Translations.FailCopy}: " + str);
                }
            }

            if (copyCount == 0)
                stringOutput.AppendLine(Program.Translations.NoFilesCopy);
            if (overtakeOutString)
                CompileOutput.AppendText(stringOutput.ToString());
            else
                CompileOutput.Text = stringOutput.ToString();
            if (CompileOutputRow.Height.Value < 11.0)
                CompileOutputRow.Height = new GridLength(200.0);
        }

        public void FTPUpload_Plugins()
        {
            if (_nonUploadedFiles.Count <= 0)
                return;

            var c = Program.Configs[Program.SelectedConfig];

            if (string.IsNullOrWhiteSpace(c.FTPHost) || string.IsNullOrWhiteSpace(c.FTPUser))
                return;

            var stringOutput = new StringBuilder();

            try
            {
                var ftp = new FTP(c.FTPHost, c.FTPUser, c.FTPPassword);

                foreach (var str in _nonUploadedFiles)
                {
                    var fileInfo = new FileInfo(str);

                    if (!fileInfo.Exists)
                        continue;

                    string uploadDir;

                    if (string.IsNullOrWhiteSpace(c.FTPDir))
                        uploadDir = fileInfo.Name;
                    else
                        uploadDir = c.FTPDir.TrimEnd('/') + "/" + fileInfo.Name;

                    try
                    {
                        ftp.Upload(uploadDir, str);
                        stringOutput.AppendLine($"{Program.Translations.Uploaded}: " + str);
                    }
                    catch (Exception e)
                    {
                        stringOutput.AppendLine(string.Format(Program.Translations.ErrorUploadFile,
                            str, uploadDir));
                        stringOutput.AppendLine($"{Program.Translations.Details}: " + e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                stringOutput.AppendLine(Program.Translations.ErrorUpload);
                stringOutput.AppendLine($"{Program.Translations.Details}: " + e.Message);
            }

            stringOutput.AppendLine(Program.Translations.Done);
            CompileOutput.Text = stringOutput.ToString();

            if (CompileOutputRow.Height.Value < 11.0)
                CompileOutputRow.Height = new GridLength(200.0);
        }

        public void Server_Start()
        {
            if (ServerIsRunning)
                return;

            var c = Program.Configs[Program.SelectedConfig];
            var serverOptionsPath = c.ServerFile;

            if (string.IsNullOrWhiteSpace(serverOptionsPath))
                return;

            var serverExec = new FileInfo(serverOptionsPath);

            if (!serverExec.Exists)
                return;

            try
            {
                if (serverExec.DirectoryName != null)
                    ServerProcess = new Process
                    {
                        StartInfo =
                        {
                            UseShellExecute = true,
                            FileName = serverExec.FullName,
                            WorkingDirectory = serverExec.DirectoryName,
                            Arguments = c.ServerArgs
                        }
                    };

                ServerCheckThread = new Thread(ProcessCheckWorker);
                ServerCheckThread.Start();
            }
            catch (Exception)
            {
                ServerProcess?.Dispose();
            }
        }

        private void ProcessCheckWorker()
        {
            try
            {
                ServerProcess.Start();
            }
            catch (Exception)
            {
                return;
            }

            ServerIsRunning = true;

            Program.MainWindow.Dispatcher.Invoke(() =>
            {
                _enableServerAnim.Begin();
                UpdateWindowTitle();
            });

            ServerProcess.WaitForExit();
            ServerProcess.Dispose();

            ServerIsRunning = false;

            Program.MainWindow.Dispatcher.Invoke(() =>
            {
                if (!Program.MainWindow.IsLoaded)
                    return;

                _disableServerAnim.Begin();
                UpdateWindowTitle();
            });
        }

        private static string ShortenScriptFileName(string fileName)
        {
            return fileName.EndsWith(".sp", StringComparison.InvariantCultureIgnoreCase) ? fileName.Substring(0, fileName.Length - 3) : fileName;
        }

        private static string ExecuteCommandLine(string code, string directory, string copyDir, string scriptFile,
            string scriptName, string pluginFile, string pluginName)
        {
            code = ReplaceCmdVaraibles(code, directory, copyDir, scriptFile, scriptName, pluginFile, pluginName);

            if (string.IsNullOrWhiteSpace(code))
                return null;

            string result;
            var batchFile =
            new FileInfo(Path.Combine("sourcepawn\\temp\\",
                Environment.TickCount + "_" +
                ((uint) code.GetHashCode() ^ (uint) directory.GetHashCode()) + "_temp.bat")).FullName;

            File.WriteAllText(batchFile, code);
            
            using (var process = new Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.WorkingDirectory = directory;
                process.StartInfo.Arguments = "/c \"" + batchFile + "\"";
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                process.WaitForExit();

                using (var reader = process.StandardOutput)
                {
                    result = reader.ReadToEnd();
                }
            }

            File.Delete(batchFile);
            return result;
        }

        private static string ReplaceCmdVaraibles(string cmd, string scriptDir, string copyDir, string scriptFile,
            string scriptName, string pluginFile, string pluginName)
        {
            cmd = cmd.Replace("{editordir}", Environment.CurrentDirectory.Trim('\\'));
            cmd = cmd.Replace("{scriptdir}", scriptDir);
            cmd = cmd.Replace("{copydir}", copyDir);
            cmd = cmd.Replace("{scriptfile}", scriptFile);
            cmd = cmd.Replace("{scriptname}", scriptName);
            cmd = cmd.Replace("{pluginfile}", pluginFile);
            cmd = cmd.Replace("{pluginname}", pluginName);
            return cmd;
        }
    }

    public class ErrorDataGridRow
    {
        public string File { set; get; }
        public string Line { set; get; }
        public string Type { set; get; }
        public string Details { set; get; }
    }
}
