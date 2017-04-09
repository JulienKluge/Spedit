using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.Text;
using System.IO;
using SourcepawnCondenser.Tokenizer;
using SourcepawnCondenser.SourcemodDefinition;

namespace CondenserTest
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			StringBuilder str = new StringBuilder();
			List<string> files = new List<string>();
			files.AddRange(Directory.GetFiles(@"C:\Users\Jelle\Desktop\coding\sm-scripting\_Sourcemod Plugins\1.7_5255", "*.inc", SearchOption.AllDirectories));
			str.AppendLine(files.Count.ToString());
			foreach (var f in files)
			{
				str.AppendLine(File.ReadAllText(f));
			}
            ExpandBox.IsChecked = false;
            //str.AppendLine(File.ReadAllText(@"C:\Users\Jelle\Desktop\coding\sm-scripting\CondenserTestFile.inc"));
            textBox.Text = str.ToString();
		}

		private void textBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			string text = textBox.Text;
			Stopwatch watch = new Stopwatch();
			watch.Start();
			List<Token> tList = Tokenizer.TokenizeString(text, false);
			watch.Stop();
			Token[] t = tList.ToArray();
			double tokenToTextLength = (double)t.Length / (double)text.Length;
			string subTitle = watch.ElapsedMilliseconds.ToString() + " ms  -  tokenL/textL: " + tokenToTextLength.ToString() + "  (" + t.Length.ToString() + " / " + text.Length.ToString() + ")";
			tokenStack.Children.Clear();
			int i = 0;
			if (t.Length < 10000)
			{
				foreach (var token in t)
				{
					++i;
					Grid g = new Grid() { Background = ChooseBackgroundFromTokenKind(token.Kind) };
					g.Tag = token;
					g.MouseLeftButtonUp += G_MouseLeftButtonUp;
					g.HorizontalAlignment = HorizontalAlignment.Stretch;
					g.Children.Add(new TextBlock() { Text = token.Kind.ToString() + " - '" + token.Value + "'", IsHitTestVisible = false });
					tokenStack.Children.Add(g);
				}
			}
			termTree.Items.Clear();
			watch.Reset();
			watch.Start();
			SourcepawnCondenser.Condenser c = new SourcepawnCondenser.Condenser(text, "");
			var def = c.Condense();
			watch.Stop();
			subTitle += "  -  condenser: " + watch.ElapsedMilliseconds.ToString() + " ms";
			this.Title = subTitle;
			bool expand = ExpandBox.IsChecked.Value;
			TreeViewItem functionItem = new TreeViewItem() { Header = "functions (" + def.Functions.Count.ToString() + ")", IsExpanded = expand };
			foreach (var f in def.Functions)
			{
				TreeViewItem item = new TreeViewItem() { Header = f.Name, IsExpanded = expand };
				item.Tag = f;
				item.MouseLeftButtonUp += ItemFunc_MouseLeftButtonUp;
				item.Items.Add(new TreeViewItem() { Header = "Index: " + f.Index.ToString(), Background = Brushes.LightGray });
				item.Items.Add(new TreeViewItem() { Header = "Length: " + f.Length.ToString() });
				item.Items.Add(new TreeViewItem() { Header = "Kind: " + f.FunctionKind.ToString(), Background = Brushes.LightGray });
				item.Items.Add(new TreeViewItem() { Header = "ReturnType: " + f.ReturnType });
				item.Items.Add(new TreeViewItem() { Header = "Comment: >>" + f.CommentString + "<<", Background = Brushes.LightGray });
				for (int j = 0; j < f.Parameters.Length; ++j)
				{
					item.Items.Add(new TreeViewItem() { Header = "Parameter " + (j + 1).ToString() + ": " + f.Parameters[j], Background = ((j + 1) % 2 == 0) ? Brushes.LightGray : Brushes.White });
				}
				functionItem.Items.Add(item);
			}
			termTree.Items.Add(functionItem);
			TreeViewItem enumItem = new TreeViewItem() { Header = "enums (" + def.Enums.Count.ToString() + ")", IsExpanded = expand };
			foreach (var en in def.Enums)
			{
				TreeViewItem item = new TreeViewItem() { Header = (string.IsNullOrWhiteSpace(en.Name)) ? "no name" : en.Name, IsExpanded = expand };
				item.Tag = en;
				item.MouseLeftButtonUp += ItemEnum_MouseLeftButtonUp;
				item.Items.Add(new TreeViewItem() { Header = "Index: " + en.Index.ToString(), Background = Brushes.LightGray });
				item.Items.Add(new TreeViewItem() { Header = "Length: " + en.Length.ToString() });
				for (int j = 0; j < en.Entries.Length; ++j)
				{
					item.Items.Add(new TreeViewItem() { Header = "Entry " + (j + 1).ToString() + ": " + en.Entries[j], Background = (j % 2 == 0) ? Brushes.LightGray : Brushes.White });
				}
				enumItem.Items.Add(item);
			}
			termTree.Items.Add(enumItem);
			TreeViewItem structItem = new TreeViewItem() { Header = "structs (" + def.Structs.Count.ToString() + ")", IsExpanded = expand };
			foreach (var s in def.Structs)
			{
				TreeViewItem item = new TreeViewItem() { Header = (string.IsNullOrWhiteSpace(s.Name)) ? "no name" : s.Name, IsExpanded = expand };
				item.Tag = s;
				item.MouseLeftButtonUp += ItemStruct_MouseLeftButtonUp;
				item.Items.Add(new TreeViewItem() { Header = "Index: " + s.Index.ToString(), Background = Brushes.LightGray });
				item.Items.Add(new TreeViewItem() { Header = "Length: " + s.Length.ToString() });
				structItem.Items.Add(item);
			}
			termTree.Items.Add(structItem);
			TreeViewItem dItem = new TreeViewItem() { Header = "defines (" + def.Defines.Count.ToString() + ")", IsExpanded = expand };
			foreach (var d in def.Defines)
			{
				TreeViewItem item = new TreeViewItem() { Header = d.Name, IsExpanded = expand };
				item.Tag = d;
				item.MouseLeftButtonUp += Itemppd_MouseLeftButtonUp;
				item.Items.Add(new TreeViewItem() { Header = "Index: " + d.Index.ToString(), Background = Brushes.LightGray });
				item.Items.Add(new TreeViewItem() { Header = "Length: " + d.Length.ToString() });
				dItem.Items.Add(item);
			}
			termTree.Items.Add(dItem);
            TreeViewItem cItem = new TreeViewItem() { Header = "constants (" + def.Constants.Count.ToString() + ")", IsExpanded = expand };
            foreach (var cn in def.Constants)
            {
                TreeViewItem item = new TreeViewItem() { Header = cn.Name, IsExpanded = expand };
                item.Tag = cn;
                item.MouseLeftButtonUp += Itemc_MouseLeftButtonUp;
                item.Items.Add(new TreeViewItem() { Header = "Index: " + cn.Index.ToString(), Background = Brushes.LightGray });
                item.Items.Add(new TreeViewItem() { Header = "Length: " + cn.Length.ToString() });
                cItem.Items.Add(item);
            }
            termTree.Items.Add(cItem);
            TreeViewItem mItem = new TreeViewItem() { Header = "methodmaps (" + def.Methodmaps.Count.ToString() + ")", IsExpanded = expand };
            foreach (var m in def.Methodmaps)
            {
                TreeViewItem item = new TreeViewItem() { Header = m.Name, IsExpanded = expand };
                item.Tag = m;
                item.MouseLeftButtonUp += ItemMM_MouseLeftButtonUp;
                item.Items.Add(new TreeViewItem() { Header = "Index: " + m.Index.ToString(), Background = Brushes.LightGray });
                item.Items.Add(new TreeViewItem() { Header = "Length: " + m.Length.ToString() });
                item.Items.Add(new TreeViewItem() { Header = "Type: " + m.Type, Background = Brushes.LightGray });
                item.Items.Add(new TreeViewItem() { Header = "InheritedType: " + m.InheritedType });
                TreeViewItem subItem = new TreeViewItem() { Header = "Methods", Background = Brushes.LightGray };
                for (int j = 0; j < m.Methods.Count; ++j)
                {
                    TreeViewItem subSubItem = new TreeViewItem() { Header = m.Methods[j].Name, Background = (j % 2 == 0) ? Brushes.LightGray : Brushes.White };
                    subSubItem.Items.Add(new TreeViewItem() { Header = "Index: " + m.Methods[j].Index.ToString() });
                    subSubItem.Items.Add(new TreeViewItem() { Header = "Length: " + m.Methods[j].Length.ToString(), Background = Brushes.LightGray });
                    subSubItem.Items.Add(new TreeViewItem() { Header = "Comment: >>" + m.Methods[j].CommentString + "<<" });
                    subSubItem.Items.Add(new TreeViewItem() { Header = "Return: " + m.Methods[j].ReturnType, Background = Brushes.LightGray });
                    int k = 0;
                    for (; k < m.Methods[j].MethodKind.Length; ++k)
                    {
                        subSubItem.Items.Add(new TreeViewItem() { Header = "MethodKind" + (k + 1).ToString() + ": " + m.Methods[j].MethodKind[k], Background = (k % 2 == 0) ? Brushes.LightGray : Brushes.White });
                    }
                    for (int l = 0; l < m.Methods[j].Parameters.Length; ++l)
                    {
                        ++k;
                        subSubItem.Items.Add(new TreeViewItem() { Header = "Parameter" + (l + 1).ToString() + ": " + m.Methods[j].Parameters[l], Background = (k % 2 == 0) ? Brushes.LightGray : Brushes.White });
                    }
                    subItem.Items.Add(subSubItem);
                }
                item.Items.Add(subItem);
                subItem = new TreeViewItem() { Header = "Fields" };
                for (int j = 0; j < m.Fields.Count; ++j)
                {
                    TreeViewItem subSubItem = new TreeViewItem() { Header = m.Fields[j].Name, Background = (j % 2 == 0) ? Brushes.LightGray : Brushes.White };
                    subSubItem.Items.Add(new TreeViewItem() { Header = "Index: " + m.Fields[j].Index.ToString() });
                    subSubItem.Items.Add(new TreeViewItem() { Header = "Length: " + m.Fields[j].Length.ToString(), Background = Brushes.LightGray });
                    //subSubItem.Items.Add(new TreeViewItem() { Header = "Type: " + m.Fields[j].Type });
                    subItem.Items.Add(subSubItem);
                }
                item.Items.Add(subItem);
                mItem.Items.Add(item);
            }
            termTree.Items.Add(mItem);
        }

		private void ItemFunc_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var token = ((TreeViewItem)sender).Tag;
			if (token != null)
			{
				if (token is SMFunction)
				{
					textBox.Focus();
					textBox.Select(((SMFunction)token).Index, ((SMFunction)token).Length);
				}
			}
		}

		private void ItemEnum_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var token = ((TreeViewItem)sender).Tag;
			if (token != null)
			{
				if (token is SMEnum)
				{
					textBox.Focus();
					textBox.Select(((SMEnum)token).Index, ((SMEnum)token).Length);
				}
			}
		}

		private void ItemStruct_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var token = ((TreeViewItem)sender).Tag;
			if (token != null)
			{
				if (token is SMStruct)
				{
					textBox.Focus();
					textBox.Select(((SMStruct)token).Index, ((SMStruct)token).Length);
				}
			}
		}

		private void Itemppd_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var token = ((TreeViewItem)sender).Tag;
			if (token != null)
			{
				if (token is SMDefine)
				{
					textBox.Focus();
					textBox.Select(((SMDefine)token).Index, ((SMDefine)token).Length);
				}
			}
		}

        private void Itemc_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var token = ((TreeViewItem)sender).Tag;
            if (token != null)
            {
                if (token is SMConstant)
                {
                    textBox.Focus();
                    textBox.Select(((SMConstant)token).Index, ((SMConstant)token).Length);
                }
            }
        }

        private void ItemMM_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var token = ((TreeViewItem)sender).Tag;
            if (token != null)
            {
                if (token is SMMethodmap)
                {
                    textBox.Focus();
                    textBox.Select(((SMMethodmap)token).Index, ((SMMethodmap)token).Length);
                }
            }
        }

        private void G_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var token = ((Grid)sender).Tag;
			if (token != null)
			{
				if (token is Token)
				{
					textBox.Focus();
					textBox.Select(((Token)token).Index, ((Token)token).Length);
				}
			}
		}

		private Brush ChooseBackgroundFromTokenKind(TokenKind kind)
		{
			switch (kind)
			{
				case TokenKind.BraceClose:
				case TokenKind.BraceOpen: return Brushes.LightGray;
				case TokenKind.Character: return Brushes.LightSalmon;
				case TokenKind.Eof: return Brushes.LimeGreen;
				case TokenKind.Identifier: return Brushes.LightSteelBlue;
				case TokenKind.Number: return Brushes.LightSeaGreen;
				case TokenKind.ParenthesisClose:
				case TokenKind.ParenthesisOpen: return Brushes.LightSlateGray;
				case TokenKind.Quote: return Brushes.LightGoldenrodYellow;
				case TokenKind.Eol: return Brushes.Aqua;
				case TokenKind.SingleLineComment:
				case TokenKind.MultiLineComment: return Brushes.Honeydew;
				default: return Brushes.IndianRed;
			}
		}

        private void CaretPositionChangedEvent(object sender, RoutedEventArgs e)
        {
            CaretLabel.Content = textBox.CaretIndex.ToString() + " / " + textBox.SelectionLength.ToString();
        }
	}
}
