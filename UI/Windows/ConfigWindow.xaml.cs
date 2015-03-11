using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Spedit.Interop;
using Spedit.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml;

namespace Spedit.UI.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class ConfigWindow : MetroWindow
    {
        public ConfigWindow()
        {
            InitializeComponent();
            for (int i = 0; i < Program.Configs.Length; ++i)
            {
                ConfigListBox.Items.Add(new ListBoxItem() { Content = Program.Configs[i].Name });
            }
            ConfigListBox.SelectedIndex = Program.SelectedConfig;
        }

        private void ConfigListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadConfigToUI(ConfigListBox.SelectedIndex);
        }

        private void LoadConfigToUI(int index)
        {
            if (index < 0 || index >= Program.Configs.Length)
            {
                return;
            }
            Config c = Program.Configs[index];
            C_Name.Text = c.Name;
            C_SMDir.Text = c.SMDirectory;
            C_CopyDir.Text = c.CopyDirectory;
            C_ServerFile.Text = c.ServerFile;
            C_ServerArgs.Text = c.ServerArgs;
            C_PreBuildCmd.Text = c.PreCmd;
            C_PostBuildCmd.Text = c.PostCmd;
            C_OptimizationLevel.Value = c.OptimizeLevel;
            C_VerboseLevel.Value = c.VerboseLevel;
            C_DeleteAfterCopy.IsChecked = c.DeleteAfterCopy;
            C_FTPHost.Text = c.FTPHost;
            C_FTPUser.Text = c.FTPUser;
            C_FTPPW.Password = c.FTPPassword;
            C_FTPDir.Text = c.FTPDir;
        }

        private void NewButton_Clicked(object sender, RoutedEventArgs e)
        {
            Config c = new Config() { Name = "New Config", Standard = false, OptimizeLevel = 2, VerboseLevel = 1 };
            List<Config> configList = new List<Config>(Program.Configs);
            configList.Add(c);
            Program.Configs = configList.ToArray();
            ConfigListBox.Items.Add(new ListBoxItem() { Content = "New Config" });
        }

        private void DeleteButton_Clicked(object sender, RoutedEventArgs e)
        {
            int index = ConfigListBox.SelectedIndex;
            Config c = Program.Configs[index];
            if (c.Standard)
            {
                this.ShowMessageAsync("Cannot delete config", "You cannot delete this config!", MessageDialogStyle.Affirmative, this.MetroDialogOptions);
                return;
            }
            List<Config> configList = new List<Config>(Program.Configs);
            configList.RemoveAt(index);
            Program.Configs = configList.ToArray();
            ConfigListBox.Items.RemoveAt(index);
            if (index == Program.SelectedConfig)
            {
                Program.SelectedConfig = 0;
            }
            ConfigListBox.SelectedIndex = 0;
        }

        private void C_Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            string Name = C_Name.Text;
            Program.Configs[ConfigListBox.SelectedIndex].Name = Name;
            ((ListBoxItem)ConfigListBox.SelectedItem).Content = Name;
        }

        private void C_SMDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            Program.Configs[ConfigListBox.SelectedIndex].SMDirectory = C_SMDir.Text;
        }

        private void C_CopyDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            Program.Configs[ConfigListBox.SelectedIndex].CopyDirectory = C_CopyDir.Text;
        }

        private void C_ServerFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            Program.Configs[ConfigListBox.SelectedIndex].ServerFile = C_ServerFile.Text;
        }

        private void C_ServerArgs_TextChanged(object sender, TextChangedEventArgs e)
        {
            Program.Configs[ConfigListBox.SelectedIndex].ServerArgs = C_ServerArgs.Text;
        }

        private void C_PostBuildCmd_TextChanged(object sender, TextChangedEventArgs e)
        {
            Program.Configs[ConfigListBox.SelectedIndex].PostCmd = C_PostBuildCmd.Text;
        }

        private void C_PreBuildCmd_TextChanged(object sender, TextChangedEventArgs e)
        {
            Program.Configs[ConfigListBox.SelectedIndex].PreCmd = C_PreBuildCmd.Text;
        }

        private void C_OptimizationLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Program.Configs[ConfigListBox.SelectedIndex].OptimizeLevel = (int)C_OptimizationLevel.Value;
        }

        private void C_VerboseLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Program.Configs[ConfigListBox.SelectedIndex].VerboseLevel = (int)C_VerboseLevel.Value;
        }

        private void C_DeleteAfterCopy_Changed(object sender, RoutedEventArgs e)
        {
            Program.Configs[ConfigListBox.SelectedIndex].DeleteAfterCopy = C_DeleteAfterCopy.IsChecked.Value;
        }

        private void C_FTPHost_TextChanged(object sender, TextChangedEventArgs e)
        {
            Program.Configs[ConfigListBox.SelectedIndex].FTPHost = C_FTPHost.Text;
        }

        private void C_FTPUser_TextChanged(object sender, TextChangedEventArgs e)
        {
            Program.Configs[ConfigListBox.SelectedIndex].FTPUser = C_FTPUser.Text;
        }

        private void C_FTPPW_TextChanged(object sender, RoutedEventArgs e)
        {
            Program.Configs[ConfigListBox.SelectedIndex].FTPPassword = C_FTPPW.Password;
        }

        private void C_FTPDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            Program.Configs[ConfigListBox.SelectedIndex].FTPDir = C_FTPDir.Text;
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            for (int i = 0; i < Program.Configs.Length; ++i)
            {
                Program.Configs[i].InvalidateSMDef();
            }
            Program.MainWindow.FillConfigMenu();
            Program.MainWindow.ChangeConfig(Program.SelectedConfig);
            StringBuilder outString = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings() { Indent = true, IndentChars = "\t", NewLineOnAttributes = false, OmitXmlDeclaration = true };
            using (XmlWriter writer = XmlWriter.Create(outString, settings))
            {
                writer.WriteStartElement("Configurations");
                for (int i = 0; i < Program.Configs.Length; ++i)
                {
                    Config c = Program.Configs[i];
                    writer.WriteStartElement("Config");
                    writer.WriteAttributeString("Name", c.Name);
                    writer.WriteAttributeString("SMDirectory", c.SMDirectory);
                    writer.WriteAttributeString("Standard", (c.Standard) ? "1" : "0");
                    writer.WriteAttributeString("CopyDirectory", c.CopyDirectory);
                    writer.WriteAttributeString("ServerFile", c.ServerFile);
                    writer.WriteAttributeString("ServerArgs", c.ServerArgs);
                    writer.WriteAttributeString("PostCmd", c.PostCmd);
                    writer.WriteAttributeString("PreCmd", c.PreCmd);
                    writer.WriteAttributeString("OptimizationLevel", c.OptimizeLevel.ToString());
                    writer.WriteAttributeString("VerboseLevel", c.VerboseLevel.ToString());
                    writer.WriteAttributeString("DeleteAfterCopy", (c.DeleteAfterCopy) ? "1" : "0");
                    writer.WriteAttributeString("FTPHost", c.FTPHost.ToString());
                    writer.WriteAttributeString("FTPUser", c.FTPUser.ToString());
                    writer.WriteAttributeString("FTPPassword", ManagedAES.Encrypt(c.FTPPassword.ToString()));
                    writer.WriteAttributeString("FTPDir", c.FTPDir.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.Flush();
            }
            File.WriteAllText("sourcepawn\\configs\\Configs.xml", outString.ToString());
        }

    }
}
