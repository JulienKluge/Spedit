using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;
using MahApps.Metro.Controls.Dialogs;
using Spedit.Utils.SPSyntaxTidy;
using System;
using System.IO;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Xceed.Wpf.AvalonDock.Layout;

namespace Spedit.UI.Components
{
    /// <summary>
    /// Interaction logic for EditorElement.xaml
    /// </summary>
    public partial class EditorElement
    {
        private readonly SPFoldingStrategy _foldingStrategy;
        private readonly ColorizeSelection _colorizeSelection;
        private readonly SPBracketSearcher _bracketSearcher;
        private readonly BracketHighlightRenderer _bracketHighlightRenderer;
        private readonly Timer _regularyTimer;
        private readonly Storyboard _fadeJumpGridIn;
        private readonly Storyboard _fadeJumpGridOut;
        private FileSystemWatcher _fileWatcher;
        private bool _wantFoldingUpdate;
        private bool _selectionIsHighlited;
        private bool _needsSave;
        private bool _jumpGridIsOpen;
        private double _lineHeight;
        private string _fullFilePath = "";

        public new LayoutDocument Parent;
        public FoldingManager FoldingManager;    
		public Timer AutoSaveTimer;

        public string FullFilePath
        {
            get { return _fullFilePath; }
            set
            {
                var fInfo = new FileInfo(value);

                _fullFilePath = fInfo.FullName;
                Parent.Title = fInfo.Name;

                if (_fileWatcher != null)
                    _fileWatcher.Path = fInfo.DirectoryName;
            }
        }

        public bool NeedsSave
        {
            get { return _needsSave; }
            set
            {
                if (!(value ^ _needsSave)) //when not changed
                    return;

                _needsSave = value;

                if (Parent == null)
                    return;

                if (_needsSave)
                    Parent.Title = "*" + Parent.Title;
                else
                    Parent.Title = Parent.Title.Trim('*');
            }
        }

        public EditorElement()
        {
            InitializeComponent();
        }

        public EditorElement(string filePath)
        {
            InitializeComponent();
			
			_bracketSearcher = new SPBracketSearcher();
            _bracketHighlightRenderer = new BracketHighlightRenderer(editor.TextArea.TextView);
            editor.TextArea.IndentationStrategy = new EditorIndetationStrategy();

            _fadeJumpGridIn = (Storyboard)Resources["FadeJumpGridIn"];
            _fadeJumpGridOut = (Storyboard)Resources["FadeJumpGridOut"];

            KeyDown += EditorElement_KeyDown;

            editor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            editor.TextArea.SelectionChanged += TextArea_SelectionChanged;
            editor.TextArea.PreviewKeyDown += TextArea_PreviewKeyDown;
            editor.PreviewMouseWheel += PrevMouseWheel;
            editor.MouseDown += editor_MouseDown;
            editor.TextArea.TextEntered += TextArea_TextEntered;

            var fInfo = new FileInfo(filePath);

            if (fInfo.Exists)
            {
                _fileWatcher = new FileSystemWatcher(fInfo.DirectoryName)
                {
                    IncludeSubdirectories = false,
                    NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite,
                    Filter = "*" + fInfo.Extension
                };

                _fileWatcher.Changed += fileWatcher_Changed;
                _fileWatcher.EnableRaisingEvents = true;
            }
            else
                _fileWatcher = null;

            _fullFilePath = filePath;
            editor.Options.ConvertTabsToSpaces = false;
            editor.Options.EnableHyperlinks = false;
			editor.Options.EnableEmailHyperlinks = false;
            editor.Options.HighlightCurrentLine = true;
            editor.Options.AllowScrollBelowDocument = true;
			editor.Options.ShowSpaces = Program.OptionsObject.EditorShowSpaces;
			editor.Options.ShowTabs = Program.OptionsObject.EditorShowTabs;
			editor.Options.IndentationSize = Program.OptionsObject.EditorIndentationSize;
			editor.TextArea.SelectionCornerRadius = 0.0;
            editor.Options.ConvertTabsToSpaces = Program.OptionsObject.EditorReplaceTabsToWhitespace;

			Brush currentLineBackground = new SolidColorBrush(Color.FromArgb(0x20, 0x88, 0x88, 0x88));
			Brush currentLinePenBrush = new SolidColorBrush(Color.FromArgb(0x30, 0x88, 0x88, 0x88));
			currentLinePenBrush.Freeze();
			var currentLinePen = new Pen(currentLinePenBrush, 1.0);
			currentLineBackground.Freeze();
			currentLinePen.Freeze();
			editor.TextArea.TextView.CurrentLineBackground = currentLineBackground;
			editor.TextArea.TextView.CurrentLineBorder = currentLinePen;

            editor.FontFamily = new FontFamily(Program.OptionsObject.EditorFontFamily);
            editor.WordWrap = Program.OptionsObject.EditorWordWrap;
            UpdateFontSize(Program.OptionsObject.EditorFontSize, false);
			
			_colorizeSelection = new ColorizeSelection();
            editor.TextArea.TextView.LineTransformers.Add(_colorizeSelection);
            editor.SyntaxHighlighting = new AeonEditorHighlighting();

            LoadAutoCompletes();

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var reader = FileReader.OpenStream(fs, Encoding.UTF8))
                {
                    var source = reader.ReadToEnd();
                    source = ((source.Replace("\r\n", "\n")).Replace("\r", "\n")).Replace("\n", "\r\n"); //normalize line endings
                    editor.Text = source;
                }
            }

            _needsSave = false;
			Language_Translate(true); //The Fontsize and content must be loaded

			var encoding = new UTF8Encoding(false);
            editor.Encoding = encoding; //let them read in whatever encoding they want - but save in UTF8

            FoldingManager = FoldingManager.Install(editor.TextArea);
            _foldingStrategy = new SPFoldingStrategy();
            _foldingStrategy.UpdateFoldings(FoldingManager, editor.Document);

            _regularyTimer = new Timer(500.0);
            _regularyTimer.Elapsed += regularyTimer_Elapsed;
            _regularyTimer.Start();

			AutoSaveTimer = new Timer();
			AutoSaveTimer.Elapsed += AutoSaveTimer_Elapsed;
			StartAutoSaveTimer();

            CompileBox.IsChecked = filePath.EndsWith(".sp");
        }

        private void AutoSaveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (NeedsSave)
                Dispatcher.Invoke(() => { Save(); });
        }

		public void StartAutoSaveTimer()
		{
		    if (!Program.OptionsObject.EditorAutoSave)
                return;

		    if (AutoSaveTimer.Enabled)
		        AutoSaveTimer.Stop();

		    AutoSaveTimer.Interval = 1000.0 * Program.OptionsObject.EditorAutoSaveInterval;
		    AutoSaveTimer.Start();
		}

        private void EditorElement_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.G)
                return;

            if (!Keyboard.IsKeyDown(Key.LeftCtrl) || (Keyboard.IsKeyDown(Key.RightAlt)))
                return;

            ToggleJumpGrid();
            e.Handled = true;
        }     

        public void ToggleJumpGrid()
        {
            if (_jumpGridIsOpen)
            {
                _fadeJumpGridOut.Begin();
                _jumpGridIsOpen = false;
            }
            else
            {
                _fadeJumpGridIn.Begin();
                _jumpGridIsOpen = true;
                JumpNumber.Focus();
                JumpNumber.SelectAll();
            }
        }

        private void JumpNumberKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            JumpToNumber(null, null);
            e.Handled = true;
        }

        private void JumpToNumber(object sender, RoutedEventArgs e)
        {
            int num;

            if (int.TryParse(JumpNumber.Text, out num))
            {
                if (LineJump.IsChecked != null && LineJump.IsChecked.Value)
                {
                    num = Math.Max(1, Math.Min(num, editor.LineCount));
                    var line = editor.Document.GetLineByNumber(num);

                    if (line != null)
                    {
                        editor.ScrollToLine(num);
                        editor.Select(line.Offset, line.Length);
                        editor.CaretOffset = line.Offset;
                    }
                }
                else
                {
                    num = Math.Max(0, Math.Min(num, editor.Text.Length));
                    var line = editor.Document.GetLineByOffset(num);

                    if (line != null)
                    {
                        editor.ScrollTo(line.LineNumber, 0);
                        editor.CaretOffset = num;
                    }
                }
            }

            ToggleJumpGrid();
            editor.Focus();
        }

        private void fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e == null)
                return;

            if (e.FullPath != _fullFilePath)
                return;

            bool reloadFile;

            if (_needsSave)
            {
                var result = MessageBox.Show(string.Format(Program.Translations.DFileChanged, _fullFilePath) + Environment.NewLine + Program.Translations.FileTryReload,
                    Program.Translations.FileChanged, MessageBoxButton.YesNo, MessageBoxImage.Asterisk);
                reloadFile = (result == MessageBoxResult.Yes);
            }
            else //when the user didnt changed anything, we just reload the file since we are intelligent...
            {
                reloadFile = true;
            }

            if (reloadFile)
            {
                Dispatcher.Invoke(() =>
                {
                    var isNotAccessed = true;

                    while (isNotAccessed)
                    {
                        try
                        {
                            FileStream stream;

                            using (stream = new FileStream(_fullFilePath, FileMode.OpenOrCreate))
                            {
                                editor.Load(stream);
                                NeedsSave = false;
                                isNotAccessed = false;
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        System.Threading.Thread.Sleep(100); //dont include System.Threading in the using directives, cause its onlyused once and the Timer class will double
                    }
                });
            }
        }

        private void regularyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
                    {
                        if (editor.SelectionLength > 0 && editor.SelectionLength < 50)
                        {
                            var selectionString = editor.SelectedText;

                            if (IsValidSearchSelectionString(selectionString))
                            {
                                _colorizeSelection.SelectionString = selectionString;
                                _colorizeSelection.HighlightSelection = true;
                                _selectionIsHighlited = true;
                                editor.TextArea.TextView.Redraw();
                            }
                            else
                            {
                                _colorizeSelection.HighlightSelection = false;
                                _colorizeSelection.SelectionString = string.Empty;

                                if (!_selectionIsHighlited)
                                    return;

                                editor.TextArea.TextView.Redraw();
                                _selectionIsHighlited = false;
                            }
                        }
                        else
                        {
                            _colorizeSelection.HighlightSelection = false;
                            _colorizeSelection.SelectionString = string.Empty;

                            if (!_selectionIsHighlited)
                                return;

                            editor.TextArea.TextView.Redraw();
                            _selectionIsHighlited = false;
                        }
                    });

            if (!_wantFoldingUpdate)
                return;

            _wantFoldingUpdate = false;

            try //this "solves" a racing-conditions error - i wasnt able to fix it till today.. 
            {
                Dispatcher.Invoke(() => { _foldingStrategy.UpdateFoldings(FoldingManager, editor.Document); });
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void Save(bool force = false)
        {
            if (!_needsSave && !force)
                return;

            if (_fileWatcher != null)
                _fileWatcher.EnableRaisingEvents = false;
            try
            {
                using (var fs = new FileStream(_fullFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    editor.Save((Stream) fs);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(Program.MainWindow,
                    Program.Translations.DSaveError + Environment.NewLine + "(" + e.Message + ")",
                    Program.Translations.SaveError,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            NeedsSave = false;

            if (_fileWatcher != null)
                _fileWatcher.EnableRaisingEvents = true;
        }

        public void UpdateFontSize(double size, bool updateLineHeight = true)
        {
            if (size > 2 && size < 31)
            {
                editor.FontSize = size;
                StatusLine_FontSize.Text = size.ToString("n0") + $" {Program.Translations.PtAbb}";
            }

            if (updateLineHeight)
                _lineHeight = editor.TextArea.TextView.DefaultLineHeight;
        }

		public void ToggleCommentOnLine()
		{
			var line = editor.Document.GetLineByOffset(editor.CaretOffset);
            var lineText = editor.Document.GetText(line.Offset, line.Length);
            var leadinggWhiteSpaces = 0;

		    foreach (var ch in lineText)
		        if (char.IsWhiteSpace(ch))
		            leadinggWhiteSpaces++;
		        else
		            break;

		    lineText = lineText.Trim();

		    if (lineText.Length > 1)
		        if (lineText[0] == '/' && lineText[1] == '/')
		            editor.Document.Remove(line.Offset + leadinggWhiteSpaces, 2);
		        else
		            editor.Document.Insert(line.Offset + leadinggWhiteSpaces, "//");
		    else
		        editor.Document.Insert(line.Offset + leadinggWhiteSpaces, "//");
		}

		public void DuplicateLine(bool down)
		{
			var line = editor.Document.GetLineByOffset(editor.CaretOffset);
            var lineText = editor.Document.GetText(line.Offset, line.Length);
			editor.Document.Insert(line.Offset, lineText + Environment.NewLine);

			if (down)
				editor.CaretOffset -= (line.Length + 1);
		}

		public void MoveLine(bool down)
		{
			var line = editor.Document.GetLineByOffset(editor.CaretOffset);

		    if (down)
		    {
		        if (line.NextLine == null)
		            editor.Document.Insert(line.Offset, Environment.NewLine);
		        else
		        {
		            var lineText = editor.Document.GetText(line.NextLine.Offset, line.NextLine.Length);
		            editor.Document.Remove(line.NextLine.Offset, line.NextLine.TotalLength);
		            editor.Document.Insert(line.Offset, lineText + Environment.NewLine);
		        }
		    }
			else
			{
				if (line.PreviousLine == null)
					editor.Document.Insert(line.Offset + line.Length, Environment.NewLine);
				else
				{
                    var insertOffset = line.PreviousLine.Offset;
                    var relativeCaretOffset = editor.CaretOffset - line.Offset;
                    var lineText = editor.Document.GetText(line.Offset, line.Length);
					editor.Document.Remove(line.Offset, line.TotalLength);
					editor.Document.Insert(insertOffset, lineText + Environment.NewLine);
					editor.CaretOffset = insertOffset + relativeCaretOffset;
				}
			}
		}

        public void Close(bool forcedToSave = false, bool checkSavings = true)
        {
            _regularyTimer.Stop();
            _regularyTimer.Close();

            if (_fileWatcher != null)
            {
                _fileWatcher.EnableRaisingEvents = false;
                _fileWatcher.Dispose();
                _fileWatcher = null;
            }

            if (checkSavings)
                if (_needsSave)
                    if (forcedToSave)
                    {
                        Save();
                    }
                    else
                    {
                        var result =
                            Program.MainWindow.ShowMessageAsync(
                                $"{Program.Translations.SavingFile} '" + Parent.Title.Trim('*') + "'", "",
                                MessageDialogStyle.AffirmativeAndNegative, Program.MainWindow.MetroDialogOptions);
                        result.Wait();

                        if (result.Result == MessageDialogResult.Affirmative)
                            Save();
                    }

            Program.MainWindow.EditorsReferences.Remove(this);
			var childs = Program.MainWindow.DockingPaneGroup.Children;

			foreach (var c in childs)
			{
			    var pane = c as LayoutDocumentPane;
			    pane?.Children.Remove(Parent);
			}

			Parent = null; //to prevent a ring depency which disables the GC from work
        }

        private void editor_TextChanged(object sender, EventArgs e)
        {
            _wantFoldingUpdate = true;
            NeedsSave = true;
        }

        private void Caret_PositionChanged(object sender, EventArgs e)
        {
            StatusLine_Coloumn.Text = $"{Program.Translations.ColAbb} {editor.TextArea.Caret.Column}";
            StatusLine_Line.Text = $"{Program.Translations.LnAbb} {editor.TextArea.Caret.Line}";

            EvaluateIntelliSense();

            var result = _bracketSearcher.SearchBracket(editor.Document, editor.CaretOffset);
            _bracketHighlightRenderer.SetHighlight(result);
        }

        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (Program.OptionsObject.EditorReformatLineAfterSemicolon)
                if (e.Text == ";")
                    if (editor.CaretOffset >= 0)
                    {
                        var line = editor.Document.GetLineByOffset(editor.CaretOffset);
                        var leadingIndentation =
                            editor.Document.GetText(TextUtilities.GetLeadingWhitespace(editor.Document, line));
                        var newLineStr = leadingIndentation + SPSyntaxTidy.TidyUp(editor.Document.GetText(line)).Trim();
                        editor.Document.Replace(line, newLineStr);
                    }

            switch (e.Text)
            {
                case "}":
                    editor.TextArea.IndentationStrategy.IndentLine(editor.Document,
                        editor.Document.GetLineByOffset(editor.CaretOffset));
                    _foldingStrategy.UpdateFoldings(FoldingManager, editor.Document);
                    break;
                case "{":
                    if (Program.OptionsObject.EditorAutoCloseBrackets)
                    {
                        editor.Document.Insert(editor.CaretOffset, "}");
                        editor.CaretOffset -= 1;
                    }
                    _foldingStrategy.UpdateFoldings(FoldingManager, editor.Document);
                    break;
                default:
                    if (Program.OptionsObject.EditorAutoCloseBrackets)
                        switch (e.Text)
                        {
                            case "(":
                                editor.Document.Insert(editor.CaretOffset, ")");
                                editor.CaretOffset -= 1;
                                break;
                            case "[":
                                editor.Document.Insert(editor.CaretOffset, "]");
                                editor.CaretOffset -= 1;
                                break;
                            default:
                                // ignored
                                break;
                        }
                    break;
            }

            if (!Program.OptionsObject.EditorAutoCloseStringChars)
                return;

            switch (e.Text)
            {
                case "\"":
                {
                    var line = editor.Document.GetLineByOffset(editor.CaretOffset);
                    var lineText = editor.Document.GetText(line.Offset, editor.CaretOffset - line.Offset);

                    if (lineText.Length > 0)
                        if (lineText[Math.Max(lineText.Length - 2, 0)] != '\\')
                        {
                            editor.Document.Insert(editor.CaretOffset, "\"");
                            editor.CaretOffset -= 1;
                        }
                }
                    break;
                case "'":
                {
                    var line = editor.Document.GetLineByOffset(editor.CaretOffset);
                    var lineText = editor.Document.GetText(line.Offset, editor.CaretOffset - line.Offset);

                    if (lineText.Length > 0)
                        if (lineText[Math.Max(lineText.Length - 2, 0)] != '\\')
                        {
                            editor.Document.Insert(editor.CaretOffset, "'");
                            editor.CaretOffset -= 1;
                        }
                }
                    break;
                default:
                    // ignored
                    break;
            }
        }

        private void TextArea_SelectionChanged(object sender, EventArgs e)
        {
            StatusLine_SelectionLength.Text = $"{Program.Translations.LenAbb} {editor.SelectionLength}";
        }

        private void PrevMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                UpdateFontSize(editor.FontSize + Math.Sign(e.Delta));
                e.Handled = true;
            }
            else
            {
                if (_lineHeight == 0.0)
                    _lineHeight = editor.TextArea.TextView.DefaultLineHeight;

                editor.ScrollToVerticalOffset(editor.VerticalOffset - Math.Sign((double)e.Delta) * _lineHeight * Program.OptionsObject.EditorScrollLines);
                e.Handled = true;
            }

            HideIsac();
        }

        private void editor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HideIsac();
        }

        private void TextArea_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = ISAC_EvaluateKeyDownEvent(e.Key);

            if (e.Handled)
                return;

            if (!e.KeyboardDevice.IsKeyDown(Key.LeftCtrl))
                return;

            if (e.KeyboardDevice.IsKeyDown(Key.LeftAlt))
            {
                switch (e.Key)
                {
                    case Key.Down:
                        DuplicateLine(true);
                        e.Handled = true;
                        break;
                    case Key.Up:
                        DuplicateLine(false);
                        e.Handled = true;
                        break;
                    default:
                        // ignored
                        break;
                }
            }
            else
            {
                switch (e.Key)
                {
                    case Key.Down:
                        MoveLine(true);
                        e.Handled = true;
                        break;
                    case Key.Up:
                        MoveLine(false);
                        e.Handled = true;
                        break;
                    default:
                        // ignored
                        break;
                }
            }
        }

        private void HandleContextMenuCommand(object sender, RoutedEventArgs e)
        {
            switch ((string) ((MenuItem) sender).Tag)
            {
                case "0":
                {
                    editor.Undo();
                    break;
                }
                case "1":
                {
                    editor.Redo();
                    break;
                }
                case "2":
                {
                    editor.Cut();
                    break;
                }
                case "3":
                {
                    editor.Copy();
                    break;
                }
                case "4":
                {
                    editor.Paste();
                    break;
                }
                case "5":
                {
                    editor.SelectAll();
                    break;
                }
                default:
                    // ignored
                    break;
            }
        }

        private void ContextMenu_Opening(object sender, RoutedEventArgs e)
        {
            ((MenuItem)((ContextMenu)sender).Items[0]).IsEnabled = editor.CanUndo;
            ((MenuItem)((ContextMenu)sender).Items[1]).IsEnabled = editor.CanRedo;
        }

        private static bool IsValidSearchSelectionString(string s)
        {
            var length = s.Length;

            for (var i = 0; i < length; ++i)
                if (
                    !(s[i] >= 'a' && s[i] <= 'z' || s[i] >= 'A' && s[i] <= 'Z' || s[i] >= '0' && s[i] <= '9' ||
                      s[i] == '_'))
                    return false;

            return true;
        }

		public void Language_Translate(bool initial = false)
		{
			if (Program.Translations.IsDefault)
				return;

			MenuC_Undo.Header = Program.Translations.Undo;
			MenuC_Redo.Header = Program.Translations.Redo;
			MenuC_Cut.Header = Program.Translations.Cut;
			MenuC_Copy.Header = Program.Translations.Copy;
			MenuC_Paste.Header = Program.Translations.Paste;
			MenuC_SelectAll.Header = Program.Translations.SelectAll;
			CompileBox.Content = Program.Translations.Compile;

		    if (initial)
                return;

		    StatusLine_Coloumn.Text = $"{Program.Translations.ColAbb} {editor.TextArea.Caret.Column}";
		    StatusLine_Line.Text = $"{Program.Translations.LnAbb} {editor.TextArea.Caret.Line}";
		    StatusLine_FontSize.Text = editor.FontSize.ToString("n0") + $" {Program.Translations.PtAbb}";
		}
    }

    public class ColorizeSelection : DocumentColorizingTransformer
    {
        public string SelectionString = string.Empty;
        public bool HighlightSelection;

        protected override void ColorizeLine(DocumentLine line)
        {
            if (!HighlightSelection)
                return;

            if (string.IsNullOrWhiteSpace(SelectionString))
                return;

            var lineStartOffset = line.Offset;
            var text = CurrentContext.Document.GetText(line);
            var start = 0;
            int index;

            while ((index = text.IndexOf(SelectionString, start, StringComparison.Ordinal)) >= 0)
            {
                ChangeLinePart(
                    lineStartOffset + index,
                    lineStartOffset + index + SelectionString.Length,
                    element => { element.BackgroundBrush = Brushes.LightGray; });
                start = index + 1;
            }
        }
    }
}
