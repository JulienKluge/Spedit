using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Diagnostics;
using MahApps.Metro.Controls;
using System.Xml;
using System.IO;
using Microsoft.Win32;

namespace Spedit.UI.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class NewFileWindow : MetroWindow
    {
        string PathStr = "sourcepawn\\scripts";
        Dictionary<string, TemplateInfo> TemplateDictionary;
        public NewFileWindow()
        {
            InitializeComponent();
            ParseTemplateFile();
            TemplateListBox.SelectedIndex = 0;
        }

        private void ParseTemplateFile()
        {
            TemplateDictionary = new Dictionary<string, TemplateInfo>();
            if (File.Exists("sourcepawn\\templates\\Templates.xml"))
            {
                using (Stream stream = File.OpenRead("sourcepawn\\templates\\Templates.xml"))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(stream);
                    if (doc.ChildNodes.Count > 0)
                    {
                        if (doc.ChildNodes[0].Name == "Templates")
                        {
                            XmlNode mainNode = doc.ChildNodes[0];
                            for (int i = 0; i < mainNode.ChildNodes.Count; ++i)
                            {
                                if (mainNode.ChildNodes[i].Name == "Template")
                                {
                                    XmlAttributeCollection attributes = mainNode.ChildNodes[i].Attributes;
                                    string NameStr = attributes["Name"].Value;
                                    string FileNameStr = attributes["File"].Value;
                                    string NewNameStr = attributes["NewName"].Value;
                                    string FilePathStr = Path.Combine("sourcepawn\\templates\\", FileNameStr);
                                    if (File.Exists(FilePathStr))
                                    {
                                        TemplateDictionary.Add(NameStr, new TemplateInfo() { Name = NameStr, FileName = FileNameStr, Path = FilePathStr, NewName = NewNameStr });
                                        TemplateListBox.Items.Add(NameStr);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void TemplateListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TemplateInfo templateInfo = TemplateDictionary[(string)TemplateListBox.SelectedItem];
            PrevieBox.Text = File.ReadAllText(templateInfo.Path);
            PathBox.Text = Path.Combine(PathStr, templateInfo.NewName);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog() { AddExtension = true, Filter = @"Sourcepawn Files (*.sp *.inc)|*.sp;*.inc|All Files (*.*)|*.*", OverwritePrompt = true, Title = "New File" };
            sfd.ShowDialog(this);
            if (!string.IsNullOrWhiteSpace(sfd.FileName))
            {
                FileInfo fileInfo = new FileInfo(sfd.FileName);
                PathStr = fileInfo.DirectoryName;
                PathBox.Text = fileInfo.FullName;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            FileInfo destFile = new FileInfo(PathBox.Text);
            TemplateInfo templateInfo = TemplateDictionary[(string)TemplateListBox.SelectedItem];
            File.Copy(templateInfo.Path, destFile.FullName, true);
            Program.MainWindow.TryLoadSourceFile(destFile.FullName);
            this.Close();
        }
    }

    public class TemplateInfo
    {
        public string Name;
        public string FileName;
        public string Path;
        public string NewName;
    }
}
