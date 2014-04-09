using System.Windows;
using System.Windows.Controls;

namespace Org.Filedrops.FileSystem.UI.Listview
{
	class FolderMenu : ContextMenu
	{
		private MenuItem add, delete, rename;
		private FiledropsFileList list;

		public FolderMenu(FiledropsFileList list)
		{
			this.list = list;

			add = new MenuItem() { Header = "Add Folder" };
			delete = new MenuItem() { Header = "Delete" };
			rename = new MenuItem() { Header = "Rename" };

			add.Click += add_Click;
			delete.Click += delete_Click;
			rename.Click += rename_Click;

			AddChild(add);
			AddChild(delete);
			AddChild(rename);
		}

		void rename_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			
		}

		void delete_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			string sMessageBoxText = "Are you sure you want to delete this file?";
			string sCaption = "Deleting file";

			MessageBoxButton btnMessageBox = MessageBoxButton.YesNo;
			MessageBoxImage icnMessageBox = MessageBoxImage.Warning;

			MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

			switch (rsltMessageBox)
			{
				case MessageBoxResult.No:
					break;

				case MessageBoxResult.Yes:
					FiledropsFileSystemEntry entry = list.SelectedItem as FiledropsFileSystemEntry;
					entry.Delete();
					list.updateViewCollection();
					break;
			}
		}

		void add_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			
		}

	}
}
