using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using Spedit.Interop;
using QueryMaster;

namespace Spedit.UI
{
    public partial class MainWindow
    {
        public void Server_Query()
        {
            Config c = Program.Configs[Program.SelectedConfig];
            if (string.IsNullOrWhiteSpace(c.RConIP) || string.IsNullOrWhiteSpace(c.RConCommands))
            { return; }
            StringBuilder stringOutput = new StringBuilder();
            try
            {
                EngineType type = EngineType.GoldSource;
                if (c.RConUseSourceEngine)
                {
                    type = EngineType.Source;
                }
                using (Server server = ServerQuery.GetServerInstance(type, c.RConIP, c.RConPort, null))
                {
                    var serverInfo = server.GetInfo();
                    stringOutput.AppendLine(serverInfo.Name);
                    using (var rcon = server.GetControl(c.RConPassword))
                    {
                        string[] cmds = ReplaceRconCMDVaraibles(c.RConCommands).Split('\n');
                        for (int i = 0; i < cmds.Length; ++i)
                        {
                            string command = (cmds[i].Trim(new char[] { '\r' })).Trim();
                            if (!string.IsNullOrWhiteSpace(command))
                            {
                                stringOutput.AppendLine(rcon.SendCommand(command));
                            }
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
            {
                CompileOutputRow.Height = new GridLength(200.0);
            }
        }

        private string ReplaceRconCMDVaraibles(string input)
        {
            if (compiledFileNames.Count < 1)
            { return input; }
            if (input.IndexOf("{plugins_reload}") >= 0)
            {
                StringBuilder replacement = new StringBuilder();
                replacement.AppendLine();
                for (int i = 0; i < compiledFileNames.Count; ++i)
                {
                    replacement.Append("sm plugins reload " + StripSMXPostFix(compiledFileNames[i]) + ";");
                }
                replacement.AppendLine();
                input = input.Replace("{plugins_reload}", replacement.ToString());
            }
            if (input.IndexOf("{plugins_load}") >= 0)
            {
                StringBuilder replacement = new StringBuilder();
                replacement.AppendLine();
                for (int i = 0; i < compiledFileNames.Count; ++i)
                {
                    replacement.Append("sm plugins load " + StripSMXPostFix(compiledFileNames[i]) + ";");
                }
                replacement.AppendLine();
                input = input.Replace("{plugins_load}", replacement.ToString());
            }
            if (input.IndexOf("{plugins_unload}") >= 0)
            {
                StringBuilder replacement = new StringBuilder();
                replacement.AppendLine();
                for (int i = 0; i < compiledFileNames.Count; ++i)
                {
                    replacement.Append("sm plugins unload " + StripSMXPostFix(compiledFileNames[i]) + ";");
                }
                replacement.AppendLine();
                input = input.Replace("{plugins_unload}", replacement.ToString());
            }
            return input;
        }

        private string StripSMXPostFix(string fileName)
        {
            if (fileName.EndsWith(".smx"))
            {
                return fileName.Substring(0, fileName.Length - 4);
            }
            return fileName;
        }
    }
}
