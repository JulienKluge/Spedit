using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using SourcepawnCondenser.SourcemodDefinition;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Spedit.UI.Components
{
    public partial class EditorElement
    {
        private bool _acOpen;
        private bool _isOpen;
        private bool _animationsLoaded;
        private bool _acIsFuncC = true;
        private int _lastShowedLine = -1;
        private string[] _funcNames;
        private SMFunction[] _funcs;
        private ACNode[] _acEntrys;
        private ISNode[] _isEntrys;
        private Storyboard _fadeIsacIn;
        private Storyboard _fadeIsacOut;
        private Storyboard _fadeAcIn;
        private Storyboard _fadeAcOut;
        private Storyboard _fadeAcFuncCIn;
        private Storyboard _fadeAcMethodCIn;
        private readonly Regex _isFindRegex = new Regex(@"\b(?<name>[a-zA-Z_][a-zA-Z0-9_]+)\(", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private readonly Regex _multilineCommentRegex = new Regex(@"/\*.*?\*/", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline);

		public ulong LastSMDefUpdateUID = 0;
        public bool IsacOpen;

        public void LoadAutoCompletes()
        {
            if (!_animationsLoaded)
            {
                _fadeIsacIn = (Storyboard) Resources["FadeISACIn"];
                _fadeIsacOut = (Storyboard) Resources["FadeISACOut"];
                _fadeAcIn = (Storyboard) Resources["FadeACIn"];
                _fadeAcOut = (Storyboard) Resources["FadeACOut"];
                _fadeAcFuncCIn = (Storyboard) Resources["FadeAC_FuncC_In"];
                _fadeAcMethodCIn = (Storyboard) Resources["FadeAC_MethodC_In"];
                _fadeIsacOut.Completed += FadeISACOut_Completed;
                _fadeAcOut.Completed += FadeACOut_Completed;
                _animationsLoaded = true;
            }

            if (IsacOpen)
				HideIsac();

			var def = Program.Configs[Program.SelectedConfig].GetSMDef();

            _funcNames = def.FunctionStrings;
			_funcs = def.Functions.ToArray();
            _acEntrys = def.ProduceACNodes();
            _isEntrys = def.ProduceISNodes();

			AutoCompleteBox.ItemsSource = _acEntrys;
			MethodAutoCompleteBox.ItemsSource = _isEntrys;
        }

		public void InterruptLoadAutoCompletes(string[] functionStrings, SMFunction[] functionArray, ACNode[] acNodes, ISNode[] isNodes)
		{
			Dispatcher.Invoke(() => {
				_funcNames = functionStrings;
				_funcs = functionArray;
				_acEntrys = acNodes;
				_isEntrys = isNodes;

				AutoCompleteBox.ItemsSource = _acEntrys;
				MethodAutoCompleteBox.ItemsSource = _isEntrys;
			});
		}

        private void EvaluateIntelliSense()
        {
            if (editor.SelectionLength > 0)
            {
                HideIsac();
                return;
            }
            var currentLineIndex = editor.TextArea.Caret.Line - 1;
            var line = editor.Document.Lines[currentLineIndex];
            var text = editor.Document.GetText(line.Offset, line.Length);
            var lineOffset = editor.TextArea.Caret.Column - 1;
            var caretOffset = editor.CaretOffset;
            var forwardShowAc = false;
            var forwardShowIs = false;
            var isFuncNameStr = string.Empty;
            var isFuncDescriptionStr = string.Empty;
            var forceReSet = currentLineIndex != _lastShowedLine;
            var forceIsKeepsClosed = forceReSet;
            var xPos = int.MaxValue;
            var quotationCount = 0;
            var methodAc = false;

            _lastShowedLine = currentLineIndex;

            for (var i = 0; i < lineOffset; ++i)
            {
                if (text[i] == '"')
                    if (i != 0)
                        if (text[i - 1] != '\\')
                            quotationCount++;

                if ((quotationCount % 2) != 0)
                    continue;

                if (text[i] != '/')
                    continue;

                if (i == 0)
                    continue;

                if (text[i - 1] != '/')
                    continue;

                HideIsac();
                return;
            }

            foreach (var t in text)
            {
                if (t == '#')
                {
                    HideIsac();
                    return;
                }

                if (!char.IsWhiteSpace(t))
                    break;
            }

            var mc = _multilineCommentRegex.Matches(editor.Text, 0); //it hurts me to do it here..but i have no other choice...
            var mlcCount = mc.Count;

            for (var i = 0; i < mlcCount; ++i)
            {
                if (caretOffset >= mc[i].Index)
                {
                    if (caretOffset > (mc[i].Index + mc[i].Length))
                        continue;

                    HideIsac();
                    return;
                }

                break;
            }

            if (lineOffset > 0)
            {
                #region IS
                var isMatches = _isFindRegex.Matches(text);
                var scopeLevel = 0;

                for (var i = lineOffset - 1; i >= 0; --i)
                {
                    if (text[i] == ')')
                    {
                        scopeLevel++;
                    }
                    else if (text[i] == '(')
                    {
                        scopeLevel--;

                        if (scopeLevel >= 0)
                            continue;

                        var foundMatch = false;
                        var searchIndex = i;

                        for (var j = 0; j < isMatches.Count; ++j)
                        {
                            if ((searchIndex < isMatches[j].Index) ||
                                (searchIndex > (isMatches[j].Index + isMatches[j].Length))) continue;

                            foundMatch = true;

                            var testString = isMatches[j].Groups["name"].Value;

                            foreach (var func in _funcs)
                            {
                                if (testString != func.Name)
                                    continue;

                                xPos = isMatches[j].Groups["name"].Index + isMatches[j].Groups["name"].Length;
                                forwardShowIs = true;
                                isFuncNameStr = func.FullName;
                                isFuncDescriptionStr = func.CommentString;
                                forceReSet = true;
                                break;
                            }
                            break;
                        }

                        if (!foundMatch)
                            continue;

                        break;
                    }
                }
                #endregion

                #region AC
                if (IsValidFunctionChar(text[lineOffset - 1]) && (quotationCount % 2) == 0)
                {
                    var isNextCharValid = true;

                    if (text.Length > lineOffset)
                        if (IsValidFunctionChar(text[lineOffset]) || text[lineOffset] == '(')
                            isNextCharValid = false;

                    if (isNextCharValid)
                    {
                        var endOffset = lineOffset - 1;

                        for (var i = endOffset; i >= 0; --i)
                        {
                            if (!IsValidFunctionChar(text[i]))
                            {
                                if (text[i] == '.')
                                    methodAc = true;

                                break;
                            }

                            endOffset = i;
                        }

                        var testString = text.Substring(endOffset, ((lineOffset - 1) - endOffset) + 1);

                        if (testString.Length > 0)
                        {
                            if (methodAc)
                            {
                                for (var i = 0; i < _isEntrys.Length; ++i)
                                {
                                    if (!_isEntrys[i].EntryName.StartsWith(testString,
                                            StringComparison.InvariantCultureIgnoreCase))
                                        continue;

                                    if (testString == _isEntrys[i].EntryName)
                                        continue;

                                    forwardShowAc = true;
                                    MethodAutoCompleteBox.SelectedIndex = i;
                                    MethodAutoCompleteBox.ScrollIntoView(MethodAutoCompleteBox.SelectedItem);
                                    break;
                                }
                            }
                            else
                            {
                                for (var i = 0; i < _acEntrys.Length; ++i)
                                {
                                    if (!_acEntrys[i].EntryName.StartsWith(testString,
                                            StringComparison.InvariantCultureIgnoreCase))
                                        continue;

                                    if (testString == _acEntrys[i].EntryName)
                                        continue;

                                    forwardShowAc = true;
                                    AutoCompleteBox.SelectedIndex = i;
                                    AutoCompleteBox.ScrollIntoView(AutoCompleteBox.SelectedItem);
                                    break;
                                }
                            }
                        }
                    }
                }
                #endregion
            }

            if (!forwardShowAc)
                if (forceIsKeepsClosed)
                    forwardShowIs = false;

            if (forwardShowAc | forwardShowIs)
            {
                if (forwardShowAc)
                    ShowAc(!methodAc);
                else
                    HideAc();

                if (forwardShowIs)
                    ShowIs(isFuncNameStr, isFuncDescriptionStr);
                else
                    HideIs();

                if (forceReSet && IsacOpen)
                    SetIsacPosition(xPos);

                ShowIsac(xPos);
            }
            else
                HideIsac();
        }

        private bool ISAC_EvaluateKeyDownEvent(Key k)
        {
            if (!IsacOpen || !_acOpen)
                return false;

            switch (k)
            {
                case Key.Enter:
                {
                    var startOffset = editor.CaretOffset - 1;
                    var endOffset = startOffset;

                    for (var i = startOffset; i >= 0; --i)
                    {
                        if (!IsValidFunctionChar(editor.Document.GetCharAt(i)))
                            break;

                        endOffset = i;
                    }

                    var length = (startOffset - endOffset) + 1;
                    string replaceString;

                    if (_acIsFuncC)
                    {
                        replaceString = ((ACNode)AutoCompleteBox.SelectedItem).EntryName;
                        if (_acEntrys[AutoCompleteBox.SelectedIndex].IsExecuteable)
                            replaceString = replaceString + "(";
                    }
                    else
                    {
                        replaceString = ((ISNode)MethodAutoCompleteBox.SelectedItem).EntryName;
                        if (_isEntrys[MethodAutoCompleteBox.SelectedIndex].IsExecuteable)
                            replaceString = replaceString + "(";
                    }

                    editor.Document.Replace(endOffset, length, replaceString);
                    return true;
                }
                case Key.Up:
                {
                    if (_acIsFuncC)
                    {
                        AutoCompleteBox.SelectedIndex = Math.Max(0, AutoCompleteBox.SelectedIndex - 1);
                        AutoCompleteBox.ScrollIntoView(AutoCompleteBox.SelectedItem);
                    }
                    else
                    {
                        MethodAutoCompleteBox.SelectedIndex = Math.Max(0, MethodAutoCompleteBox.SelectedIndex - 1);
                        MethodAutoCompleteBox.ScrollIntoView(MethodAutoCompleteBox.SelectedItem);
                    }

                    return true;
                }
                case Key.Down:
                {
                    if (_acIsFuncC)
                    {
                        AutoCompleteBox.SelectedIndex = Math.Min(AutoCompleteBox.Items.Count - 1, AutoCompleteBox.SelectedIndex + 1);
                        AutoCompleteBox.ScrollIntoView(AutoCompleteBox.SelectedItem);
                    }
                    else
                    {
                        MethodAutoCompleteBox.SelectedIndex = Math.Min(MethodAutoCompleteBox.Items.Count - 1, MethodAutoCompleteBox.SelectedIndex + 1);
                        MethodAutoCompleteBox.ScrollIntoView(MethodAutoCompleteBox.SelectedItem);
                    }

                    return true;
                }
                case Key.Escape:
                {
                    HideIsac();
                    return true;
                }
                default:
                    // ignored
                    break;
            }

            return false;
        }

        private void ShowIsac(int forcedXPos = int.MaxValue)
        {
            if (IsacOpen)
                return;

            IsacOpen = true;
            ISAC_Grid.Visibility = Visibility.Visible;
            SetIsacPosition(forcedXPos);

            if (Program.OptionsObject.UIAnimations)
                _fadeIsacIn.Begin();
            else
                ISAC_Grid.Opacity = 1.0;
        }

        private void HideIsac()
        {
            if (!IsacOpen)
                return;

            IsacOpen = false;

            if (Program.OptionsObject.UIAnimations)
                _fadeIsacOut.Begin();
            else
            {
                ISAC_Grid.Opacity = 0.0;
                ISAC_Grid.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowAc(bool isFunc)
        {
            if (!_acOpen)
            {
                _acOpen = true;
                ACBorder.Height = 175.0;

                if (Program.OptionsObject.UIAnimations)
                    _fadeAcIn.Begin();

                else
                {
                    AutoCompleteBox.Width = 260.0;
                    MethodAutoCompleteBox.Width = 260.0;
                }
            }

            if (isFunc && _acIsFuncC || !_acOpen)
                return;

            if (isFunc)
            {
                if (_acIsFuncC)
                    return;

                _acIsFuncC = true;

                if (Program.OptionsObject.UIAnimations)
                    _fadeAcFuncCIn.Begin();
                else
                {
                    AutoCompleteBox.Opacity = 1.0;
                    MethodAutoCompleteBox.Opacity = 0.0;
                }
            }
            else
            {
                if (!_acIsFuncC)
                    return;

                _acIsFuncC = false;

                if (Program.OptionsObject.UIAnimations)
                    _fadeAcMethodCIn.Begin();
                else
                {
                    AutoCompleteBox.Opacity = 0.0;
                    MethodAutoCompleteBox.Opacity = 1.0;
                }
            }
        }

        private void HideAc()
        {
            if (!_acOpen)
                return;

            _acOpen = false;

            if (Program.OptionsObject.UIAnimations)
                _fadeAcOut.Begin();
            else
            {
                AutoCompleteBox.Width = 0.0;
                MethodAutoCompleteBox.Width = 0.0;
                ACBorder.Height = 0.0;
            }
        }

        private void ShowIs(string funcName, string funcDescription)
        {
            if (!_isOpen)
            {
                _isOpen = true;
                ISenseColumn.Width = new GridLength(0.0, GridUnitType.Auto);
            }

            IS_FuncName.Text = funcName;
            IS_FuncDescription.Text = funcDescription;
        }

        private void HideIs()
        {
            if (_isOpen)
            {
                _isOpen = false;
                ISenseColumn.Width = new GridLength(0.0);
            }

            IS_FuncDescription.Text = string.Empty;
            IS_FuncName.Text = string.Empty;
        }

        private void SetIsacPosition(int forcedXPos = int.MaxValue)
        {
            Point p;

            if (forcedXPos != int.MaxValue)
            {
                var tvp = new TextViewPosition(editor.TextArea.Caret.Position.Line, forcedXPos + 1);
                p = editor.TextArea.TextView.GetVisualPosition(tvp, VisualYPosition.LineBottom) - editor.TextArea.TextView.ScrollOffset;
            }
            else
                p = editor.TextArea.TextView.GetVisualPosition(editor.TextArea.Caret.Position, VisualYPosition.LineBottom) - editor.TextArea.TextView.ScrollOffset;

            IS_FuncDescription.Measure(new Size(double.MaxValue, double.MaxValue));

            var y = p.Y;
            var isacHeight = 0.0;

            if (_acOpen && _isOpen)
            {
                var isHeight = IS_FuncDescription.DesiredSize.Height;
                isacHeight = Math.Max(175.0, isHeight);
            }
            else if (_acOpen)
                isacHeight = 175.0;
            else if (_isOpen)
                isacHeight = IS_FuncDescription.DesiredSize.Height;

            if (y + isacHeight > editor.ActualHeight)
            {
                y = (editor.TextArea.TextView.GetVisualPosition(editor.TextArea.Caret.Position, VisualYPosition.LineTop) - editor.TextArea.TextView.ScrollOffset).Y;
                y -= isacHeight;
            }

            ISAC_Grid.Margin = new Thickness(p.X + ((LineNumberMargin)editor.TextArea.LeftMargins[0]).ActualWidth + 20.0, y, 0.0, 0.0);
        }

        private void FadeISACOut_Completed(object sender, EventArgs e)
        {
            ISAC_Grid.Visibility = Visibility.Collapsed;
        }

        private void FadeACOut_Completed(object sender, EventArgs e)
        {
            if (_fadeAcIn.GetCurrentState() != ClockState.Active)
                ACBorder.Height = 0.0;
        }

        private static bool IsValidFunctionChar(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || (c == '_');
        }
    }
}
