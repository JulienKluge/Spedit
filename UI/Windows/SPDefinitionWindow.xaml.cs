using MahApps.Metro.Controls;
using Spedit.SPCondenser;
using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Spedit.UI.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class SPDefinitionWindow : MetroWindow
    {
        CondensedSourcepawnDefinition def;

        SPDefEntry[] defArray;
        ListViewItem[] items;
        Timer searchTimer = new Timer(1000.0);

        public SPDefinitionWindow()
        {
            InitializeComponent();
            def = Program.Configs[Program.SelectedConfig].GetSMDef();
            if (def == null)
            {
                MessageBox.Show("The config was not able to parse a sourcepawn definiton.", "Stop", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Close();
                return;
            }
            List<SPDefEntry> defList = new List<SPDefEntry>();
            for (int i = 0; i < def.Functions.Length; ++i) { defList.Add((SPDefEntry)def.Functions[i]); }
            for (int i = 0; i < def.Constants.Length; ++i) { defList.Add(new SPDefEntry() { Name = def.Constants[i], Entry = "Constant" }); }
            for (int i = 0; i < def.Types.Length; ++i) { defList.Add(new SPDefEntry() { Name = def.Types[i], Entry = "Type" }); }
            for (int i = 0; i < def.MethodNames.Length; ++i) { defList.Add(new SPDefEntry() { Name = def.MethodNames[i], Entry = "Method" }); }
            for (int i = 0; i < def.Properties.Length; ++i) { defList.Add(new SPDefEntry() { Name = def.Properties[i], Entry = "Property" }); }
            defList.Sort((a, b) => { return string.Compare(a.Name, b.Name); });
            defArray = defList.ToArray();
            int defArrayLength = defArray.Length;
            items = new ListViewItem[defArrayLength];
            for (int i = 0; i < defArrayLength; ++i)
            {
                items[i] = new ListViewItem() { Content = defArray[i].Name, Tag = defArray[i].Entry };
                SPBox.Items.Add(items[i]);
            }
            searchTimer.Elapsed += searchTimer_Elapsed;
        }

        void searchTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DoSearch();
        }

        private void SPFunctionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object obj = SPBox.SelectedItem;
            if (obj == null) { return; }
            ListViewItem item = (ListViewItem)obj;
            object TagValue = item.Tag;
            if (TagValue != null)
            {
                if (TagValue is SPFunction)
                {
                    SPFunction func = (SPFunction)TagValue;
                    SPNameBlock.Text = func.Name;
                    SPFullNameBlock.Text = func.FullName;
                    SPCommentBox.Text = func.Comment;
                    return;
                }
                else if (TagValue is string)
                {
                    SPNameBlock.Text = (string)item.Content;
                    SPFullNameBlock.Text = (string)TagValue;
                    SPCommentBox.Text = string.Empty;
                    return;
                }
            }
            SPNameBlock.Text = (string)item.Content;
            SPFullNameBlock.Text = string.Empty;
            SPCommentBox.Text = string.Empty;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SPProgress.IsIndeterminate = true;
            searchTimer.Stop();
            searchTimer.Start();
        }

        private void DoSearch()
        {
            this.Dispatcher.Invoke(() =>
                {
                    int itemCount = defArray.Length;
                    string searchString = SPSearchBox.Text.ToLowerInvariant();
                    bool foundOccurence = false;
                    SPBox.Items.Clear();
                    for (int i = 0; i < itemCount; ++i)
                    {
                        if (defArray[i].Name.ToLowerInvariant().Contains(searchString))
                        {
                            foundOccurence = true;
                            SPBox.Items.Add(items[i]);
                        }
                    }
                    if (foundOccurence)
                    {
                        SPSearchBox.Background = Brushes.White;
                    }
                    else
                    {
                        SPSearchBox.Background = Brushes.LightYellow;
                    }
                    SPProgress.IsIndeterminate = false;
                });
        }

        private class SPDefEntry
        {
            public string Name;
            public object Entry;

            public static explicit operator SPDefEntry(SPFunction func)
            {
                return new SPDefEntry() { Name = func.Name, Entry = func };
            }
        }
    }
}
