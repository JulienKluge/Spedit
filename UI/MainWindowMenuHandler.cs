using MahApps.Metro.Controls.Dialogs;
using Spedit.Interop.Updater;
using Spedit.UI.Windows;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Spedit.UI
{
    public partial class MainWindow
    {
        private void FileMenu_Open(object sender, RoutedEventArgs e)
        {
			var editors = GetAllEditorElements();
            var editorsAreOpen = false;

			if (editors != null)
				editorsAreOpen = (editors.Length > 0);

            var editorIsSelected = GetCurrentEditorElement() != null;

            ((MenuItem)((MenuItem)sender).Items[3]).IsEnabled = editorIsSelected;
            ((MenuItem)((MenuItem)sender).Items[5]).IsEnabled = editorIsSelected;
            ((MenuItem)((MenuItem)sender).Items[7]).IsEnabled = editorIsSelected;
            ((MenuItem)((MenuItem)sender).Items[4]).IsEnabled = editorsAreOpen;
            ((MenuItem)((MenuItem)sender).Items[8]).IsEnabled = editorsAreOpen;
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
            var element = GetCurrentEditorElement();
            var menu = (MenuItem)sender;

            if (element == null)
            {
                foreach (var item in menu.Items)
                {
                    var menuItem = item as MenuItem;

                    if (menuItem != null)
                        menuItem.IsEnabled = false;
                }
            }
            else
            {
                MenuI_Undo.IsEnabled = element.editor.CanUndo;
                MenuI_Redo.IsEnabled = element.editor.CanRedo;

                for (var i = 2; i < menu.Items.Count; ++i)
                {
                    var item = menu.Items[i] as MenuItem;

                    if (item != null)
                        item.IsEnabled = true;
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

        private void Menu_ExpandAll(object sender, RoutedEventArgs e)
        {
            Command_FlushFoldingState(false);
        }

        private void Menu_CollapseAll(object sender, RoutedEventArgs e)
        {
            Command_FlushFoldingState(true);
        }

        private void Menu_JumpTo(object sender, RoutedEventArgs e)
        {
            Command_JumpTo();
        }

		private void Menu_ToggleCommentLine(object sender, RoutedEventArgs e)
		{
			Command_ToggleCommentLine();
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

        private void Menu_SendRCon(object sender, RoutedEventArgs e)
        {
            ServerQuery();
        }

        private void Menu_OpenWebsiteFromTag(object sender, RoutedEventArgs e)
        {
            var url = (string)((MenuItem)sender).Tag;
            Process.Start(new ProcessStartInfo(url));
        }

        private void Menu_About(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow() { Owner = this, ShowInTaskbar = false };
            aboutWindow.ShowDialog();
        }

        private void Menu_OpenSPDef(object sender, RoutedEventArgs e)
        {
            var spDefinitionWindow = new SPDefinitionWindow() { Owner = this, ShowInTaskbar = false };
            spDefinitionWindow.ShowDialog();
        }

        private void Menu_OpenOptions(object sender, RoutedEventArgs e)
        {
            var optionsWindow = new OptionsWindow() { Owner = this, ShowInTaskbar = false };
            optionsWindow.ShowDialog();
        }

        private void Menu_ReFormatCurrent(object sender, RoutedEventArgs e)
        {
            Command_TidyCode(false);
        }

        private void Menu_ReFormatAll(object sender, RoutedEventArgs e)
        {
            Command_TidyCode(true);
        }

        private void Menu_DecompileLysis(object sender, RoutedEventArgs e)
        {
            Command_Decompile(this);
        }

        private void ReportBug_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(@"https://github.com/TheJelle/Spedit/issues/new"));
        }

        private void UpdateCheck_Click(object sender, RoutedEventArgs e)
        {
            UpdateCheck.Check(false);

            var status = Program.UpdateStatus;

            if (status.IsAvailable)
            {
                var uWindow = new UpdateWindow(status) { Owner = this };
                uWindow.ShowDialog();

                if (!uWindow.Succeeded)
                    return;

                Command_SaveAll();

                lock (Program.UpdateStatus)
                {
                    Program.UpdateStatus.WriteAble = false;
                    Program.UpdateStatus.IsAvailable = false;
                }

                Close();
            }
            else
            {
                if (status.GotException)
                {
                    this.ShowMessageAsync(Program.Translations.FailedCheck, Program.Translations.ErrorUpdate + Environment.NewLine + $"{Program.Translations.Details}: " + status.ExceptionMessage
                        , MessageDialogStyle.Affirmative, MetroDialogOptions);
                }
                else
                {
                    this.ShowMessageAsync(Program.Translations.VersUpToDate, string.Format(Program.Translations.VersionYour, Assembly.GetEntryAssembly().GetName().Version)
                        , MessageDialogStyle.Affirmative, MetroDialogOptions);
                }
            }
        }

        private void MenuButton_Compile(object sender, RoutedEventArgs e)
        {
            var selected = CompileButton.SelectedIndex;
            Compile_SPScripts(selected != 1);
        }

        private void MenuButton_Action(object sender, RoutedEventArgs e)
        {
            var selected = CActionButton.SelectedIndex;

            switch (selected)
            {
                case 0:
                    Copy_Plugins();
                    break;
                case 1:
                    FTPUpload_Plugins();
                    break;
                case 2:
                    Server_Start();
                    break;
                default:
                    // ignored
                    break;
            }
        }
    }
}
