using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Spedit.UI.Components;
using System.Globalization;

namespace Spedit.UI
{
    public partial class MainWindow : MetroWindow
    {
        private bool InCompiling = false;
        private async void Compile_SPScripts(bool Copy)
        {
            Command_SaveAll();
            if (InCompiling) { return; }
            InCompiling = true;
            var c = Program.Configs[Program.SelectedConfig];
            //string spCompPath = Path.Combine(Environment.CurrentDirectory + @"\sourcepawn\spcomp.exe");
            FileInfo spCompInfo = new FileInfo(Path.Combine(c.SMDirectory, "spcomp.exe"));
            if (spCompInfo.Exists)
            {
                List<string> filesToCompile = new List<string>();
                EditorElement[] editors = GetAllEditorElements();
                for (int i = 0; i < editors.Length; ++i)
                {
                    if (editors[i].CompileBox.IsChecked.Value)
                    {
                        filesToCompile.Add(editors[i].FullFilePath);
                    }
                }
                int compileCount = filesToCompile.Count;
                if (compileCount > 0)
                {
                    var conf = Program.Configs[Program.SelectedConfig];
                    var progressTask = await this.ShowProgressAsync("Compiling", "", false, this.MetroDialogOptions);
                    progressTask.SetProgress(0.0);
                    StringBuilder stringOutput = new StringBuilder();
                    for (int i = 0; i < compileCount; ++i)
                    {
                        string file = filesToCompile[i];
                        progressTask.SetMessage(file);
                        MainWindow.ProcessUITasks();
                        FileInfo fileInfo = new FileInfo(file);
                        stringOutput.AppendLine(fileInfo.Name);
                        if (fileInfo.Exists)
                        {
                            string execResult = ExecuteCommandLine(conf.PreCmd, fileInfo.DirectoryName);
                            if (!string.IsNullOrWhiteSpace(execResult))
                            {
                                stringOutput.AppendLine(execResult.Trim(new char[] { '\n', '\r' }));
                            }
                            using (Process process = new Process())
                            {
                                process.StartInfo.WorkingDirectory = Environment.CurrentDirectory + @"\sourcepawn";
                                process.StartInfo.UseShellExecute = true;
                                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                process.StartInfo.CreateNoWindow = true;
                                process.StartInfo.FileName = spCompInfo.FullName;
                                string destinationFileName = ShortenScriptFileName(fileInfo.Name) + ".smx";
                                string outFile = Path.Combine(fileInfo.DirectoryName, destinationFileName);
                                if (File.Exists(outFile)) { File.Delete(outFile); }
                                string errorFile = Environment.CurrentDirectory + @"\sourcepawn\errorfiles\error_" + Environment.TickCount.ToString() + "_" + file.GetHashCode().ToString("X") + "_" + i.ToString() + ".txt";
                                if (File.Exists(errorFile)) { File.Delete(errorFile); }
                                process.StartInfo.Arguments = "\"" + fileInfo.FullName + "\" -o=\"" + outFile + "\" -e=\"" + errorFile + "\" -i=" + c.SMDirectory + " -O=" + c.OptimizeLevel.ToString() + " -v=" + c.VerboseLevel.ToString();
                                progressTask.SetProgress((((double)(i + 1)) - 0.5d) / ((double)compileCount));
                                MainWindow.ProcessUITasks();
                                process.Start();
                                process.WaitForExit();
                                if (File.Exists(errorFile))
                                {
                                    stringOutput.AppendLine(File.ReadAllText(errorFile).Trim(new char[] { '\n', '\r' } ));
                                    File.Delete(errorFile);
                                }
                                else
                                {
                                    stringOutput.AppendLine("No Errors");
                                }
                                if (Copy)
                                {
                                    if (File.Exists(outFile))
                                    {
                                        try
                                        {
                                            string copyFileDestination = Path.Combine(c.CopyDirectory, destinationFileName);
                                            File.Copy(outFile, copyFileDestination, true);
                                            stringOutput.AppendLine("Plugin copied.");
                                        }
                                        catch (Exception) { }
                                    }
                                }
                                stringOutput.AppendLine();
                                progressTask.SetProgress(((double)(i + 1)) / ((double)compileCount));
                                MainWindow.ProcessUITasks();
                            }
                            execResult = ExecuteCommandLine(conf.PostCmd, fileInfo.DirectoryName);
                            if (!string.IsNullOrWhiteSpace(execResult))
                            {
                                stringOutput.AppendLine(execResult.Trim(new char[] { '\n', '\r' }));
                            }
                        }
                    }
                    progressTask.SetProgress(1.0);
                    CompileOutput.Text = stringOutput.ToString();
                    if (CompileOutputRow.Height.Value < 11.0)
                    {
                        CompileOutputRow.Height = new GridLength(200.0);
                    }
                    await progressTask.CloseAsync();

                }
            }
            else
            {
                await this.ShowMessageAsync("The 'spcomp.exe' compiler could not be found.", "Error", MessageDialogStyle.Affirmative, this.MetroDialogOptions);
            }
            InCompiling = false;
        }


        public async void Server_Start()
        {
            var c = Program.Configs[Program.SelectedConfig];
            string serverOptionsPath = c.ServerFile;
            if (string.IsNullOrWhiteSpace(serverOptionsPath))
            {
                return;
            }
            FileInfo serverExec = new FileInfo(serverOptionsPath);
            if (!serverExec.Exists)
            {
                return;
            }
            using (Process process = new Process())
            {
                var progressTask = await this.ShowProgressAsync("Running local server", "Waiting for server to terminating.", false, this.MetroDialogOptions);
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = serverExec.FullName;
                process.StartInfo.WorkingDirectory = serverExec.DirectoryName;
                process.StartInfo.Arguments = c.ServerArgs;
                process.Start();
                process.WaitForExit();
                await progressTask.CloseAsync();
            }
        }


        private string ShortenScriptFileName(string fileName)
        {
            if (fileName.EndsWith(".sp", StringComparison.InvariantCultureIgnoreCase))
            {
                return fileName.Substring(0, fileName.Length - 3);
            }
            return fileName;
        }

        private string ExecuteCommandLine(string code, string directory)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }
            string batchFile = (new FileInfo(Path.Combine("sourcepawn\\temp\\", Environment.TickCount.ToString() + "_" + (code.GetHashCode() ^ directory.GetHashCode()).ToString() + "_temp.bat"))).FullName;
            File.WriteAllText(batchFile, code);
            string result = null;
            using (Process process = new Process())
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
                using (StreamReader reader = process.StandardOutput)
                {
                    result = reader.ReadToEnd();
                }
            }
            File.Delete(batchFile);
            return result;
        }
    }
}
