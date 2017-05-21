using MahApps.Metro.Controls;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Navigation;
using MahApps.Metro;

namespace Spedit.UI.Interop
{
	/// <summary>
	/// Interaction logic for LanguageChooserWindow.xaml
	/// </summary>
	public partial class LanguageChooserWindow : MetroWindow
	{
		public string SelectedID = string.Empty;
		public LanguageChooserWindow()
		{
			InitializeComponent();
		}

		public LanguageChooserWindow(string[] ids, string[] languages)
		{
			InitializeComponent();
			for (int i = 0; i < ids.Length; ++i)
			{
				LanguageBox.Items.Add(new ComboBoxItem() { Content = languages[i], Tag = ids[i] });
			}
			if (ids.Length > 0)
			{
				LanguageBox.SelectedIndex = 0;
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			object selectedObj = LanguageBox.SelectedItem;
			if (selectedObj == null)
			{
				return;
			}
			if (selectedObj is ComboBoxItem)
			{
				ComboBoxItem selectedItem = (ComboBoxItem)selectedObj;
				SelectedID = (string)selectedItem.Tag;
			}
			Close();
		}
	}
}
