using Spedit.SPCondenser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml;

namespace Spedit.Interop
{
    public static class ConfigLoader
    {
        public static Config[] Load()
        {
            List<Config> configs = new List<Config>();
            if (File.Exists("sourcepawn\\configs\\Configs.xml"))
            {
                try
                {
                    XmlDocument document = new XmlDocument();
                    document.Load("sourcepawn\\configs\\Configs.xml");
                    if (document.ChildNodes.Count < 1)
                    {
                        throw new Exception("No main 'Configurations' node.");
                    }
                    XmlNode mainNode = document.ChildNodes[0];
                    if (mainNode.ChildNodes.Count < 1)
                    {
                        throw new Exception("No 'config' nodes found.");
                    }
                    for (int i = 0; i < mainNode.ChildNodes.Count; ++i)
                    {
                        XmlNode node = mainNode.ChildNodes[i];
                        string _Name = node.Attributes["Name"].Value;
                        string _SMDirectory = node.Attributes["SMDirectory"].Value;
                        string _Standard = node.Attributes["Standard"].Value;
                        bool IsStandardConfig = false;
                        if (_Standard != "0" && !string.IsNullOrWhiteSpace(_Standard))
                        {
                            IsStandardConfig = true;
                        }
                        string _CopyDirectory = node.Attributes["CopyDirectory"].Value;
                        string _ServerFile = node.Attributes["ServerFile"].Value;
                        string _ServerArgs = node.Attributes["ServerArgs"].Value;
                        string _PostCmd = node.Attributes["PostCmd"].Value;
                        string _PreCmd = node.Attributes["PreCmd"].Value;
                        int _OptimizationLevel = 2, _VerboseLevel = 1;
                        int subValue;
                        if (int.TryParse(node.Attributes["OptimizationLevel"].Value, out subValue))
                        {
                            _OptimizationLevel = subValue;
                        }
                        if (int.TryParse(node.Attributes["VerboseLevel"].Value, out subValue))
                        {
                            _VerboseLevel = subValue;
                        }
                        bool _DeleteAfterCopy = false;
                        string DeleteAfterCopyStr = node.Attributes["DeleteAfterCopy"].Value;
                        if (!(DeleteAfterCopyStr == "0" || string.IsNullOrWhiteSpace(DeleteAfterCopyStr)))
                        {
                            _DeleteAfterCopy = true;
                        }
                        string _FTPHost = node.Attributes["FTPHost"].Value;
                        string _FTPUser = node.Attributes["FTPUser"].Value;
                        string _FTPPW = node.Attributes["FTPPassword"].Value;
                        string _FTPDir = node.Attributes["FTPDir"].Value;
                        Config c = new Config()
                        {
                            Name = _Name,
                            SMDirectory = _SMDirectory,
                            Standard = IsStandardConfig
                            ,
                            CopyDirectory = _CopyDirectory,
                            ServerFile = _ServerFile,
                            ServerArgs = _ServerArgs
                            ,
                            PostCmd = _PostCmd,
                            PreCmd = _PreCmd,
                            OptimizeLevel = _OptimizationLevel,
                            VerboseLevel = _VerboseLevel,
                            DeleteAfterCopy = _DeleteAfterCopy
                            ,
                            FTPHost = _FTPHost,
                            FTPUser = _FTPUser,
                            FTPPassword = _FTPPW,
                            FTPDir = _FTPDir
                        };
                        if (IsStandardConfig)
                        {
                            c.LoadSMDef();
                        }
                        configs.Add(c);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("An error appeared while reading the configs. Without them, the editor wont start. Reinstall program!" + Environment.NewLine + "Details: " + e.Message
                        , "Error while reading configs."
                        , MessageBoxButton.OK
                        , MessageBoxImage.Warning);
                    Environment.Exit(Environment.ExitCode);
                }
            }
            else
            {
                MessageBox.Show("The Editor could not find the Configs.xml file. Without it, the editor wont start. Reinstall program.", "File not found.", MessageBoxButton.OK, MessageBoxImage.Warning);
                Environment.Exit(Environment.ExitCode);
            }
            return configs.ToArray();
        }
    }

    public class Config
    {
        public string Name = string.Empty;

        public bool Standard = false;

        public string SMDirectory = string.Empty;
        public string CopyDirectory = string.Empty;
        public string ServerFile = string.Empty;
        public string ServerArgs = string.Empty;

        public string PostCmd = string.Empty;
        public string PreCmd = string.Empty;

        public bool DeleteAfterCopy = false;

        public int OptimizeLevel = 2;
        public int VerboseLevel = 1;

        public string FTPHost = "ftp://localhost/";
        public string FTPUser = string.Empty;
        public string FTPPassword = string.Empty; //securestring? No! Because it's saved in plaintext and if you want to keep it a secret, you shouldn't automaticly uploade it anyways...
        public string FTPDir = string.Empty;

        private CondensedSourcepawnDefinition SMDef;

        public CondensedSourcepawnDefinition GetSMDef()
        {
            if (SMDef == null)
            {
                LoadSMDef();
            }
            return SMDef;
        }

        public void InvalidateSMDef()
        {
            this.SMDef = null;
        }

        public void LoadSMDef()
        {
            if (this.SMDef != null)
            {
                return;
            }
            try
            {
                this.SMDef = SourcepawnCondenser.Condense(SMDirectory);
            }
            catch (Exception)
            {
                this.SMDef = new CondensedSourcepawnDefinition(); //this could be dangerous...
            }
        }
    }
}
