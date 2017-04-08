using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Spedit.UI.Components;
using Spedit.UI.Windows;
using Spedit.Utils.SPSyntaxTidy;
using System.Text;
using Lysis;
using System.IO;
using System.Linq;

namespace Spedit.UI
{
    public partial class MainWindow
    {
        public EditorElement GetCurrentEditorElement()
        {
            EditorElement outElement = null;

            if (DockingPane.SelectedContent?.Content == null)
                return null;

            var possElement = DockingManager.ActiveContent;

            if (possElement == null)
                return null;

            var element = possElement as EditorElement;

            if (element != null)
                outElement = element;

            return outElement;
        }

        public EditorElement[] GetAllEditorElements()
        {
            return EditorsReferences.Count < 1 ? null : EditorsReferences.ToArray();
        }

        private void Command_New()
        {
            var nfWindow = new NewFileWindow {Owner = this, ShowInTaskbar = false};
            nfWindow.ShowDialog();
        }

        private void Command_Open()
        {
            var ofd = new OpenFileDialog
            {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = @"Sourcepawn Files (*.sp *.inc)|*.sp;*.inc|Sourcemod Plugins (*.smx)|*.smx|All Files (*.*)|*.*",
                Multiselect = true,
                Title = Program.Translations.OpenNewFile
            };

            var result = ofd.ShowDialog(this);

            if (result.Value)
            {
                var anyFileLoaded = false;

                if (ofd.FileNames.Length > 0)
                {
                    for (var i = 0; i < ofd.FileNames.Length; ++i)
                        anyFileLoaded |= TryLoadSourceFile(ofd.FileNames[i], i == 0, true, i == 0);

                    if (!anyFileLoaded)
                    {
                        MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Theme;
                        this.ShowMessageAsync(Program.Translations.NoFileOpened, Program.Translations.NoFileOpenedCap, MessageDialogStyle.Affirmative, MetroDialogOptions);
                    }
                }
            }

            Activate();
        }

        private void Command_Save()
        {
            var element = GetCurrentEditorElement();

            if (element == null)
                return;

            element.Save(true);
            _blendOverEffect.Begin();
        }

        private void Command_SaveAs()
        {
            var element = GetCurrentEditorElement();

            if (element == null)
                return;

            var sfd = new SaveFileDialog
            {
                AddExtension = true,
                Filter = @"Sourcepawn Files (*.sp *.inc)|*.sp;*.inc|All Files (*.*)|*.*",
                OverwritePrompt = true,
                Title = Program.Translations.SaveFileAs,
                FileName = element.Parent.Title.Trim('*')
            };

            var result = sfd.ShowDialog(this);

            if (!result.Value)
                return;

            if (string.IsNullOrWhiteSpace(sfd.FileName))
                return;

            element.FullFilePath = sfd.FileName;
            element.Save(true);
            _blendOverEffect.Begin();
        }

        private void Command_SaveAll()
        {
            var editors = GetAllEditorElements();

            if (!(editors?.Length > 0))
                return;

            foreach (var element in editors)
                element.Save();

            _blendOverEffect.Begin();
        }

        private void Command_Close()
        {
            var element = GetCurrentEditorElement();
            element?.Close();
        }

        private async void Command_CloseAll()
        {
            var editors = GetAllEditorElements();

            if (!(editors?.Length > 0))
                return;

            var unsavedEditorsExisting = editors.Aggregate(false, (current, t) => current | t.NeedsSave);

            var forceSave = false;

            if (unsavedEditorsExisting)
            {
                var str = new StringBuilder();

                for (var i = 0; i < editors.Length; ++i)
                    if (i == 0)
                        str.Append(editors[i].Parent.Title.Trim('*'));
                    else
                        str.AppendLine(editors[i].Parent.Title.Trim('*'));

                var result = await this.ShowMessageAsync(Program.Translations.SaveFollow, str.ToString(), MessageDialogStyle.AffirmativeAndNegative, MetroDialogOptions);

                if (result == MessageDialogResult.Affirmative)
                    forceSave = true;
            }

            foreach (var element in editors)
                element.Close(forceSave, forceSave);
        }

        private void Command_Undo()
        {
            var element = GetCurrentEditorElement();

            if (element == null)
                return;

            if (element.editor.CanUndo)
                element.editor.Undo();
        }

        private void Command_Redo()
        {
            var element = GetCurrentEditorElement();

            if (element == null)
                return;

            if (element.editor.CanRedo)
                element.editor.Redo();
        }

        private void Command_Cut()
        {
            var element = GetCurrentEditorElement();
            element?.editor.Cut();
        }

        private void Command_Copy()
        {
            var element = GetCurrentEditorElement();
            element?.editor.Copy();
        }

        private void Command_Paste()
        {
            var element = GetCurrentEditorElement();
            element?.editor.Paste();
        }

        private void Command_FlushFoldingState(bool state)
        {
            var element = GetCurrentEditorElement();

            if (element?.FoldingManager == null)
                return;

            var foldings = element.FoldingManager.AllFoldings;

            foreach (var folding in foldings)
                folding.IsFolded = state;
        }

        private void Command_JumpTo()
        {
            var element = GetCurrentEditorElement();
            element?.ToggleJumpGrid();
        }

        private void Command_SelectAll()
        {
            var element = GetCurrentEditorElement();
            element?.editor.SelectAll();
        }

		private void Command_ToggleCommentLine()
		{
			var element = GetCurrentEditorElement();
		    element?.ToggleCommentOnLine();
		}

        private void Command_TidyCode(bool all)
        {
            var editors = all ? GetAllEditorElements() : new[] { GetCurrentEditorElement() };

            foreach (var element in editors)
            {
                if (element == null)
                    continue;

                element.editor.Document.BeginUpdate();
                var source = element.editor.Text;
                element.editor.Document.Replace(0, source.Length, SPSyntaxTidy.TidyUp(source));
                element.editor.Document.EndUpdate();
            }
        }

        private async void Command_Decompile(MainWindow win)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Sourcepawn Plugins (*.smx)|*.smx",
                Title = Program.Translations.ChDecomp
            };

            var result = ofd.ShowDialog();

            if (result == null || !result.Value)
                return;

            if (string.IsNullOrWhiteSpace(ofd.FileName))
                return;

            var fInfo = new FileInfo(ofd.FileName);

            if (!fInfo.Exists)
                return;

            ProgressDialogController task = null;

            if (win != null)
            {
                task = await this.ShowProgressAsync(Program.Translations.Decompiling, fInfo.FullName, false, MetroDialogOptions);
                ProcessUITasks();
            }

            var destFile = fInfo.FullName + ".sp";
            File.WriteAllText(destFile, LysisDecompiler.Analyze(fInfo), Encoding.UTF8);
            TryLoadSourceFile(destFile, true, false);

            if (task != null)
                await task.CloseAsync();
        }
    }
}
