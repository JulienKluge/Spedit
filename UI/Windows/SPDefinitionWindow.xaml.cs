using MahApps.Metro.Controls;
using SourcepawnCondenser.SourcemodDefinition;
using System.Text;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro;

namespace Spedit.UI.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class SPDefinitionWindow
    {
        private readonly SPDefEntry[] _defArray;
        private readonly ListViewItem[] _items;
        private readonly Timer _searchTimer = new Timer(1000.0);
        private readonly Brush _errorSearchBoxBrush = new SolidColorBrush(Color.FromArgb(0x50, 0xA0, 0x30, 0));

        public SPDefinitionWindow()
        {
            InitializeComponent();
            Language_Translate();

            if (Program.OptionsObject.ProgramAccentColor != "Red" || Program.OptionsObject.ProgramTheme != "BaseDark")
                ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(Program.OptionsObject.ProgramAccentColor),
                    ThemeManager.GetAppTheme(Program.OptionsObject.ProgramTheme));

            _errorSearchBoxBrush.Freeze();

            var def = Program.Configs[Program.SelectedConfig].GetSMDef();

            if (def == null)
            {
                MessageBox.Show(Program.Translations.ConfigWrongPars, Program.Translations.Error, MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                Close();
                return;
            }

            var defList = def.Functions.Select(t => (SPDefEntry) t).ToList();
            defList.AddRange(def.Constants.Select(t => (SPDefEntry) t));
            defList.AddRange(def.Enums.Select(t => (SPDefEntry) t));
            defList.AddRange(def.Defines.Select(t => (SPDefEntry) t));
            defList.AddRange(def.Structs.Select(t => (SPDefEntry) t));
            defList.AddRange(def.Methodmaps.Select(t => (SPDefEntry) t));
            defList.AddRange(def.Typedefs.Select(t => (SPDefEntry) t));

            foreach (var mm in def.Methodmaps)
            {
                defList.AddRange(mm.Methods.Select(t => (SPDefEntry) t));
                defList.AddRange(mm.Fields.Select(t => (SPDefEntry) t));
            }

            foreach (var e in defList)
                if (string.IsNullOrWhiteSpace(e.Name))
                    e.Name = $"--{Program.Translations.NoName}--";

            defList.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            _defArray = defList.ToArray();
            var defArrayLength = _defArray.Length;
            _items = new ListViewItem[defArrayLength];

            for (var i = 0; i < defArrayLength; ++i)
            {
                _items[i] = new ListViewItem {Content = _defArray[i].Name, Tag = _defArray[i].Entry};
                SPBox.Items.Add(_items[i]);
            }

            _searchTimer.Elapsed += searchTimer_Elapsed;
        }

        private void searchTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DoSearch();
            _searchTimer.Stop();
        }

        private void SPFunctionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var obj = SPBox.SelectedItem;

            if (obj == null)
                return;

            var item = (ListViewItem) obj;
            var tagValue = item.Tag;

            if (tagValue != null)
            {
                var value = tagValue as SMFunction;

                if (value != null)
                {
                    var sm = value;
                    SPNameBlock.Text = sm.Name;
                    SPFullNameBlock.Text = sm.FullName;
                    SPFileBlock.Text = sm.File + ".inc" +
                                       $" ({string.Format(Program.Translations.PosLen, sm.Index, sm.Length)})";
                    SPTypeBlock.Text = "Function";
                    SPCommentBox.Text = sm.CommentString;
                    return;
                }

                var constant = tagValue as SMConstant;

                if (constant != null)
                {
                    var sm = constant;
                    SPNameBlock.Text = sm.Name;
                    SPFullNameBlock.Text = string.Empty;
                    SPFileBlock.Text = sm.File + ".inc" +
                                       $" ({string.Format(Program.Translations.PosLen, sm.Index, sm.Length)})";
                    SPTypeBlock.Text = "Constant";
                    SPCommentBox.Text = string.Empty;
                    return;
                }

                var @enum = tagValue as SMEnum;

                if (@enum != null)
                {
                    var sm = @enum;
                    SPNameBlock.Text = sm.Name;
                    SPFullNameBlock.Text = string.Empty;
                    SPFileBlock.Text = sm.File + ".inc" +
                                       $" ({string.Format(Program.Translations.PosLen, sm.Index, sm.Length)})";
                    SPTypeBlock.Text = "Enum " + sm.Entries.Length + " entries";
                    var outString = new StringBuilder();

                    for (var i = 0; i < sm.Entries.Length; ++i)
                    {
                        outString.Append((i + ".").PadRight(5, ' '));
                        outString.AppendLine(sm.Entries[i]);
                    }

                    SPCommentBox.Text = outString.ToString();
                    return;
                }

                var @struct = tagValue as SMStruct;

                if (@struct != null)
                {
                    var sm = @struct;
                    SPNameBlock.Text = sm.Name;
                    SPFullNameBlock.Text = string.Empty;
                    SPFileBlock.Text = sm.File + ".inc" +
                                       $" ({string.Format(Program.Translations.PosLen, sm.Index, sm.Length)})";
                    SPTypeBlock.Text = "Struct";
                    SPCommentBox.Text = string.Empty;
                    return;
                }

                var define = tagValue as SMDefine;

                if (define != null)
                {
                    var sm = define;
                    SPNameBlock.Text = sm.Name;
                    SPFullNameBlock.Text = string.Empty;
                    SPFileBlock.Text = sm.File + ".inc" +
                                       $" ({string.Format(Program.Translations.PosLen, sm.Index, sm.Length)})";
                    SPTypeBlock.Text = "Definition";
                    SPCommentBox.Text = string.Empty;
                    return;
                }

                var methodmap = tagValue as SMMethodmap;

                if (methodmap != null)
                {
                    var sm = methodmap;
                    SPNameBlock.Text = sm.Name;
                    SPFullNameBlock.Text = $"{Program.Translations.TypeStr}: " + sm.Type +
                                           $" - {Program.Translations.InheritedFrom}: {sm.InheritedType}";
                    SPFileBlock.Text = sm.File + ".inc" +
                                       $" ({string.Format(Program.Translations.PosLen, sm.Index, sm.Length)})";
                    SPTypeBlock.Text = "Methodmap " + sm.Methods.Count + " methods - " + sm.Fields.Count + " fields";
                    var outString = new StringBuilder();
                    outString.AppendLine("Methods:");

                    foreach (var m in sm.Methods)
                        outString.AppendLine(m.FullName);

                    outString.AppendLine();
                    outString.AppendLine("Fields:");

                    foreach (var f in sm.Fields)
                        outString.AppendLine(f.FullName);

                    SPCommentBox.Text = outString.ToString();
                    return;
                }

                var method = tagValue as SMMethodmapMethod;

                if (method != null)
                {
                    var sm = method;
                    SPNameBlock.Text = sm.Name;
                    SPFullNameBlock.Text = sm.FullName;
                    SPFileBlock.Text = sm.File + ".inc" +
                                       $" ({string.Format(Program.Translations.PosLen, sm.Index, sm.Length)})";
                    SPTypeBlock.Text = $"{Program.Translations.MethodFrom} {sm.MethodmapName}";
                    SPCommentBox.Text = sm.CommentString;
                    return;
                }

                var field = tagValue as SMMethodmapField;

                if (field != null)
                {
                    var sm = field;
                    SPNameBlock.Text = sm.Name;
                    SPFullNameBlock.Text = sm.FullName;
                    SPFileBlock.Text = sm.File + ".inc" +
                                       $" ({string.Format(Program.Translations.PosLen, sm.Index, sm.Length)})";
                    SPTypeBlock.Text = $"{Program.Translations.PropertyFrom} {sm.MethodmapName}";
                    SPCommentBox.Text = string.Empty;
                    return;
                }

                var typedef = tagValue as SMTypedef;

                if (typedef != null)
                {
                    var sm = typedef;
                    SPNameBlock.Text = sm.Name;
                    SPFullNameBlock.Text = string.Empty;
                    SPFileBlock.Text = sm.File + ".inc" +
                                       $" ({string.Format(Program.Translations.PosLen, sm.Index, sm.Length)})";
                    SPTypeBlock.Text = "Typedef/Typeset";
                    SPCommentBox.Text = sm.FullName;
                    return;
                }

                var s = tagValue as string;

                if (s != null)
                {
                    SPNameBlock.Text = (string) item.Content;
                    SPFullNameBlock.Text = s;
                    SPFileBlock.Text = string.Empty;
                    SPCommentBox.Text = string.Empty;
                    return;
                }
            }

            SPNameBlock.Text = (string) item.Content;
            SPFullNameBlock.Text = string.Empty;
            SPFileBlock.Text = string.Empty;
            SPTypeBlock.Text = string.Empty;
            SPCommentBox.Text = string.Empty;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SPProgress.IsIndeterminate = true;
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void DoSearch()
        {
            Dispatcher.Invoke(() =>
            {
                var itemCount = _defArray.Length;
                var searchString = SPSearchBox.Text.ToLowerInvariant();
                var foundOccurence = false;

                SPBox.Items.Clear();

                for (var i = 0; i < itemCount; ++i)
                    if (_defArray[i].Name.ToLowerInvariant().Contains(searchString))
                    {
                        foundOccurence = true;
                        SPBox.Items.Add(_items[i]);
                    }

                SPSearchBox.Background = foundOccurence ? Brushes.Transparent : _errorSearchBoxBrush;
                SPProgress.IsIndeterminate = false;
            });
        }

        private void Language_Translate()
        {
            TextBoxHelper.SetWatermark(SPSearchBox, Program.Translations.Search);
            /*if (Program.Translations.IsDefault)
            {
                return;
            }*/
        }

        private class SPDefEntry
        {
            public string Name;
            public object Entry;

            public static explicit operator SPDefEntry(SMFunction func)
            {
                return new SPDefEntry() {Name = func.Name, Entry = func};
            }

            public static explicit operator SPDefEntry(SMConstant sm)
            {
                return new SPDefEntry() {Name = sm.Name, Entry = sm};
            }

            public static explicit operator SPDefEntry(SMDefine sm)
            {
                return new SPDefEntry() {Name = sm.Name, Entry = sm};
            }

            public static explicit operator SPDefEntry(SMEnum sm)
            {
                return new SPDefEntry() {Name = sm.Name, Entry = sm};
            }

            public static explicit operator SPDefEntry(SMStruct sm)
            {
                return new SPDefEntry() {Name = sm.Name, Entry = sm};
            }

            public static explicit operator SPDefEntry(SMMethodmap sm)
            {
                return new SPDefEntry() {Name = sm.Name, Entry = sm};
            }

            public static explicit operator SPDefEntry(SMMethodmapMethod sm)
            {
                return new SPDefEntry() {Name = sm.Name, Entry = sm};
            }

            public static explicit operator SPDefEntry(SMMethodmapField sm)
            {
                return new SPDefEntry() {Name = sm.Name, Entry = sm};
            }

            public static explicit operator SPDefEntry(SMTypedef sm)
            {
                return new SPDefEntry() {Name = sm.Name, Entry = sm};
            }
        }
    }
}
