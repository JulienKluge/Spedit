using Spedit.UI.Components;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Spedit.UI
{
    public partial class MainWindow
    {
        private bool _isSearchFieldOpen;

        public void ToggleSearchField()
        {
            var element = GetCurrentEditorElement();

            if (_isSearchFieldOpen)
            {
                if (element != null)
                {
                    if (element.IsKeyboardFocusWithin)
                    {
                        if (element.editor.SelectionLength > 0)
                        {
                            FindBox.Text = element.editor.SelectedText;
                        }

                        FindBox.SelectAll();
                        FindBox.Focus();

                        return;
                    }
                }

                _isSearchFieldOpen = false;
                FindReplaceGrid.IsHitTestVisible = false;

                if (Program.OptionsObject.UIAnimations)
                    _fadeFindReplaceGridOut.Begin();
                else
                    FindReplaceGrid.Opacity = 0.0;

                if (element == null)
                    return;

                element.editor.Focus();
            }
            else
            {
                _isSearchFieldOpen = true;
                FindReplaceGrid.IsHitTestVisible = true;

                if (element == null)
                    return;

                if (element.editor.SelectionLength > 0)
                    FindBox.Text = element.editor.SelectedText;

                FindBox.SelectAll();

                if (Program.OptionsObject.UIAnimations)
                    _fadeFindReplaceGridIn.Begin();
                else
                    FindReplaceGrid.Opacity = 1.0;

                FindBox.Focus();
            }
        }

        private void CloseFindReplaceGrid(object sender, RoutedEventArgs e)
        {
            ToggleSearchField();
        }

        private void SearchButtonClicked(object sender, RoutedEventArgs e)
        {
            Search();
        }

        private void ReplaceButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ReplaceButton.SelectedIndex == 1)
                ReplaceAll();
            else
                Replace();
        }

        private void CountButtonClicked(object sender, RoutedEventArgs e)
        {
            Count();
        }

        private void SearchBoxTextChanged(object sender, RoutedEventArgs e)
        {
            FindResultBlock.Text = string.Empty;
        }

        private void SearchBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Search();
        }

        private void ReplaceBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Replace();
        }

        private void FindReplaceGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                ToggleSearchField();
        }

        private void Search()
        {
            int editorIndex;
            var editors = GetEditorElementsForFrAction(out editorIndex);

            if (editors == null)
                return;

            if (editors.Length < 1)
                return;

            if (editors[0] == null)
                return;

            var regex = GetSearchRegex();

            if (regex == null)
                return;

            var startFileCaretOffset = 0;
            var foundOccurence = false;

            for (var i = editorIndex; i < editors.Length + editorIndex + 1; ++i)
            {
                var index = ValueUnderMap(i, editors.Length);
                string searchText;
                var addToOffset = 0;

                if (i == editorIndex)
                {
                    startFileCaretOffset = editors[index].editor.CaretOffset;
                    addToOffset = startFileCaretOffset;

                    if (startFileCaretOffset < 0)
                        startFileCaretOffset = 0;

                    searchText = editors[index].editor.Text.Substring(startFileCaretOffset);
                }
                else if (i == editors.Length + editorIndex)
                {
                    searchText = startFileCaretOffset == 0
                        ? string.Empty
                        : editors[index].editor.Text.Substring(0, startFileCaretOffset);
                }
                else
                {
                    searchText = editors[index].editor.Text;
                }

                if (string.IsNullOrWhiteSpace(searchText))
                    continue;

                var m = regex.Match(searchText);

                if (!m.Success)
                    continue;

                foundOccurence = true;
                editors[index].Parent.IsSelected = true;
                editors[index].editor.CaretOffset = m.Index + addToOffset + m.Length;
                editors[index].editor.Select(m.Index + addToOffset, m.Length);
                var location = editors[index].editor.Document.GetLocation(m.Index + addToOffset);
                editors[index].editor.ScrollTo(location.Line, location.Column);
                //FindResultBlock.Text = "Found in offset " + (m.Index + addToOffset).ToString() + " with length " + m.Length.ToString();
                FindResultBlock.Text = string.Format(Program.Translations.FoundInOff, m.Index + addToOffset, m.Length);
                break;
            }

            if (!foundOccurence)
                FindResultBlock.Text = Program.Translations.FoundNothing;
        }

        private void Replace()
        {
            int editorIndex;
            var editors = GetEditorElementsForFrAction(out editorIndex);

            if (editors == null)
                return;

            if (editors.Length < 1)
                return;

            if (editors[0] == null)
                return;

            var regex = GetSearchRegex();

            if (regex == null)
                return;

            var replaceString = ReplaceBox.Text;
            var startFileCaretOffset = 0;
            var foundOccurence = false;

            for (var i = editorIndex; i < editors.Length + editorIndex + 1; ++i)
            {
                var index = ValueUnderMap(i, editors.Length);
                string searchText;
                var addToOffset = 0;

                if (i == editorIndex)
                {
                    startFileCaretOffset = editors[index].editor.CaretOffset;
                    addToOffset = startFileCaretOffset;

                    if (startFileCaretOffset < 0)
                        startFileCaretOffset = 0;

                    searchText = editors[index].editor.Text.Substring(startFileCaretOffset);
                }
                else if (i == editors.Length + editorIndex)
                {
                    searchText = startFileCaretOffset == 0
                        ? string.Empty
                        : editors[index].editor.Text.Substring(0, startFileCaretOffset);
                }
                else
                {
                    searchText = editors[index].editor.Text;
                }

                if (string.IsNullOrWhiteSpace(searchText))
                    continue;

                var m = regex.Match(searchText);

                if (!m.Success)
                    continue;

                foundOccurence = true;
                editors[index].Parent.IsSelected = true;
                var result = m.Result(replaceString);
                editors[index].editor.Document.Replace(m.Index + addToOffset, m.Length, result);
                editors[index].editor.CaretOffset = m.Index + addToOffset + result.Length;
                editors[index].editor.Select(m.Index + addToOffset, result.Length);
                var location = editors[index].editor.Document.GetLocation(m.Index + addToOffset);
                editors[index].editor.ScrollTo(location.Line, location.Column);
                FindResultBlock.Text = $"{Program.Translations.ReplacedOff} {MinHeight + addToOffset}";
                break;
            }

            if (!foundOccurence)
                FindResultBlock.Text = Program.Translations.FoundNothing;
        }

        private void ReplaceAll()
        {
            int editorIndex;
            var editors = GetEditorElementsForFrAction(out editorIndex);

            if (editors == null)
                return;

            if (editors.Length < 1)
                return;

            if (editors[0] == null)
                return;

            var regex = GetSearchRegex();

            if (regex == null)
                return;

            var count = 0;
            var fileCount = 0;
            var replaceString = ReplaceBox.Text;

            foreach (var element in editors)
            {
                var mc = regex.Matches(element.editor.Text);

                if (mc.Count <= 0)
                    continue;

                fileCount++;
                count += mc.Count;
                element.editor.BeginChange();

                for (var j = mc.Count - 1; j >= 0; --j)
                {
                    var replace = mc[j].Result(replaceString);
                    element.editor.Document.Replace(mc[j].Index, mc[j].Length, replace);
                }

                element.editor.EndChange();
                element.NeedsSave = true;
            }
            //FindResultBlock.Text = "Replaced " + count.ToString() + " occurences in " + fileCount.ToString() + " documents";
            FindResultBlock.Text = string.Format(Program.Translations.ReplacedOcc, count, fileCount);
        }

        private void Count()
        {
            int editorIndex;
            var editors = GetEditorElementsForFrAction(out editorIndex);

            if (editors == null)
                return;

            if (editors.Length < 1)
                return;

            if (editors[0] == null)
                return;

            var regex = GetSearchRegex();

            if (regex == null)
                return;

            var count = editors.Select(t => regex.Matches(t.editor.Text)).Select(mc => mc.Count).Sum();

            FindResultBlock.Text = count + Program.Translations.OccFound;
        }

        private Regex GetSearchRegex()
        {
            var findString = FindBox.Text;

            if (string.IsNullOrEmpty(findString))
            {
                FindResultBlock.Text = Program.Translations.EmptyPatt;
                return null;
            }

            Regex regex;
            var regexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant;

            if (CCBox.IsChecked != null && !CCBox.IsChecked.Value)
                regexOptions |= RegexOptions.IgnoreCase;

            if (NSearch_RButton.IsChecked != null && NSearch_RButton.IsChecked.Value)
                regex = new Regex(Regex.Escape(findString), regexOptions);
            else if (WSearch_RButton.IsChecked != null && WSearch_RButton.IsChecked.Value)
                regex = new Regex("\\b" + Regex.Escape(findString) + "\\b", regexOptions);
            else if (ASearch_RButton.IsChecked != null && ASearch_RButton.IsChecked.Value)
            {
                findString = findString.Replace("\\t", "\t").Replace("\\r", "\r").Replace("\\n", "\n");
                var rx = new Regex(@"\\[uUxX]([0-9A-F]{4})");
                findString = rx.Replace(findString,
                    match => ((char) int.Parse(match.Value.Substring(2), NumberStyles.HexNumber)).ToString());
                regex = new Regex(Regex.Escape(findString), regexOptions);
            }
            else //if (RSearch_RButton.IsChecked.Value)
            {
                regexOptions |= RegexOptions.Multiline;

                if (MLRBox.IsChecked != null && MLRBox.IsChecked.Value)
                    regexOptions |= RegexOptions.Singleline;

                try
                {
                    regex = new Regex(findString, regexOptions);
                }
                catch (Exception)
                {
                    FindResultBlock.Text = Program.Translations.NoValidRegex;
                    return null;
                }
            }

            return regex;
        }

        private EditorElement[] GetEditorElementsForFrAction(out int editorIndex)
        {
            var editorStartIndex = 0;
            EditorElement[] editors;

            if (FindDestinies.SelectedIndex == 0)
                editors = new[] {GetCurrentEditorElement()};
            else
            {
                editors = GetAllEditorElements();
                var checkElement = DockingPane.SelectedContent?.Content;
                if (checkElement is EditorElement)
                    for (var i = 0; i < editors.Length; ++i)
                        if (editors[i] == checkElement)
                            editorStartIndex = i;
            }

            editorIndex = editorStartIndex;
            return editors;
        }

        private static int ValueUnderMap(int value, int map)
        {
            while (value >= map)
                value -= map;

            return value;
        }
    }
}
