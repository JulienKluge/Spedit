using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using MahApps.Metro;
using Microsoft.Win32;

namespace Spedit.UI.Windows
{
    /// <summary>
    ///     Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class NewFileWindow
    {
        private readonly string PathStr = "sourcepawn\\scripts";
        private Dictionary<string, TemplateInfo> _templateDictionary;
        private ICommand _textBoxButtonFileCmd;

        public NewFileWindow()
        {
            InitializeComponent();
            Language_Translate();

            if (Program.OptionsObject.ProgramAccentColor != "Red" || Program.OptionsObject.ProgramTheme != "BaseDark")
                ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(Program.OptionsObject.ProgramAccentColor),
                    ThemeManager.GetAppTheme(Program.OptionsObject.ProgramTheme));

            ParseTemplateFile();
            TemplateListBox.SelectedIndex = 0;
        }

        public ICommand TextBoxButtonFileCmd
        {
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
            }

            get
            {
                if (_textBoxButtonFileCmd != null)
                    return _textBoxButtonFileCmd;

                var cmd = new SimpleCommand
                {
                    CanExecutePredicate = o => true,
                    ExecuteAction = o =>
                    {
                        if (!(o is TextBox))
                            return;

                        var dialog = new SaveFileDialog
                        {
                            AddExtension = true,
                            Filter = "Sourcepawn Files (*.sp *.inc)|*.sp;*.inc|All Files (*.*)|*.*",
                            OverwritePrompt = true,
                            Title = Program.Translations.NewFile
                        };

                        var result = dialog.ShowDialog();

                        if (result != null && result.Value)
                            ((TextBox) o).Text = dialog.FileName;
                    }
                };

                _textBoxButtonFileCmd = cmd;
                return cmd;
            }
        }

        private void ParseTemplateFile()
        {
            _templateDictionary = new Dictionary<string, TemplateInfo>();

            if (!File.Exists("sourcepawn\\templates\\Templates.xml"))
                return;

            using (Stream stream = File.OpenRead("sourcepawn\\templates\\Templates.xml"))
            {
                var doc = new XmlDocument();
                doc.Load(stream);

                if (doc.ChildNodes.Count <= 0)
                    return;

                if (doc.ChildNodes[0].Name != "Templates")
                    return;

                var mainNode = doc.ChildNodes[0];

                for (var i = 0; i < mainNode.ChildNodes.Count; ++i)
                    if (mainNode.ChildNodes[i].Name == "Template")
                    {
                        var attributes = mainNode.ChildNodes[i].Attributes;

                        if (attributes == null)
                            continue;

                        var nameStr = attributes["Name"].Value;
                        var fileNameStr = attributes["File"].Value;
                        var newNameStr = attributes["NewName"].Value;
                        var filePathStr = Path.Combine("sourcepawn\\templates\\", fileNameStr);

                        if (!File.Exists(filePathStr))
                            continue;

                        _templateDictionary.Add(nameStr,
                            new TemplateInfo
                            {
                                Name = nameStr,
                                FileName = fileNameStr,
                                Path = filePathStr,
                                NewName = newNameStr
                            });

                        TemplateListBox.Items.Add(nameStr);
                    }
            }
        }

        private void TemplateListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var templateInfo = _templateDictionary[(string) TemplateListBox.SelectedItem];
            PrevieBox.Text = File.ReadAllText(templateInfo.Path);
            PathBox.Text = Path.Combine(PathStr, templateInfo.NewName);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var destFile = new FileInfo(PathBox.Text);
            var templateInfo = _templateDictionary[(string) TemplateListBox.SelectedItem];

            File.Copy(templateInfo.Path, destFile.FullName, true);
            Program.MainWindow.TryLoadSourceFile(destFile.FullName, true, true, true);

            Close();
        }

        private void Language_Translate()
        {
            if (Program.Translations.IsDefault)
                return;

            PreviewBlock.Text = $"{Program.Translations.Preview}:";
            SaveButton.Content = Program.Translations.Save;
        }

        private class SimpleCommand : ICommand
        {
            public Predicate<object> CanExecutePredicate { get; set; }
            public Action<object> ExecuteAction { private get; set; }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public void Execute(object parameter)
            {
                ExecuteAction?.Invoke(parameter);
            }
        }
    }

    public class TemplateInfo
    {
        public string FileName;
        public string Name;
        public string NewName;
        public string Path;
    }
}