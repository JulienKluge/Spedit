using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Globalization;
using Spedit.UI.Components;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;

namespace Spedit.UI
{
    public partial class MainWindow
    {
        bool IsSearchFieldOpen = false;

        public void ToggleSearchField()
        {
            if (IsSearchFieldOpen)
            {
                IsSearchFieldOpen = false;
                FindReplaceGrid.IsHitTestVisible = false;
                if (Program.OptionsObject.UI_Animations)
                {
                    FadeFindReplaceGridOut.Begin();
                }
                else
                {
                    FindReplaceGrid.Opacity = 0.0;
                }
            }
            else
            {
                IsSearchFieldOpen = true;
                FindReplaceGrid.IsHitTestVisible = true;
                if (Program.OptionsObject.UI_Animations)
                {
                    FadeFindReplaceGridIn.Begin();
                }
                else
                {
                    FindReplaceGrid.Opacity = 1.0;
                    FindBox.Focus();
                }
            }
        }

        private void FadeInCompleted_FindReplace(object sender, EventArgs e)
        {
            FindBox.Focus();
        }

        private void SearchButtonClicked(object sender, RoutedEventArgs e)
        {
            Search();
        }
        private void ReplaceButtonClicked(object sender, RoutedEventArgs e)
        {
            Search(true);
        }
        private void ReplaceAllButtonClicked(object sender, RoutedEventArgs e)
        {
            ReplaceAll();
        }
        private void SearchBoxTextChanged(object sender, RoutedEventArgs e)
        {
            FindResultBlock.Text = string.Empty;
        }

        public void Search(bool Replace = false)
        {
            EditorElement editorElement = GetCurrentEditorElement();
            if (editorElement == null)
            {
                return;
            }
            Regex regex = GetSearchRegex();
            if (regex == null)
            {
                return;
            }

            int index = editorElement.editor.CaretOffset;
            MatchCollection matchCollection = regex.Matches(editorElement.editor.Text, 0);

            FindResultBlock.Text = matchCollection.Count.ToString() + " Occurences";
            if (matchCollection.Count > 0)
            {
                int matchIndexTaken = 0;
                for (int i = 0; i < matchCollection.Count; ++i)
                {
                    if (matchCollection[i].Index >= index)
                    {
                        matchIndexTaken = i;
                        break;
                    }
                }
                if (Replace)
                {
                    string replaceString = ReplaceBox.Text;
                    editorElement.editor.Document.Replace(matchCollection[matchIndexTaken].Index, matchCollection[matchIndexTaken].Length, replaceString);
                    editorElement.editor.Select(matchCollection[matchIndexTaken].Index, replaceString.Length);
                    editorElement.NeedsSave = true;
                }
                else
                {
                    editorElement.editor.Select(matchCollection[matchIndexTaken].Index, matchCollection[matchIndexTaken].Length);
                }
                editorElement.editor.TextArea.Caret.BringCaretToView();
            }
        }

        private void ReplaceAll()
        {
            EditorElement editorElement = GetCurrentEditorElement();
            if (editorElement == null)
            {
                return;
            }
            Regex regex = GetSearchRegex();
            if (regex == null)
            {
                return;
            }
            MatchCollection matchCollection = regex.Matches(editorElement.editor.Text, 0);
            string replaceString = ReplaceBox.Text;
            for (int i = matchCollection.Count - 1; i >= 0; --i)
            {
                editorElement.editor.Document.Replace(matchCollection[i].Index, matchCollection[i].Length, replaceString);
            }
            FindResultBlock.Text = matchCollection.Count.ToString() + " occurences replaced";

        }

        private Regex GetSearchRegex()
        {
            string findString = FindBox.Text;
            if (string.IsNullOrWhiteSpace(findString))
            {
                FindResultBlock.Text = "Empty search pattern";
                return null;
            }
            Regex regex;
            RegexOptions regexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant;
            if (!CCBox.IsChecked.Value)
            { regexOptions |= RegexOptions.IgnoreCase; }
            if (NSearch_RButton.IsChecked.Value)
            {
                regex = new Regex(Regex.Escape(findString), regexOptions);
            }
            else if (WSearch_RButton.IsChecked.Value)
            {
                regex = new Regex("\\b" + Regex.Escape(findString) + "\\b", regexOptions);
            }
            else if (ASearch_RButton.IsChecked.Value)
            {
                findString = findString.Replace("\\t", "\t").Replace("\\r", "\r").Replace("\\n", "\n");
                Regex rx = new Regex(@"\\[uUxX]([0-9A-F]{4})");
                findString = rx.Replace(findString, delegate(Match match) { return ((char)Int32.Parse(match.Value.Substring(2), NumberStyles.HexNumber)).ToString(); });
                regex = new Regex(Regex.Escape(findString), regexOptions);
            }
            else //if (RSearch_RButton.IsChecked.Value)
            {
                if (MLRBox.IsChecked.Value)
                { regexOptions |= RegexOptions.Multiline; }
                else
                { regexOptions |= RegexOptions.Singleline; }
                try
                {
                    regex = new Regex(findString, regexOptions);
                }
                catch (Exception) { FindResultBlock.Text = "No valid regex pattern!"; return null; }
            }
            return regex;
        }
    }
}
