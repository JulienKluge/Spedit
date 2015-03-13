using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Spedit.UI.Components;
using Spedit.UI.Windows;
using System.Text;

namespace Spedit.UI
{
    public partial class MainWindow
    {
        public EditorElement GetCurrentEditorElement()
        {
            EditorElement outElement = null;
            if (DockingPane.SelectedContent != null)
            {
                if (DockingPane.SelectedContent.Content != null)
                {
                    outElement = (EditorElement)DockingPane.SelectedContent.Content;
                }
            }
            return outElement;
        }

        public EditorElement[] GetAllEditorElements()
        {
            return this.EditorsReferences.ToArray();
        }

        private void Command_New()
        {
            NewFileWindow nfWindow = new NewFileWindow() { Owner = this, ShowInTaskbar = false };
            nfWindow.ShowDialog();
        }

        private void Command_Open()
        {
            OpenFileDialog ofd = new OpenFileDialog() { AddExtension = true, CheckFileExists = true, CheckPathExists = true, Filter = @"Sourcepawn Files (*.sp *.inc)|*.sp;*.inc|All Files (*.*)|*.*", Multiselect = true, Title = "Open new File" };
            var result = ofd.ShowDialog(this);
            if (result.Value)
            {
                bool AnyFileLoaded = false;
                if (ofd.FileNames.Length > 0)
                {
                    for (int i = 0; i < ofd.FileNames.Length; ++i)
                    {
                        AnyFileLoaded |= TryLoadSourceFile(ofd.FileNames[i]);
                    }
                    if (!AnyFileLoaded)
                    {
                        this.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Theme;
                        this.ShowMessageAsync("No Files Openend.", "None of the selected files could be opened.", MessageDialogStyle.Affirmative, this.MetroDialogOptions);
                    }
                }
            }
        }

        private void Command_Save()
        {
            EditorElement ee = GetCurrentEditorElement();
            if (ee != null)
            {
                ee.Save(true);
                BlendOverEffect.Begin();
            }
        }

        private void Command_SaveAs()
        {
            EditorElement ee = GetCurrentEditorElement();
            if (ee != null)
            {
                SaveFileDialog sfd = new SaveFileDialog() { AddExtension = true, Filter = @"Sourcepawn Files (*.sp *.inc)|*.sp;*.inc|All Files (*.*)|*.*", OverwritePrompt = true, Title = "Save File As" };
                sfd.FileName = ee.Parent.Title.Trim(new char[] { '*' });
                var result = sfd.ShowDialog(this);
                if (result.Value)
                {
                    if (!string.IsNullOrWhiteSpace(sfd.FileName))
                    {
                        ee.FullFilePath = sfd.FileName;
                        ee.Save(true);
                        BlendOverEffect.Begin();
                    }
                }
            }
        }

        private void Command_SaveAll()
        {
            EditorElement[] editors = GetAllEditorElements();
            if (editors == null)
            {
                return;
            }
            if (editors.Length > 0)
            {
                for (int i = 0; i < editors.Length; ++i)
                {
                    editors[i].Save();
                }
                BlendOverEffect.Begin();
            }
        }

        private void Command_Close()
        {
            EditorElement ee = GetCurrentEditorElement();
            if (ee != null)
            {
                ee.Close();
            }
        }

        private async void Command_CloseAll()
        {
            EditorElement[] editors = GetAllEditorElements();
            if (editors == null)
            {
                return;
            }
            if (editors.Length > 0)
            {
                bool UnsavedEditorsExisting = false;
                for (int i = 0; i < editors.Length; ++i)
                {
                    UnsavedEditorsExisting |= editors[i].NeedsSave;
                }
                bool ForceSave = false;
                if (UnsavedEditorsExisting)
                {
                    StringBuilder str = new StringBuilder();
                    for (int i = 0; i < editors.Length; ++i)
                    {
                        if (i == 0)
                        { str.Append(editors[i].Parent.Title.Trim(new char[] { '*' })); }
                        else
                        { str.AppendLine(editors[i].Parent.Title.Trim(new char[] { '*' })); }
                    }
                    var Result = await this.ShowMessageAsync("Save following Files:", str.ToString(), MessageDialogStyle.AffirmativeAndNegative, this.MetroDialogOptions);
                    if (Result == MessageDialogResult.Affirmative)
                    {
                        ForceSave = true;
                    }
                }
                for (int i = 0; i < editors.Length; ++i)
                {
                    editors[i].Close(ForceSave, ForceSave);
                }
            }
        }

        private void Command_Undo()
        {
            EditorElement ee = GetCurrentEditorElement();
            if (ee != null)
            {
                if (ee.editor.CanUndo)
                {
                    ee.editor.Undo();
                }
            }
        }

        private void Command_Redo()
        {
            EditorElement ee = GetCurrentEditorElement();
            if (ee != null)
            {
                if (ee.editor.CanRedo)
                {
                    ee.editor.Redo();
                }
            }
        }

        private void Command_Cut()
        {
            EditorElement ee = GetCurrentEditorElement();
            if (ee != null)
            {
                ee.editor.Cut();
            }
        }

        private void Command_Copy()
        {
            EditorElement ee = GetCurrentEditorElement();
            if (ee != null)
            {
                ee.editor.Copy();
            }
        }

        private void Command_Paste()
        {
            EditorElement ee = GetCurrentEditorElement();
            if (ee != null)
            {
                ee.editor.Paste();
            }
        }

        private void Command_SelectAll()
        {
            EditorElement ee = GetCurrentEditorElement();
            if (ee != null)
            {
                ee.editor.SelectAll();
            }
        }

    }
}
