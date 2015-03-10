using MahApps.Metro.Controls;
using Spedit.UI.Components;
using Spedit.UI.Windows;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Spedit.UI
{
    public partial class MainWindow : MetroWindow
    {
        private void FileMenu_Open(object sender, RoutedEventArgs e)
        {
			var editors = GetAllEditorElements();
            bool EditorsAreOpen = false;
			if (editors != null)
			{
				EditorsAreOpen = (editors.Length > 0);
			}
            bool EditorIsSelected = (GetCurrentEditorElement() != null);
            ((MenuItem)((MenuItem)sender).Items[3]).IsEnabled = EditorIsSelected;
            ((MenuItem)((MenuItem)sender).Items[5]).IsEnabled = EditorIsSelected;
            ((MenuItem)((MenuItem)sender).Items[7]).IsEnabled = EditorIsSelected;
            ((MenuItem)((MenuItem)sender).Items[4]).IsEnabled = EditorsAreOpen;
            ((MenuItem)((MenuItem)sender).Items[8]).IsEnabled = EditorsAreOpen;
        }

        private void Menu_New(object sender, RoutedEventArgs e)
        {
            Command_New();
        }

        private void Menu_Open(object sender, RoutedEventArgs e)
        {
            Command_Open();
        }

        private void Menu_Save(object sender, RoutedEventArgs e)
        {
            Command_Save();
        }

        private void Menu_SaveAll(object sender, RoutedEventArgs e)
        {
            Command_SaveAll();
        }

        private void Menu_SaveAs(object sender, RoutedEventArgs e)
        {
            Command_SaveAs();
        }

        private void Menu_Close(object sender, RoutedEventArgs e)
        {
            Command_Close();
        }
        private void Menu_CloseAll(object sender, RoutedEventArgs e)
        {
            Command_CloseAll();
        }

        private void EditMenu_Open(object sender, RoutedEventArgs e)
        {
            EditorElement ee = GetCurrentEditorElement();
            MenuItem menu = (MenuItem)sender;
            if (ee == null)
            {
                for (int i = 0; i < menu.Items.Count; ++i)
                {
                    if (menu.Items[i] is MenuItem)
                    {
                        ((MenuItem)menu.Items[i]).IsEnabled = false;
                    }
                }
            }
            else
            {
                ((MenuItem)menu.Items[0]).IsEnabled = ee.editor.CanUndo;
                ((MenuItem)menu.Items[1]).IsEnabled = ee.editor.CanRedo;
                for (int i = 2; i < menu.Items.Count; ++i)
                {
                    if (menu.Items[i] is MenuItem)
                    {
                        ((MenuItem)menu.Items[i]).IsEnabled = true;
                    }
                }
            }
        }

        private void Menu_Undo(object sender, RoutedEventArgs e)
        {
            Command_Undo();
        }

        private void Menu_Redo(object sender, RoutedEventArgs e)
        {
            Command_Redo();
        }

        private void Menu_Cut(object sender, RoutedEventArgs e)
        {
            Command_Cut();
        }

        private void Menu_Copy(object sender, RoutedEventArgs e)
        {
            Command_Copy();
        }

        private void Menu_Paste(object sender, RoutedEventArgs e)
        {
            Command_Paste();
        }

        private void Menu_SelectAll(object sender, RoutedEventArgs e)
        {
            Command_SelectAll();
        }

        private void Menu_FindAndReplace(object sender, RoutedEventArgs e)
        {
            ToggleSearchField();
        }

        private void Menu_CompileAll(object sender, RoutedEventArgs e)
        {
            Compile_SPScripts();
        }

        private void Menu_Compile(object sender, RoutedEventArgs e)
        {
            Compile_SPScripts(false);
        }

        private void Menu_CopyPlugin(object sender, RoutedEventArgs e)
        {
            Copy_Plugins();
        }

        private void Menu_FTPUpload(object sender, RoutedEventArgs e)
        {
            FTPUpload_Plugins();
        }

        private void Menu_StartServer(object sender, RoutedEventArgs e)
        {
            Server_Start();
        }

        private void Menu_OpenWebsiteFromTag(object sender, RoutedEventArgs e)
        {
            string url = (string)((MenuItem)sender).Tag;
            Process.Start(new ProcessStartInfo(url));
        }

        private void Menu_About(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow() { Owner = this, ShowInTaskbar = false };
            aboutWindow.ShowDialog();
        }

        private void Menu_OpenSPDef(object sender, RoutedEventArgs e)
        {
            SPDefinitionWindow spDefinitionWindow = new SPDefinitionWindow() { Owner = this, ShowInTaskbar = false };
            spDefinitionWindow.ShowDialog();
        }

        private void Menu_OpenOptions(object sender, RoutedEventArgs e)
        {
            OptionsWindow optionsWindow = new OptionsWindow() { Owner = this, ShowInTaskbar = false };
            optionsWindow.ShowDialog();
        }

        private void ReportBug_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(@"https://github.com/TheJelle/Spedit/issues/new"));
        }
    }
}
