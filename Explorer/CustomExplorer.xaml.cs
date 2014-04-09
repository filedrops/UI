using System.Windows;
using System.Windows.Controls;

namespace Org.Filedrops.FileSystem.UI.Explorer
{
	/// <summary>
	/// Interaction logic for Explorer.xaml
	/// </summary>
	public partial class CustomExplorer : UserControl
	{
		public void AddRoot(FiledropsDirectory dir)
		{
			TreeDisplay.addRoot(dir);
		}

		public CustomExplorer()
		{
			InitializeComponent();
		}

		private void extensions_Checked(object sender, RoutedEventArgs e)
		{
			ListDisplay.ShowFileExtensions = true;
		}

		private void extensions_Unchecked(object sender, RoutedEventArgs e)
		{
			ListDisplay.ShowFileExtensions = false;
		}

		private void folders_Checked(object sender, RoutedEventArgs e)
		{
			ListDisplay.ShowFolders = true;
		}

		private void folders_Unchecked(object sender, RoutedEventArgs e)
		{
			ListDisplay.ShowFolders = false;
		}

		private void layout_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ComboBox c = sender as ComboBox;
			
			if ((c.SelectedValue as ComboBoxItem).Content == null) { return; }

			switch ((c.SelectedValue as ComboBoxItem).Content.ToString())
			{
				case "Details":
					ListDisplay.Layout = UI.Listview.FiledropsFileListLayout.Details;
					break;
				case "List":
					ListDisplay.Layout = UI.Listview.FiledropsFileListLayout.List;
					break;
				case "BigIcons":
					ListDisplay.Layout = UI.Listview.FiledropsFileListLayout.BigIcons;
					break;
			}
		}

	    public void Connect(int connectionId, object target)
	    {
	        throw new System.NotImplementedException();
	    }
	}
}
