using Spedit.UI.Components;
using Spedit.UI.Windows;
using System.Windows;
using System.Windows.Controls;

namespace Spedit.UI
{
    public partial class MainWindow
    {
        public void FillConfigMenu()
        {
            ConfigMenu.Items.Clear();

            for (var i = 0; i < Program.Configs.Length; ++i)
            {
                var item = new MenuItem
                {
                    Header = Program.Configs[i].Name,
                    IsCheckable = true,
                    IsChecked = i == Program.SelectedConfig
                };

                item.Click += item_Click;
                ConfigMenu.Items.Add(item);
            }

            ConfigMenu.Items.Add(new Separator());
            var editItem = new MenuItem() { Header = Program.Translations.EditConfig };
            editItem.Click += editItem_Click;
            ConfigMenu.Items.Add(editItem);
        }

        private void editItem_Click(object sender, RoutedEventArgs e)
        {
            var configWindow = new ConfigWindow {Owner = this, ShowInTaskbar = false};
            configWindow.ShowDialog();
        }

        private void item_Click(object sender, RoutedEventArgs e)
        {
            var name = (string)((MenuItem)sender).Header;
            ChangeConfig(name);
        }

        public void ChangeConfig(int index)
        {
            if (index < 0 || index >= Program.Configs.Length)
                return;

            Program.Configs[index].LoadSMDef();
            var name = Program.Configs[index].Name;

            for (var i = 0; i < ConfigMenu.Items.Count - 2; ++i)
                ((MenuItem) ConfigMenu.Items[i]).IsChecked = name == (string) ((MenuItem) ConfigMenu.Items[i]).Header;

            Program.SelectedConfig = index;
            Program.OptionsObject.ProgramSelectedConfig = Program.Configs[Program.SelectedConfig].Name;
            var editors = GetAllEditorElements();

            if (editors == null)
                return;

            foreach (var element in editors)
            {
                element.LoadAutoCompletes();
                element.editor.SyntaxHighlighting = new AeonEditorHighlighting();
                element.InvalidateVisual();
            }
        }

        public void ChangeConfig(string name)
        {
            for (var i = 0; i < Program.Configs.Length; ++i)
            {
                if (Program.Configs[i].Name != name)
                    continue;

                ChangeConfig(i);

                return;
            }
        }
    }
}
