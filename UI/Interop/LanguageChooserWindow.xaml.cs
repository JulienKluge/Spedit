using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Spedit.UI.Interop
{
	/// <summary>
	/// Interaction logic for LanguageChooserWindow.xaml
	/// </summary>
	public partial class LanguageChooserWindow
	{
		public string SelectedId = string.Empty;

		public LanguageChooserWindow()
		{
			InitializeComponent();
		}

		public LanguageChooserWindow(IReadOnlyList<string> ids, IReadOnlyList<string> languages)
		{
			InitializeComponent();

			for (var i = 0; i < ids.Count; ++i)
				LanguageBox.Items.Add(new ComboBoxItem() { Content = languages[i], Tag = ids[i] });

			if (ids.Count > 0)
				LanguageBox.SelectedIndex = 0;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var selectedObj = LanguageBox.SelectedItem;

			if (selectedObj == null)
				return;

		    var item = selectedObj as ComboBoxItem;

		    if (item != null)
			{
				var selectedItem = item;
				SelectedId = (string)selectedItem.Tag;
			}

			Close();
		}
	}
}
