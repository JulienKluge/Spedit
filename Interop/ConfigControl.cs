using SourcepawnCondenser.SourcemodDefinition;
using Spedit.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;

namespace Spedit.Interop
{
    public static class ConfigLoader
    {
        public static Config[] Load()
        {
            var configs = new List<Config>();

            if (File.Exists("sourcepawn\\configs\\Configs.xml"))
            {
                try
                {
                    var document = new XmlDocument();

                    document.Load("sourcepawn\\configs\\Configs.xml");

                    if (document.ChildNodes.Count < 1)
                        throw new Exception("No main 'Configurations' node.");

                    var mainNode = document.ChildNodes[0];

                    if (mainNode.ChildNodes.Count < 1)
                        throw new Exception("No 'config' nodes found.");

                    for (var i = 0; i < mainNode.ChildNodes.Count; ++i)
                    {
                        var node = mainNode.ChildNodes[i];
                        var name = ReadAttributeStringSafe(ref node, "Name", "UNKOWN CONFIG " + (i + 1));
                        var smDirectoryStr = ReadAttributeStringSafe(ref node, "SMDirectory", "");
                        var smDirectoriesSplitted = smDirectoryStr.Split(';');
                        var standard = ReadAttributeStringSafe(ref node, "Standard", "0");
                        var isStandardConfig = standard != "0" && !string.IsNullOrWhiteSpace(standard);
                        var autoCopyStr = ReadAttributeStringSafe(ref node, "AutoCopy", "0");
                        var autoCopy = autoCopyStr != "0" && !string.IsNullOrWhiteSpace(autoCopyStr);
                        var copyDirectory = ReadAttributeStringSafe(ref node, "CopyDirectory");
                        var serverFile = ReadAttributeStringSafe(ref node, "ServerFile");
                        var serverArgs = ReadAttributeStringSafe(ref node, "ServerArgs");
                        var postCmd = ReadAttributeStringSafe(ref node, "PostCmd");
                        var preCmd = ReadAttributeStringSafe(ref node, "PreCmd");
                        int optimizationLevel = 2, verboseLevel = 1;
                        int subValue;

                        if (int.TryParse(ReadAttributeStringSafe(ref node, "OptimizationLevel", "2"), out subValue))
                            optimizationLevel = subValue;

                        if (int.TryParse(ReadAttributeStringSafe(ref node, "VerboseLevel", "1"), out subValue))
                            verboseLevel = subValue;

                        var deleteAfterCopy = false;
                        var deleteAfterCopyStr = ReadAttributeStringSafe(ref node, "DeleteAfterCopy", "0");

                        if (!(deleteAfterCopyStr == "0" || string.IsNullOrWhiteSpace(deleteAfterCopyStr)))
                            deleteAfterCopy = true;

                        var ftpHost = ReadAttributeStringSafe(ref node, "FTPHost", "ftp://localhost/");
                        var ftpUser = ReadAttributeStringSafe(ref node, "FTPUser");
                        var encryptedFtppw = ReadAttributeStringSafe(ref node, "FTPPassword");
                        var ftppw = ManagedAES.Decrypt(encryptedFtppw);
                        var ftpDir = ReadAttributeStringSafe(ref node, "FTPDir");
                        var rConEngineSourceStr = ReadAttributeStringSafe(ref node, "RConSourceEngine", "1");
                        var rConEngineTypeSource =
                            !(rConEngineSourceStr == "0" || string.IsNullOrWhiteSpace(rConEngineSourceStr));
                        var rConIP = ReadAttributeStringSafe(ref node, "RConIP", "127.0.0.1");
                        var rConPortStr = ReadAttributeStringSafe(ref node, "RConPort", "27015");
                        ushort rConPort;

                        if (!ushort.TryParse(rConPortStr, NumberStyles.Any, CultureInfo.InvariantCulture, out rConPort))
                            rConPort = 27015;

                        var encryptedRConPassword = ReadAttributeStringSafe(ref node, "RConPassword");
                        var rConPassword = ManagedAES.Decrypt(encryptedRConPassword);
                        var rConCommands = ReadAttributeStringSafe(ref node, "RConCommands");
                        var c = new Config
                        {
                            Name = name,
                            SMDirectories =
                                smDirectoriesSplitted.Select(dir => dir.Trim())
                                    .Where(d => Directory.Exists(d))
                                    .ToArray(),
                            Standard = isStandardConfig,
                            AutoCopy = autoCopy,
                            CopyDirectory = copyDirectory,
                            ServerFile = serverFile,
                            ServerArgs = serverArgs,
                            PostCmd = postCmd,
                            PreCmd = preCmd,
                            OptimizeLevel = optimizationLevel,
                            VerboseLevel = verboseLevel,
                            DeleteAfterCopy = deleteAfterCopy,
                            FTPHost = ftpHost,
                            FTPUser = ftpUser,
                            FTPPassword = ftppw,
                            FTPDir = ftpDir,
                            RConUseSourceEngine = rConEngineTypeSource,
                            RConIP = rConIP,
                            RConPort = rConPort,
                            RConPassword = rConPassword,
                            RConCommands = rConCommands
                        };

                        if (isStandardConfig)
                            c.LoadSMDef();

                        configs.Add(c);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(
                        "An error appeared while reading the configs. Without them, the editor wont start. Reinstall program!" +
                        Environment.NewLine + "Details: " + e.Message
                        , "Error while reading configs."
                        , MessageBoxButton.OK
                        , MessageBoxImage.Warning);
                    Environment.Exit(Environment.ExitCode);
                }
            }
            else
            {
                MessageBox.Show(
                    "The Editor could not find the Configs.xml file. Without it, the editor wont start. Reinstall program.",
                    "File not found.", MessageBoxButton.OK, MessageBoxImage.Warning);
                Environment.Exit(Environment.ExitCode);
            }

            return configs.ToArray();
        }

        private static string ReadAttributeStringSafe(ref XmlNode node, string attributeName, string defaultValue = "")
        {
            if (node?.Attributes == null)
                return null;

            for (var i = 0; i < node.Attributes.Count; ++i)
                if (node.Attributes[i].Name == attributeName)
                    return node.Attributes[i].Value;

            return defaultValue;
        }
    }

    public class Config
    {
        private SMDefinition _smDef;

        public string Name = string.Empty;
        public bool Standard;
        public bool AutoCopy;
        public string[] SMDirectories = new string[0];
        public string CopyDirectory = string.Empty;
        public string ServerFile = string.Empty;
        public string ServerArgs = string.Empty;
        public string PostCmd = string.Empty;
        public string PreCmd = string.Empty;
        public bool DeleteAfterCopy;
        public int OptimizeLevel = 2;
        public int VerboseLevel = 1;
        public string FTPHost = "ftp://localhost/";
        public string FTPUser = string.Empty;
        public string FTPPassword = string.Empty; //securestring? No! Because it's saved in plaintext and if you want to keep it a secret, you shouldn't automaticly uploade it anyways...
        public string FTPDir = string.Empty;
        public bool RConUseSourceEngine = true;
        public string RConIP = "127.0.0.1";
        public ushort RConPort = 27015;
        public string RConPassword = string.Empty;
        public string RConCommands = string.Empty;

        public SMDefinition GetSMDef()
        {
            if (_smDef == null)
                LoadSMDef();

            return _smDef;
        }

        public void InvalidateSMDef()
        {
            _smDef = null;
        }

        public void LoadSMDef()
        {
            if (_smDef != null)
                return;

            try
            {
				var def = new SMDefinition();
				def.AppendFiles(SMDirectories);
				_smDef = def;
            }
            catch (Exception)
            {
                _smDef = new SMDefinition(); //this could be dangerous...
            }
        }
    }
}
