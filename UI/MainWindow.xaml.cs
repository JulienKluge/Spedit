using MahApps.Metro;
using Spedit.UI.Components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Xceed.Wpf.AvalonDock.Layout;
using Spedit.Interop.Updater; //not delete!

namespace Spedit.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {      
        private readonly bool _fullyInitialized;
        private readonly Storyboard _blendOverEffect;
        private readonly Storyboard _fadeFindReplaceGridIn;
        private readonly Storyboard _fadeFindReplaceGridOut;
        private readonly Storyboard _enableServerAnim;
        private readonly Storyboard _disableServerAnim;

        private ObservableCollection<string> _compileButtonDict = new ObservableCollection<string> { Program.Translations.CompileAll, Program.Translations.CompileCurr };
        private ObservableCollection<string> _actionButtonDict = new ObservableCollection<string> { Program.Translations.Copy, Program.Translations.FTPUp, Program.Translations.StartServer };
        private ObservableCollection<string> _findReplaceButtonDict = new ObservableCollection<string> { Program.Translations.Replace, Program.Translations.ReplaceAll };

        public List<EditorElement> EditorsReferences = new List<EditorElement>();

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(SplashScreen sc)
        {
            InitializeComponent();

            if (Program.OptionsObject.ProgramAccentColor != "Red" || Program.OptionsObject.ProgramTheme != "BaseDark")
            {
                ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(Program.OptionsObject.ProgramAccentColor),
                    ThemeManager.GetAppTheme(Program.OptionsObject.ProgramTheme));
            }

            ObjectBrowserColumn.Width = new GridLength(Program.OptionsObject.ProgramObjectbrowserWidth,
                GridUnitType.Pixel);
            var heightDescriptor = DependencyPropertyDescriptor.FromProperty(ColumnDefinition.WidthProperty,
                typeof(ItemsControl));
            heightDescriptor.AddValueChanged(EditorObjectBrowserGrid.ColumnDefinitions[1],
                EditorObjectBrowserGridRow_WidthChanged);
            FillConfigMenu();
            CompileButton.ItemsSource = _compileButtonDict;
            CompileButton.SelectedIndex = 0;
            CActionButton.ItemsSource = _actionButtonDict;
            CActionButton.SelectedIndex = 0;
            ReplaceButton.ItemsSource = _findReplaceButtonDict;
            ReplaceButton.SelectedIndex = 0;

            if (Program.OptionsObject.UIShowToolBar)
                Win_ToolBar.Height = double.NaN;

            MetroDialogOptions.AnimateHide = MetroDialogOptions.AnimateShow = false;
            _blendOverEffect = (Storyboard) Resources["BlendOverEffect"];
            _fadeFindReplaceGridIn = (Storyboard) Resources["FadeFindReplaceGridIn"];
            _fadeFindReplaceGridOut = (Storyboard) Resources["FadeFindReplaceGridOut"];
            _enableServerAnim = (Storyboard) Resources["EnableServerAnim"];
            _disableServerAnim = (Storyboard) Resources["DisableServerAnim"];
            ChangeObjectBrowserToDirectory(Program.OptionsObject.ProgramObjectBrowserDirectory);
            Language_Translate(true);
#if DEBUG
            TryLoadSourceFile(@"C:\Users\Jelle\Desktop\scripting\AeroControler.sp", false);
#endif
            if (Program.OptionsObject.LastOpenFiles != null)
                foreach (var filePath in Program.OptionsObject.LastOpenFiles)
                    TryLoadSourceFile(filePath, false);

            var args = Environment.GetCommandLineArgs();

            for (var i = 0; i < args.Length; ++i)
                if (!args[i].EndsWith("exe"))
                    TryLoadSourceFile(args[i], false, true, (i == 0));

            sc.Close(TimeSpan.FromMilliseconds(500.0));
            StartBackgroundParserThread();
            _fullyInitialized = true;
        }

        public bool TryLoadSourceFile(string filePath, bool useBlendoverEffect = true, bool tryOpenIncludes = true, bool selectMe = false)
        {
            var fileInfo = new FileInfo(filePath);

            if (!fileInfo.Exists)
                return false;

            var extension = fileInfo.Extension.ToLowerInvariant().Trim(new char[] { '.', ' ' });
            switch (extension)
            {
                case "sp":
                case "inc":
                case "txt":
                case "cfg":
                case "ini":
                    var finalPath = fileInfo.FullName;
                    var editors = GetAllEditorElements();

                    if (editors != null)
                    {
                        foreach (var element in editors)
                        {
                            if (element.FullFilePath != finalPath)
                                continue;

                            if (selectMe)
                                element.Parent.IsSelected = true;

                            return false;
                        }
                    }

                    AddEditorElement(finalPath, fileInfo.Name, selectMe);

                    if (tryOpenIncludes && Program.OptionsObject.ProgramOpenCustomIncludes)
                    {
                        using (var textReader = fileInfo.OpenText())
                        {
                            var source = Regex.Replace(textReader.ReadToEnd(), @"/\*.*?\*/", string.Empty, RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline);
                            var regex = new Regex(@"^\s*\#include\s+((\<|"")(?<name>.+?)(\>|""))", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);
                            var mc = regex.Matches(source);

                            for (var i = 0; i < mc.Count; ++i)
                            {
                                try
                                {
                                    var fileName = mc[i].Groups["name"].Value;

                                    if (!(fileName.EndsWith(".inc", StringComparison.InvariantCultureIgnoreCase) || fileName.EndsWith(".sp", StringComparison.InvariantCultureIgnoreCase)))
                                        fileName = fileName + ".inc";

                                    if (fileInfo.DirectoryName != null)
                                        fileName = Path.Combine(fileInfo.DirectoryName, fileName);

                                    TryLoadSourceFile(fileName, false, Program.OptionsObject.ProgramOpenIncludesRecursively);
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }
                        }
                    }
                    break;
                case "smx":
                    var layoutDocument = new LayoutDocument {Title = "DASM: " + fileInfo.Name};
                    var dasmElement = new DASMElement(fileInfo);
                    layoutDocument.Content = dasmElement;
                    DockingPane.Children.Add(layoutDocument);
                    DockingPane.SelectedContentIndex = DockingPane.ChildrenCount - 1;
                    break;
                default:
                    // ignored
                    break;
            }

            if (useBlendoverEffect)
                _blendOverEffect.Begin();

            return true;
        }

        public void AddEditorElement(string filePath, string name, bool selectMe)
        {
            var layoutDocument = new LayoutDocument {Title = name};
            layoutDocument.Closing += layoutDocument_Closing;
            layoutDocument.ToolTip = filePath;
            var editor = new EditorElement(filePath) {Parent = layoutDocument};
            layoutDocument.Content = editor;
            EditorsReferences.Add(editor);
            DockingPane.Children.Add(layoutDocument);

            if (selectMe)
				layoutDocument.IsSelected = true;
        }

        private void DockingManager_ActiveContentChanged(object sender, EventArgs e)
        {
            UpdateWindowTitle();
            var element = GetCurrentEditorElement();
            element?.editor.Focus();
        }

        private void DockingManager_DocumentClosing(object sender, Xceed.Wpf.AvalonDock.DocumentClosingEventArgs e)
        {
            var element = e.Document.Content as EditorElement;
            element?.Close();
            UpdateWindowTitle();
        }

        private static void layoutDocument_Closing(object sender, CancelEventArgs e)
        {
			e.Cancel = true;
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            _backgroundParserThread?.Abort();
            _parseDistributorTimer?.Stop();
            ServerCheckThread?.Abort(); //a join would not work, so we have to be..forcefully...
            var lastOpenFiles = new List<string>();
            var editors = GetAllEditorElements();
            bool? saveUnsaved = null;

			if (editors != null)
			{
				foreach (var element in editors)
				{
				    if (!File.Exists(element.FullFilePath))
                        continue;

				    lastOpenFiles.Add(element.FullFilePath);

				    if (element.NeedsSave)
				    {
				        if (saveUnsaved == null)
				        {
				            var result = MessageBox.Show(this, Program.Translations.SavingUFiles, Program.Translations.Saving, MessageBoxButton.YesNo, MessageBoxImage.Question);
				            saveUnsaved = result == MessageBoxResult.Yes;
				        }

				        if (saveUnsaved.Value)
				            element.Close(true);
				        else
				            element.Close(false, false);
				    }
				    else
				        element.Close(false, false);
				}
			}

            Program.OptionsObject.LastOpenFiles = lastOpenFiles.ToArray();
#if !DEBUG
            if (Program.UpdateStatus.IsAvailable)
            {
                UpdateWindow updateWin = new UpdateWindow(Program.UpdateStatus) { Owner = this };
                updateWin.ShowDialog();
            }
#endif
        }

        private void MetroWindow_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files == null)
                return;

            Activate();
            Focus();

            for (var i = 0; i < files.Length; ++i)
                TryLoadSourceFile(files[i], i == 0, true, i == 0);
        }

        public static void ProcessUITasks()
        {
            var frame = new DispatcherFrame();

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate
            {
                frame.Continue = false;
                return null;
            }), null);

            Dispatcher.PushFrame(frame);
        }

        private void ErrorResultGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var row = ((ErrorDataGridRow)ErrorResultGrid.SelectedItem);

            if (row == null)
                return;

            var fileName = row.File;
            var editors = GetAllEditorElements();

            if (editors == null)
                return;

            foreach (var element in editors)
            {
                if (element.FullFilePath != fileName)
                    continue;

                element.Parent.IsSelected = true;

                var line = GetLineInteger(row.Line);

                if (line <= 0 || line > element.editor.LineCount)
                    continue;

                var lineObj = element.editor.Document.Lines[line - 1];

                element.editor.ScrollToLine(line - 1);
                element.editor.Select(lineObj.Offset, lineObj.Length);
            }
        }

        private void CloseErrorResultGrid(object sender, RoutedEventArgs e)
        {
            CompileOutputRow.Height = new GridLength(8.0);
        }

		private void EditorObjectBrowserGridRow_WidthChanged(object sender, EventArgs e)
		{
			if (_fullyInitialized)
				Program.OptionsObject.ProgramObjectbrowserWidth = ObjectBrowserColumn.Width.Value;
		}

		private void UpdateWindowTitle()
        {
            var element = GetCurrentEditorElement();
            string outString;

            if (element == null)
                outString = "SPEdit";
            else
                outString = element.FullFilePath + " - SPEdit";

            if (ServerIsRunning)
                outString = $"{outString} ({Program.Translations.ServerRunning})";

            Title = outString;
        }

        private static int GetLineInteger(string lineStr)
        {
            var end = 0;

            for (var i = 0; i < lineStr.Length; ++i)
                if (lineStr[i] >= '0' && lineStr[i] <= '9')
                    end = i;
                else
                    break;

            int line;

            if (int.TryParse(lineStr.Substring(0, end + 1), out line))
                return line;

            return -1;
        }
    }
}
