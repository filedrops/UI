using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace Org.Filedrops.FileSystem.UI.Listview
{
	/// <summary>
	/// Interaction logic for FiledropsFileListItem.xaml
	/// </summary>
	public partial class FiledropsFileListItem : ListViewItem, INotifyPropertyChanged
	{
		public FiledropsFileListItem()
		{
			InitializeComponent();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

	    public void Connect(int connectionId, object target)
	    {
	        throw new NotImplementedException();
	    }
	}
}
