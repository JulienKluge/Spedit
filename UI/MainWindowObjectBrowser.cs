using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Spedit.UI
{
	public partial class MainWindow
	{
		private string _currentObjectBrowserDirectory = string.Empty;

		private void TreeViewOBItem_Expanded(object sender, RoutedEventArgs e)
		{
			var source = e.Source;

		    if (!(source is TreeViewItem))
                return;

		    var item = (TreeViewItem)source;
            var itemInfo = (ObjectBrowserTag)item.Tag;

		    if (itemInfo.Kind != ObjectBrowserItemKind.Directory)
                return;

		    if (!Directory.Exists(itemInfo.Value))
		        return;

		    using (Dispatcher.DisableProcessing())
		    {
		        item.Items.Clear();
		        var newItems = BuildDirectoryItems(itemInfo.Value);

		        foreach (var i in newItems)
		        {
		            item.Items.Add(i);
		        }
		    }
		}

		private void TreeViewOBItemParentDir_DoubleClicked(object sender, RoutedEventArgs e)
		{
			var currentInfo = new DirectoryInfo(_currentObjectBrowserDirectory);
            var parentInfo = currentInfo.Parent;

			if (parentInfo != null)
			{
				if (parentInfo.Exists)
				{
					ChangeObjectBrowserToDirectory(parentInfo.FullName);
					return;
				}
			}

			ChangeObjectBrowserToDrives();
		}

		private void TreeViewOBItemFile_DoubleClicked(object sender, RoutedEventArgs e)
		{
		    if (!(sender is TreeViewItem))
                return;

		    var item = (TreeViewItem)sender;
            var itemInfo = (ObjectBrowserTag)item.Tag;

		    if (itemInfo.Kind == ObjectBrowserItemKind.File)
		        TryLoadSourceFile(itemInfo.Value, true, false, true);
		}

		private void ListViewOBItem_SelectFile(object sender, RoutedEventArgs e)
		{
		    if (!(sender is ListViewItem))
                return;

		    var element = GetCurrentEditorElement();

		    if (element != null)
		    {
		        var fInfo = new FileInfo(element.FullFilePath);
		        ChangeObjectBrowserToDirectory(fInfo.DirectoryName);
		    }

		    ((ListViewItem)sender).IsSelected = false;
		    ObjectBrowserButtonHolder.SelectedIndex = -1;
		}

		private void ListViewOBItem_SelectConfig(object sender, RoutedEventArgs e)
		{
		    if (!(sender is ListViewItem))
                return;

		    var cc = Program.Configs[Program.SelectedConfig];

		    if (cc.SMDirectories.Length > 0)
		        ChangeObjectBrowserToDirectory(cc.SMDirectories[0]);

		    ((ListViewItem)sender).IsSelected = false;
		    ObjectBrowserButtonHolder.SelectedIndex = -1;
		}

		private void ListViewOBItem_SelectOBItem(object sender, RoutedEventArgs e)
		{
		    if (!(sender is ListViewItem))
                return;

		    var objectBrowserSelectedItem = ObjectBrowser.SelectedItem;
		    var viewItem = objectBrowserSelectedItem as TreeViewItem;

		    if (viewItem != null)
		    {
		        var item = viewItem;
                var itemInfo = (ObjectBrowserTag)item.Tag;

		        switch (itemInfo.Kind)
		        {
		            case ObjectBrowserItemKind.Directory:
		                ChangeObjectBrowserToDirectory(itemInfo.Value);
		                break;
		            case ObjectBrowserItemKind.ParentDirectory:
                        var currentInfo = new DirectoryInfo(_currentObjectBrowserDirectory);
		                var parentInfo = currentInfo.Parent;
		                if (parentInfo != null)
		                {
		                    if (parentInfo.Exists)
		                    {
		                        ChangeObjectBrowserToDirectory(parentInfo.FullName);
		                        return;
		                    }
		                }
		                ChangeObjectBrowserToDrives();
		                break;
		            case ObjectBrowserItemKind.File:
                        // ignored
                        break;
		            default:
                        // ignored
		                break;
		        }
		    }

		    ((ListViewItem)sender).IsSelected = false;
		    ObjectBrowserButtonHolder.SelectedIndex = -1;
		}

		private void ChangeObjectBrowserToDirectory(string dir)
		{
			if (string.IsNullOrWhiteSpace(dir))
			{
				var cc = Program.Configs[Program.SelectedConfig];

				if (cc.SMDirectories.Length > 0)
					dir = cc.SMDirectories[0];
			}
			else if (dir == "0:")
			{
				ChangeObjectBrowserToDrives();
				return;
			}
			if (!Directory.Exists(dir))
				dir = Environment.CurrentDirectory;

			_currentObjectBrowserDirectory = dir;
			Program.OptionsObject.ProgramObjectBrowserDirectory = _currentObjectBrowserDirectory;

			using (Dispatcher.DisableProcessing())
			{
			    ObjectBrowserDirBlock.Text = dir;
			    ObjectBrowser.Items.Clear();

			    var parentDirItem = new TreeViewItem() {
			        Header = "..",
			        Tag = new ObjectBrowserTag() { Kind = ObjectBrowserItemKind.ParentDirectory }
			    };

			    parentDirItem.MouseDoubleClick += TreeViewOBItemParentDir_DoubleClicked;
			    ObjectBrowser.Items.Add(parentDirItem);
			    var newItems = BuildDirectoryItems(dir);

			    foreach (var item in newItems)
			        ObjectBrowser.Items.Add(item);
			}
		}

		private void ChangeObjectBrowserToDrives()
		{
			Program.OptionsObject.ProgramObjectBrowserDirectory = "0:";
			var drives = DriveInfo.GetDrives();

			using (Dispatcher.DisableProcessing())
			{
			    ObjectBrowserDirBlock.Text = string.Empty;
			    ObjectBrowser.Items.Clear();
			    foreach (var dInfo in drives)
			    {
			        if (!dInfo.IsReady || (dInfo.DriveType != DriveType.Fixed && dInfo.DriveType != DriveType.Removable))
			            continue;

			        var tvi = new TreeViewItem()
			        {
			            Header = BuildTreeViewItemContent(dInfo.Name, "iconmonstr-folder-13-16.png"),
			            Tag = new ObjectBrowserTag() { Kind = ObjectBrowserItemKind.Directory, Value = dInfo.RootDirectory.FullName }
			        };

			        tvi.Items.Add("...");
			        ObjectBrowser.Items.Add(tvi);
			    }
			}
		}

		private IEnumerable<TreeViewItem> BuildDirectoryItems(string dir)
		{
			var itemList = new List<TreeViewItem>();
			var spFiles = Directory.GetFiles(dir, "*.sp", SearchOption.TopDirectoryOnly);
            var incFiles = Directory.GetFiles(dir, "*.inc", SearchOption.TopDirectoryOnly);
            var directories = Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly);

			foreach (var str in directories)
			{
				var dInfo = new DirectoryInfo(str);

				if (!dInfo.Exists)
					continue;

				var tvi = new TreeViewItem()
				{
					Header = BuildTreeViewItemContent(dInfo.Name, "iconmonstr-folder-13-16.png"),
					Tag = new ObjectBrowserTag() { Kind = ObjectBrowserItemKind.Directory, Value = dInfo.FullName }
				};

				tvi.Items.Add("...");
				itemList.Add(tvi);
			}

			foreach (var fileName in spFiles)
			{
				var fileInfo = new FileInfo(fileName);

				if (!fileInfo.Exists)
					continue;

				var tvi = new TreeViewItem()
				{
					Header = BuildTreeViewItemContent(fileInfo.Name, "iconmonstr-file-5-16.png"),
					Tag = new ObjectBrowserTag() { Kind = ObjectBrowserItemKind.File, Value = fileInfo.FullName }
				};

				tvi.MouseDoubleClick += TreeViewOBItemFile_DoubleClicked;
				itemList.Add(tvi);
			}

			foreach (var fileName in incFiles)
			{
				var fileInfo = new FileInfo(fileName);

				if (!fileInfo.Exists)
					continue;

				var tvi = new TreeViewItem()
				{
					Header = BuildTreeViewItemContent(fileInfo.Name, "iconmonstr-file-8-16.png"),
					Tag = new ObjectBrowserTag() { Kind = ObjectBrowserItemKind.File, Value = fileInfo.FullName }
				};

				tvi.MouseDoubleClick += TreeViewOBItemFile_DoubleClicked;
				itemList.Add(tvi);
			}

			return itemList;
		}

		private static object BuildTreeViewItemContent(string headerString, string iconFile)
		{
		    var stack = new StackPanel {Orientation = Orientation.Horizontal};
		    var image = new Image();
			string uriPath = $"/Spedit;component/Resources/{iconFile}";

			image.Source = new BitmapImage(new Uri(uriPath, UriKind.Relative));
			image.Width = 16;
			image.Height = 16;

		    var textBlock = new TextBlock
		    {
		        Text = headerString,
		        Margin = new Thickness(2.0, 0.0, 0.0, 0.0)
		    };

		    stack.Children.Add(image);
			stack.Children.Add(textBlock);

			return stack;
		}

		private class ObjectBrowserTag
		{
			public ObjectBrowserItemKind Kind;
			public string Value;
		}

		private enum ObjectBrowserItemKind
		{
			ParentDirectory,
			Directory,
			File
		}
	}
}
