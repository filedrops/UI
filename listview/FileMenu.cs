using System.Windows;
using System.Windows.Controls;

namespace Org.Filedrops.FileSystem.UI.Listview
{
	class FileMenu : ContextMenu
	{
		private MenuItem delete, rename;
		private FiledropsFileList list;

		public FileMenu(FiledropsFileList file)
		{
			this.list = file;

			delete = new MenuItem() { Header = "Delete" };
			rename = new MenuItem() { Header = "Rename" };

			delete.Click += delete_Click;
			rename.Click += rename_Click;

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
					FiledropsFileSystemEntry entry = list.SelectedItem;
					entry.Delete();
					list.updateViewCollection();
					break;
			}
		}

	}
}
