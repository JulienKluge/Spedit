using System;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using QueryMaster;

namespace Spedit.UI
{
    public partial class MainWindow
    {
        public void ServerQuery()
        {
            var c = Program.Configs[Program.SelectedConfig];
            var stringOutput = new StringBuilder();

            if (string.IsNullOrWhiteSpace(c.RConIP) || string.IsNullOrWhiteSpace(c.RConCommands))
                return;       

            try
            {
                var type = EngineType.GoldSource;

                if (c.RConUseSourceEngine)
                    type = EngineType.Source;

                using (var server = QueryMaster.ServerQuery.GetServerInstance(type, c.RConIP, c.RConPort, null))
                {
                    var serverInfo = server.GetInfo();
                    stringOutput.AppendLine(serverInfo.Name);

                    using (var rcon = server.GetControl(c.RConPassword))
                    {
                        var cmds = ReplaceRconCmdVaraibles(c.RConCommands).Split('\n');
                        foreach (var str in cmds)
                        {
                            var t = Task.Run(() =>
                            {
                                var command = str.Trim('\r').Trim();

                                if (string.IsNullOrWhiteSpace(command))
                                    return;

                                if (rcon != null)
                                    stringOutput.AppendLine(rcon.SendCommand(command));
                            });

                            t.Wait();
                        }
                    }
                }

                stringOutput.AppendLine("Done");
            }
            catch (Exception e)
            {
                stringOutput.AppendLine("Error: " + e.Message);
            }

            CompileOutput.Text = stringOutput.ToString();

            if (CompileOutputRow.Height.Value < 11.0)
                CompileOutputRow.Height = new GridLength(200.0);
        }

        private string ReplaceRconCmdVaraibles(string input)
        {
            if (_compiledFileNames.Count < 1)
                return input;

            if (input.IndexOf("{plugins_reload}", StringComparison.Ordinal) >= 0)
            {
                var replacement = new StringBuilder();
                replacement.AppendLine();

                foreach (var str in _compiledFileNames)
                    replacement.Append("sm plugins reload " + StripSmxPostFix(str) + ";");

                replacement.AppendLine();
                input = input.Replace("{plugins_reload}", replacement.ToString());
            }

            if (input.IndexOf("{plugins_load}", StringComparison.Ordinal) >= 0)
            {
                var replacement = new StringBuilder();
                replacement.AppendLine();

                foreach (var str in _compiledFileNames)
                    replacement.Append("sm plugins load " + StripSmxPostFix(str) + ";");

                replacement.AppendLine();
                input = input.Replace("{plugins_load}", replacement.ToString());
            }

            if (input.IndexOf("{plugins_unload}", StringComparison.Ordinal) < 0)
                return input;

            {
                var replacement = new StringBuilder();
                replacement.AppendLine();

                foreach (var str in _compiledFileNames)
                    replacement.Append("sm plugins unload " + StripSmxPostFix(str) + ";");

                replacement.AppendLine();
                input = input.Replace("{plugins_unload}", replacement.ToString());
            }

            return input;
        }

        private static string StripSmxPostFix(string fileName)
        {
            return fileName.EndsWith(".smx") ? fileName.Substring(0, fileName.Length - 4) : fileName;
        }
    }
}
