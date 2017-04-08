using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using smxdasm;

namespace Spedit.UI.Components
{
    /// <summary>
    /// Interaction logic for DASMElement.xaml
    /// </summary>
    public partial class DASMElement
    {
        private readonly StringBuilder _detailBuffer = new StringBuilder();
        private SmxFile _file;
        private double _lineHeight;

        public delegate void DrawNodeFn();

        public DASMElement()
        {
            InitializeComponent();
        }

        public DASMElement(FileInfo fInfo)
        {
            InitializeComponent();

            LoadFile(fInfo);
            detailbox_.PreviewMouseWheel += PrevMouseWheel;
            detailbox_.Options.EnableHyperlinks = false;
            detailbox_.Options.HighlightCurrentLine = true;
            detailbox_.TextArea.SelectionCornerRadius = 0.0;
            detailbox_.SyntaxHighlighting = new DASMHighlighting();
        }

        private void LoadFile(FileInfo fInfo)
        {
            try
            {
                using (var stream = fInfo.OpenRead())
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        _file = new SmxFile(reader);
                    }
                }
            }
            catch (Exception e)
            {
                detailbox_.Text = Program.Translations.ErrorFileLoadProc + Environment.NewLine + Environment.NewLine +
                                  $"{Program.Translations.Details}: " + e.Message;
                return;
            }

            RenderFile();
        }

        private void PrevMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_lineHeight == 0.0)
                _lineHeight = detailbox_.TextArea.TextView.DefaultLineHeight;

            detailbox_.ScrollToVerticalOffset(detailbox_.VerticalOffset - (Math.Sign((double)e.Delta) * _lineHeight * Program.OptionsObject.EditorScrollLines));
            e.Handled = true;
        }

        private void RenderFile()
        {
            var roots = new Dictionary<string, TreeViewItem>();
            var node = new TreeViewItem
            {
                Header = "(header)",
                Tag = new NodeData(RenderFileDetail, null)
            }; //hehe

            treeview_.Items.Clear();
            treeview_.Items.Add(node);      

            // Add section headers.
            foreach (var sec in _file.Header.Sections)
            {
                var section = sec;
                var root = new TreeViewItem
                {
                    Header = section.Name,
                    Tag = new NodeData(delegate
                    {
                        RenderSectionHeaderDetail(section);
                        EndDetailUpdate();
                    }, section)
                };

                roots[section.Name] = root;
                treeview_.Items.Add(root);
            }

            // Add specific sections.
            if (roots.ContainsKey(".natives"))
                RenderNativeList(roots[".natives"], _file.Natives);
            if (roots.ContainsKey(".tags"))
                RenderTagList(roots[".tags"], _file.Tags);
            if (roots.ContainsKey(".pubvars"))
                RenderPubvarList(roots[".pubvars"], _file.Pubvars);
            if (roots.ContainsKey(".publics"))
                RenderPublicsList(roots[".publics"], _file.Publics);
            if (roots.ContainsKey(".code"))
                RenderCodeSection(roots[".code"], _file.CodeV1);
            if (roots.ContainsKey(".data"))
                RenderDataList(roots[".data"], _file.Data);
            if (roots.ContainsKey(".names"))
                RenderNamesList(roots[".names"], _file.Names);
            if (roots.ContainsKey(".dbg.files"))
                RenderDebugFiles(roots[".dbg.files"], _file.DebugFiles);
            if (roots.ContainsKey(".dbg.lines"))
                RenderDebugLines(roots[".dbg.lines"], _file.DebugLines);
            if (roots.ContainsKey(".dbg.info"))
                RenderDebugInfo(roots[".dbg.info"], _file.DebugInfo);
            if (roots.ContainsKey(".dbg.strings"))
                RenderNamesList(roots[".dbg.strings"], _file.DebugNames);
            if (roots.ContainsKey(".dbg.symbols"))
                RenderDebugSymbols(roots[".dbg.symbols"], _file.DebugSymbols);
            if (roots.ContainsKey(".dbg.natives"))
                RenderDebugNatives(roots[".dbg.natives"], _file.DebugNatives);

            RenderFileDetail();
        }

        private void StartDetailUpdate()
        {
            _detailBuffer.Clear();
        }

        private void StartDetail(string fmt, params object[] args)
        {
            StartDetailUpdate();
            AddDetailLine(fmt, args);
        }

        private void AddDetailLine(string fmt, params object[] args)
        {
            _detailBuffer.Append(string.Format(fmt, args) + "\r\n");
        }

        private void EndDetailUpdate()
        {
            detailbox_.Text = _detailBuffer.ToString();
        }

        private void RenderFileDetail()
        {
            StartDetailUpdate();

            AddDetailLine("magic = 0x{0:x}", _file.Header.Magic);
            AddDetailLine("version = 0x{0:x}", _file.Header.Version);
            AddDetailLine("compression = {0} (0x{1:x})", _file.Header.Compression.ToString(), _file.Header.Compression);
            AddDetailLine("disksize = {0} bytes", _file.Header.DiskSize);
            AddDetailLine("imagesize = {0} bytes", _file.Header.ImageSize);
            AddDetailLine("sections = {0}", _file.Header.num_sections);
            AddDetailLine("stringtab = @{0}", _file.Header.stringtab);
            AddDetailLine("dataoffs = @{0}", _file.Header.dataoffs);

            EndDetailUpdate();
        }

        private void RenderSectionHeaderDetail(SectionEntry header)
        {
            StartDetailUpdate();

            AddDetailLine(".nameoffs = 0x{0:x} ; \"{1}\"", header.nameoffs, header.Name);
            AddDetailLine(".dataoffs = 0x{0:x}", header.dataoffs);
            AddDetailLine(".size = {0} bytes", header.Size);
        }

        private void RenderByteView(BinaryReader reader, int size)
        {
            var ndigits = $"{size:x}".Length;
            var addrfmt = "0x{0:x" + ndigits + "}: ";
            var chars = new StringBuilder();

            StartDetailUpdate();

            for (var i = 0; i < size; i++)
            {
                if (i % 16 == 0)
                {
                    if (i != 0)
                    {
                        _detailBuffer.Append("  ");
                        _detailBuffer.Append(chars);
                        _detailBuffer.Append("\r\n");
                        chars.Clear();
                    }
                    _detailBuffer.Append(string.Format(addrfmt, i));
                }
                else if (i % 8 == 0)
                {
                    _detailBuffer.Append(" ");
                    chars.Append(" ");
                }

                var value = reader.ReadByte();
                _detailBuffer.Append($"{value:x2} ");

                if (value >= 0x21 && value <= 0x7f)
                    chars.Append(Convert.ToChar(value));
                else
                    chars.Append(".");
            }
            _detailBuffer.Append("  ");
            _detailBuffer.Append(chars);
            _detailBuffer.Append("\r\n");

            EndDetailUpdate();
        }

        private void RenderHexView(BinaryReader reader, int size)
        {
            var ndigits = $"{size:x}".Length;
            var addrfmt = "0x{0:x" + ndigits + "}: ";

            StartDetailUpdate();

            for (var i = 0; i < size; i += 4)
            {
                if (i % 32 == 0)
                {
                    if (i != 0)
                    {
                        _detailBuffer.Append("  ");
                        _detailBuffer.Append("\r\n");
                    }

                    _detailBuffer.Append(string.Format(addrfmt, i));
                }
                else if (i % 16 == 0)
                {
                    _detailBuffer.Append(" ");
                }

                var value = reader.ReadInt32();
                _detailBuffer.Append($"{value:x8} ");
            }

            EndDetailUpdate();
        }

        private void RenderStringAnalysis(MemoryStream stream, BinaryReader reader, int size)
        {
            StartDetailUpdate();

            var current = new StringBuilder();

            for (var i = 0; i < size; i++)
            {
                var b = reader.ReadByte();

                if (b == 0 && current.Length > 0)
                {
                    AddDetailLine("0x{0:x6}: {1}", i, current.ToString());
                    current.Clear();
                }

                if (b < 0x20 || b > 0x7f)
                {
                    current.Clear();
                    continue;
                }

                current.Append(Convert.ToChar(b));
            }

            EndDetailUpdate();
        }

        private void RenderCodeView(SmxCodeV1Section code, string name, int address)
        {
            StartDetailUpdate();

            V1Instruction[] insns;

            try
            {
                insns = V1Disassembler.Disassemble(_file, code, address);
            }
            catch (Exception e)
            {
                AddDetailLine(Program.Translations.NotDissMethod, name, e.Message);
                EndDetailUpdate();
                return;
            }

            AddDetailLine("; {0}", name);
            AddDetailLine("; {0} instruction(s)", insns.Length);
            AddDetailLine("; starts at code address 0x{0:x}", address);
            AddDetailLine("---");

            if (insns.Length == 0)
            {
                EndDetailUpdate();
                return;
            }

            // Find the largest address so we can get consistent column length.
            var lastAddress = insns[insns.Length - 1].Address;
            var ndigits = $"{lastAddress:x}".Length;
            var addrfmt = "0x{0:x" + ndigits + "}: ";
            var buffer = new StringBuilder();
            var comment = new StringBuilder();

            foreach (var insn in insns)
            {
                buffer.Clear();
                comment.Clear();
                buffer.Append(insn.Info.Name);

                for (var i = 0; i < insn.Params.Length; i++)
                {
                    if (i >= insn.Info.Params.Length)
                        break;
                    var kind = insn.Info.Params[i];
                    var value = insn.Params[i];

                    switch (kind)
                    {
                        case V1Param.Constant:
                        case V1Param.CaseTable:
                            buffer.Append($" 0x{value:x}");
                            comment.Append($" {value}");
                            break;
                        case V1Param.Native:
                            buffer.Append($" {value}");
                            if (_file.Natives != null && value < _file.Natives.Length)
                                comment.Append($" {_file.Natives[value].Name}");
                            break;
                        case V1Param.Jump:
                            var delta = value - insn.Address;
                            buffer.Append($" 0x{value:x}");
                            comment.Append(delta >= 0 ? $" +0x{delta:x}" : $" -0x{-delta:x}");
                            break;
                        case V1Param.Address:
                        {
                            DebugSymbolEntry sym = null;
                            if (_file.DebugSymbols != null)
                                sym = _file.DebugSymbols.FindDataRef(value);
                            buffer.Append($" 0x{value:x}");
                            comment.Append(sym != null ? $" {sym.Name}" : $" {value}");
                            break;
                        }
                        case V1Param.Stack:
                        {
                            DebugSymbolEntry sym = null;
                            if (_file.DebugSymbols != null)
                                sym = _file.DebugSymbols.FindStackRef(insn.Address, value);
                            buffer.Append($" 0x{value:x}");
                            comment.Append(sym != null ? $" {sym.Name}" : $" {value}");
                            break;
                        }
                        case V1Param.Function:
                            var fun = _file.FindFunctionName(value);
                            buffer.Append($" 0x{value:x}");
                            comment.Append($" {fun}");
                            break;
                        default:
                            // ignored
                            break;
                    }
                }

                _detailBuffer.Append(string.Format(addrfmt, insn.Address));
                _detailBuffer.Append($"{buffer,-32}");

                if (comment.Length > 0)
                    _detailBuffer.Append($" ;{comment}");

                _detailBuffer.Append("\r\n");
            }

            EndDetailUpdate();
        }

        private void RenderCodeSection(ItemsControl root, SmxCodeV1Section code)
        {
            root.Tag = new NodeData(delegate
            {
                RenderSectionHeaderDetail(code.SectionHeader);
                AddDetailLine("codesize = {0} bytes", code.Header.CodeSize);
                AddDetailLine("cellsize = {0} bytes", code.Header.CellSize);
                AddDetailLine("codeversion = 0x{0:x}", code.Header.CodeVersion);
                AddDetailLine("flags = 0x{0:x} ; {0}", code.Header.Flags, code.Header.Flags.ToString());
                AddDetailLine("main = 0x{0:x}", code.Header.main);
                AddDetailLine("codeoffs = 0x{0:x}", code.Header.codeoffs);
                EndDetailUpdate();
            }, code);

            var node = new TreeViewItem() { Header = "cell view" };

            root.Items.Add(node);
            node.Tag = new NodeData(delegate
            {
                RenderHexView(code.Reader(), code.Header.CodeSize);
            }, null);

            var functionMap = new Dictionary<string, uint>();

            if (_file.Publics != null)
                foreach (var pubfun in _file.Publics.Entries)
                    functionMap[pubfun.Name] = pubfun.Address;

            if (_file.DebugSymbols != null)
                foreach (var sym in _file.DebugSymbols.Entries)
                {
                    if (sym.Ident != SymKind.Function)
                        continue;

                    functionMap[sym.Name] = sym.CodeStart;
                }

            foreach (var pair in functionMap)
            {
                var name = pair.Key;
                var address = functionMap[pair.Key];
                var snode = new TreeViewItem() { Header = pair.Key };
                root.Items.Add(snode);
                snode.Tag = new NodeData(delegate()
                {
                    RenderCodeView(code, name, (int)address);
                }, null);
            }
        }

        private void RenderDataList(ItemsControl root, SmxDataSection data)
        {
            root.Tag = new NodeData(delegate
            {
                RenderSectionHeaderDetail(data.SectionHeader);
                AddDetailLine("datasize = {0} bytes", data.Header.DataSize);
                AddDetailLine("memory = {0} bytes", data.Header.MemorySize);
                AddDetailLine("dataoffs = 0x{0:x}", data.Header.dataoffs);
                EndDetailUpdate();
            }, data);

            var node = new TreeViewItem { Header = "byte view" };

            root.Items.Add(node);
            node.Tag = new NodeData(delegate
            {
                RenderByteView(data.Reader(), (int)data.Header.DataSize);
            }, null);

            node = new TreeViewItem() { Header = "cell view" };
            root.Items.Add(node);
            node.Tag = new NodeData(delegate()
            {
                RenderHexView(data.Reader(), (int)data.Header.DataSize);
            }, null);

            node = new TreeViewItem { Header = "string analysis" };
            root.Items.Add(node);
            node.Tag = new NodeData(delegate
            {
                RenderStringAnalysis(data.Memory(), data.Reader(), (int)data.Header.DataSize);
            }, null);
        }

        private void RenderPublicsList(ItemsControl root, SmxPublicTable publics)
        {
            for (var i = 0; i < publics.Length; i++)
            {
                var index = i;
                var pubfun = publics[i];
                var node = new TreeViewItem { Header = i + ": " + pubfun.Name };

                root.Items.Add(node);
                node.Tag = new NodeData(delegate
                {
                    StartDetail("; public entry {0}", index);
                    AddDetailLine("nameoffs = 0x{0:x} ; {1}", pubfun.nameoffs, pubfun.Name);
                    AddDetailLine("address = 0x{0:x}", pubfun.Address);
                    EndDetailUpdate();
                }, null);
            }
        }

        private void RenderPubvarList(ItemsControl root, SmxPubvarTable pubvars)
        {
            for (var i = 0; i < pubvars.Length; i++)
            {
                var index = i;
                var pubvar = pubvars[i];
                var node = new TreeViewItem { Header = i + ": " + pubvar.Name };

                root.Items.Add(node);
                node.Tag = new NodeData(delegate
                {
                    StartDetail("; pubvar entry {0}", index);
                    AddDetailLine("nameoffs = 0x{0:x} ; {1}", pubvar.nameoffs, pubvar.Name);
                    AddDetailLine("address = 0x{0:x}", pubvar.Address);
                    EndDetailUpdate();
                }, null);
            }
        }

        private void RenderTagList(ItemsControl root, SmxTagTable tags)
        {
            for (var i = 0; i < tags.Length; i++)
            {
                var tag = tags[i];
                var text = tag.Id + ": " + tag.Name;

                if ((tag.Flags & ~(TagFlags.Fixed)) != 0)
                    text += " (" + (tag.Flags & ~(TagFlags.Fixed)) + ")";

                var node = new TreeViewItem() { Header = text };
                root.Items.Add(node);
                node.Tag = new NodeData(delegate
                {
                    StartDetail("tag: 0x{0:x} ; flags = {1}", tag.Value, tag.Flags.ToString());
                    AddDetailLine("nameoffs: 0x{0:x} ; {1}", tag.entry.nameoffs, tag.Name);
                    AddDetailLine("id: 0x{0:x}", tag.Id);
                    EndDetailUpdate();
                }, null);
            }
        }

        private void RenderDebugLines(FrameworkElement root, SmxDebugLinesTable lines)
        {
            root.Tag = new NodeData(delegate
            {
                RenderSectionHeaderDetail(lines.SectionHeader);

                foreach (var line in lines.Entries)
                    AddDetailLine("line {0} @ address 0x{1:x}", line.Line, line.Address);

                EndDetailUpdate();
            }, null);
        }

        private void RenderNativeList(ItemsControl root, SmxNativeTable natives)
        {
            for (var i = 0; i < natives.Length; i++)
            {
                var index = i;
                var native = natives[i];
                var node = new TreeViewItem { Header = ("[" + i + "] " + native.Name) };

                root.Items.Add(node);
                node.Tag = new NodeData(delegate
                {
                    StartDetail("index = {0}", index);
                    AddDetailLine("nameoffs: 0x{0:x} ; {1}", native.nameoffs, native.Name);
                    EndDetailUpdate();
                }, null);
            }
        }

        private void RenderNamesList(FrameworkElement root, SmxNameTable names)
        {
            root.Tag = new NodeData(delegate
            {
                RenderSectionHeaderDetail(names.SectionHeader);

                foreach (var offset in names.Extents)
                    AddDetailLine("0x{0:x}: {1}", offset, names.StringAt(offset));

                EndDetailUpdate();
            }, null);
        }

        private void RenderDebugFiles(FrameworkElement root, SmxDebugFilesTable files)
        {
            root.Tag = new NodeData(delegate
            {
                RenderSectionHeaderDetail(files.SectionHeader);
                AddDetailLine("--");

                foreach (var file in files.Entries)
                {
                    AddDetailLine("\"{0}\"", file.Name);
                    AddDetailLine(" nameoffs = 0x{0:x}", file.nameoffs);
                    AddDetailLine(" address = 0x{0:x}", file.Address);
                }

                EndDetailUpdate();
            }, null);
        }

        private void RenderDebugInfo(FrameworkElement root, SmxDebugInfoSection info)
        {
            root.Tag = new NodeData(delegate
            {
                RenderSectionHeaderDetail(info.SectionHeader);
                AddDetailLine("num_files = {0}", info.NumFiles);
                AddDetailLine("num_lines = {0}", info.NumLines);
                AddDetailLine("num_symbols = {0}", info.NumSymbols);
                AddDetailLine("num_arrays = {0}", info.NumArrays);
                EndDetailUpdate();
            }, null);
        }

        private static string DimsToString(Tag tag, IReadOnlyList<DebugSymbolDimEntry> dims)
        {
            var str = "";

            for (var i = 0; i < dims.Count; i++)
            {
                int size;
                if (i == dims.Count - 1 && tag != null && tag.Name == "String")
                    size = dims[i].Size * 4;
                else
                    size = dims[i].Size;

                if (size == 0)
                    str += "[]";
                else
                    str += $"[{size}]";
            }

            return str;
        }

        private void RenderSymbolDetail(DebugSymbolEntry entry)
        {
            Tag tag = null;

            if (_file.Tags != null)
                tag = _file.Tags.FindTag(entry.TagId);

            StartDetail("; {0}", entry.Name);

            if (entry.Address < 0)
                AddDetailLine("address = -0x{0:x}", -entry.Address);
            else
                AddDetailLine("address = 0x{0:x}", entry.Address);

            if (tag == null)
                AddDetailLine("tagid = 0x{0:x}", entry.TagId);
            else
                AddDetailLine("tagid = 0x{0:x} ; {1}", entry.TagId, tag.Name);

            AddDetailLine("codestart = 0x{0:x}", entry.CodeStart);
            AddDetailLine("codeend = 0x{0:x}", entry.CodeEnd);
            AddDetailLine("nameoffs = 0x{0:x} ; {1}", entry.nameoffs, entry.Name);
            AddDetailLine("kind = {0:d} ; {1}", entry.Ident, entry.Ident.ToString());
            AddDetailLine("scope = {0:d} ; {1}", entry.Scope, entry.Scope.ToString());

            if (entry.Dims != null)
                AddDetailLine("dims = {0}", DimsToString(tag, entry.Dims));

            string file = null;

            if (_file.DebugFiles != null)
                file = _file.DebugFiles.FindFile(entry.CodeStart);

            if (file != null)
                AddDetailLine("file: \"{0}\"", file);

            uint? line = null;

            if (_file.DebugLines != null)
                line = _file.DebugLines.FindLine(entry.CodeStart);

            if (line != null)
                AddDetailLine("line: \"{0}\"", (uint)line);

            EndDetailUpdate();
        }

        private void RenderDebugFunction(SmxDebugSymbolsTable syms, ItemsControl root, DebugSymbolEntry fun)
        {
            root.Tag = new NodeData(delegate
            {
                RenderSymbolDetail(fun);
            }, null);

            var args = new List<DebugSymbolEntry>();
            var locals = new List<DebugSymbolEntry>();

            foreach (var symbol in syms.Entries)
            {
                var sym = symbol;

                if (sym.Scope == SymScope.Global)
                    continue;

                if (sym.CodeStart < fun.CodeStart || sym.CodeEnd > fun.CodeEnd)
                    continue;

                if (sym.Address < 0)
                    locals.Add(sym);
                else
                    args.Add(sym);
            }

            args.Sort((e1, e2) => e1.Address.CompareTo(e2.Address));

            foreach (var symbol in args)
            {
                var sym = symbol;
                var node = new TreeViewItem { Header = sym.Name };
                root.Items.Add(node);
                node.Tag = new NodeData(delegate
                {
                    RenderSymbolDetail(sym);
                }, null);
            }

            locals.Sort((e1, e2) => e1.CodeStart.CompareTo(e2.CodeStart));

            foreach (var symbol in locals)
            {
                var sym = symbol;
                var node = new TreeViewItem() { Header = sym.Name };
                root.Items.Add(node);
                node.Tag = new NodeData(delegate
                {
                    RenderSymbolDetail(sym);
                }, null);
            }
        }

        private void RenderDebugSymbols(ItemsControl root, SmxDebugSymbolsTable syms)
        {
            root.Items.Add("globals");

            foreach (var symbol in syms.Entries)
            {
                var sym = symbol;

                if (sym.Scope != SymScope.Global)
                    continue;

                if (sym.Ident == SymKind.Function)
                    continue;

                var node = new TreeViewItem() { Header = sym.Name };
                root.Items.Add(node);
                node.Tag = new NodeData(delegate
                {
                    RenderSymbolDetail(sym);
                }, null);
            }

            root.Items.Add("functions");

            foreach (var symbol in syms.Entries)
            {
                var sym = symbol;
                if (sym.Scope != SymScope.Global)
                    continue;
                if (sym.Ident != SymKind.Function)
                    continue;
                var node = new TreeViewItem { Header = sym.Name };
                root.Items.Add(node);
                RenderDebugFunction(syms, node, sym);
            }
        }

        private void RenderDebugNative(DebugNativeEntry entry)
        {
            Tag tag = null;

            if (_file.Tags != null)
                tag = _file.Tags.FindTag(entry.tagid);

            StartDetailUpdate();

            AddDetailLine("nameoffs = 0x{0:x}", entry.nameoffs, entry.Name);

            if (tag == null)
                AddDetailLine("tagid = 0x{0:x}", entry.tagid);
            else
                AddDetailLine("tagid = 0x{0:x} ; {1}", entry.tagid, entry.Name);

            AddDetailLine("index = {0}", entry.Index);

            for (var i = 0; i < entry.Args.Length; i++)
            {
                var arg = entry.Args[i];
                AddDetailLine("arg {0}", i);
                AddDetailLine("  nameoffs = 0x{0:x} ; {1}", arg.nameoffs, arg.Name);
                AddDetailLine("  kind = {0:d} ; {1}", arg.Ident, arg.Ident.ToString());

                if (arg.Dims != null)
                    AddDetailLine("  dims = {0}", DimsToString(tag, arg.Dims));
            }
            EndDetailUpdate();
        }

        private void RenderDebugNatives(ItemsControl root, SmxDebugNativesTable natives)
        {
            foreach (var native in natives.Entries)
            {
                var nat = native;
                var node = new TreeViewItem {Header = nat.Name};
                root.Items.Add(node);
                node.Tag = new NodeData(delegate { RenderDebugNative(nat); }, null);
            }
        }

        private void treeview__SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var node = treeview_.SelectedItem;
            var data = (NodeData) (node as TreeViewItem)?.Tag;
            data?.Callback?.Invoke();
        }

        public class NodeData
        {
            public DrawNodeFn Callback;
            public object Data;

            public NodeData(DrawNodeFn aCallback, object aData)
            {
                Callback = aCallback;
                Data = aData;
            }
        }
    }
}
