using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Org.Filedrops.FileSystem.UI.Listview
{
	/// <summary>
	/// Interaction logic for FiledropsListview.xaml
	/// </summary>
	public partial class FiledropsFileList : UserControl, INotifyPropertyChanged
	{
        private GetIcon getIcon;
		private List<ListViewItem> viewCollection = new List<ListViewItem>();
		private FileMenu fileMenu;
		private FolderMenu folderMenu;
        public FiledropsFileSystemEntry SelectedItem
		{
			get {
                if (viewComponent.SelectedIndex >= 0)
                    return viewCollection[viewComponent.SelectedIndex].Content as FiledropsFileSystemEntry;
                else
                    return null;
            }
		}

		public FiledropsFileList()
		{
			InitializeComponent();
            viewComponent.MouseDoubleClick += item_DoubleClick;
		}

		public FiledropsFileList(FiledropsDirectory rootdir = null,
									bool showFolders = false,
									bool showFileExt = false,
									string extensionFilter = ".*",
									string directoryFilter = ".*",
									FiledropsFileListLayout layout = FiledropsFileListLayout.BigIcons,
                                    GetIcon function = null)
		{
			InitializeComponent();
            this.getIcon = function;
			fileMenu = new FileMenu(this);
			folderMenu = new FolderMenu(this);

			ContextMenu = fileMenu;
			viewComponent.ItemsSource = viewCollection;
			viewComponent.MouseDoubleClick += item_DoubleClick;

			ShowFolders = showFolders;
			ShowFileExtensions = showFileExt;
			DirectoryNameFilter = directoryFilter;
			FileExtensionFilter = extensionFilter;
			this.Layout = layout;
			RootDirectory = rootdir;

			this.KeyDown += (s, e) =>
			{
				if (e.Key == Key.F2 && !InEditMode)
					InEditMode = true;		
			};

			viewComponent.Loaded += (s, e) => { resetColumnWidths(); Layout = FiledropsFileListLayout.BigIcons;  };
		}

		public void updateViewCollection()
		{
			if (RootDirectory as FiledropsDirectory == null) { return; }
			FiledropsDirectory d = RootDirectory as FiledropsDirectory;

			if (ShowFolders && ShowFiles)
                fillViewCollection<FiledropsFileSystemEntry>(d.GetEntries());
			else if(ShowFolders)
				fillViewCollection<FiledropsDirectory>(d.GetDirectories());
            else if(ShowFiles)
                fillViewCollection<FiledropsFile>(d.GetFiles());
			viewComponent.ItemsSource = viewCollection;
			viewComponent.Items.Refresh();
		}

        private BitmapImage getIconSource(FiledropsFileSystemEntry entry)
        {
            BitmapImage img = null; 
            if (this.getIcon != null)
            {
                img = getIcon(entry, null, 32, false);
            }
            if(img == null){
                img = entry.Icon32x32;
            }
            return img;
        }

        private void fillViewCollection<T>(List<T> list) where T : FiledropsFileSystemEntry
		{
			viewCollection.Clear();

            foreach (FiledropsFileSystemEntry entry in list)
			{
				if (entry is FiledropsDirectory)
				{
					if (DirectoryNameFilter != null)
					{
						if (Regex.Match(entry.Name, DirectoryNameFilter).Success)
							viewCollection.Add(new ListViewItem() { ContextMenu = folderMenu, Content = entry, });
					}
					else
					{
						viewCollection.Add(new ListViewItem() { ContextMenu = folderMenu, Content = entry, });
					}
				}
				else if (entry is FiledropsFile)
				{
					if (FileExtensionFilter != null)
					{
						if (Regex.Match((entry as FiledropsFile).Extension, FileExtensionFilter).Success)
							viewCollection.Add(new ListViewItem() { ContextMenu = fileMenu, Content = entry });
					}
					else
					{
						viewCollection.Add(new ListViewItem() { ContextMenu = fileMenu, Content = entry });
					}
				}

			}
		}

		private void item_DoubleClick(object sender, EventArgs e)
		{
			if (viewComponent.SelectedIndex < 0) { return; }

			if (SelectedItem is FiledropsDirectory)
			{
				// open this folder
				RootDirectory = SelectedItem as FiledropsDirectory;
			}
			else if (SelectedItem is FiledropsFile)
			{
				// open this file
			}
		}

		private void resetColumnWidths()
		{
			GridView gridView = viewComponent.View as GridView;
			if (gridView != null)
			{
				foreach (var col in gridView.Columns)
				{
					if (double.IsNaN(col.Width)) { col.Width = col.ActualWidth; col.Width += 50; }
					col.Width = double.NaN;
				}
			}
		}

		private void renamer_Enter(object sender, KeyEventArgs args)
		{
			if (sender is TextBox && args.Key == Key.Enter)
			{
				TextBox t = sender as TextBox;
				SelectedItem.Rename(t.Text);
				updateViewCollection();
				InEditMode = false;
			}
		}

		private void renamer_LostFocus(object sender, EventArgs args)
		{
			if (sender is TextBox)
			{
				TextBox t = sender as TextBox;
				SelectedItem.Rename(t.Text);
				updateViewCollection();
				InEditMode = false;
			}
		}


		/// <summary>
		/// START OF PROPERTY SECTION
		/// </summary>

		# region Properties

		public static DependencyProperty RootProperty =
            DependencyProperty.Register("RootDirectory", typeof(FiledropsFileSystemEntry), typeof(FiledropsFileList), new UIPropertyMetadata(null));
        public FiledropsFileSystemEntry RootDirectory
		{
            get { return (FiledropsFileSystemEntry)GetValue(RootProperty); }
			set { SetValue(RootProperty, value); updateViewCollection(); }
		}

        public static DependencyProperty ShowFoldersProperty =
            DependencyProperty.Register("ShowFolders", typeof(bool), typeof(FiledropsFileList), new UIPropertyMetadata(false));
        public bool ShowFolders
        {
            get { return (bool)GetValue(ShowFoldersProperty); }
            set { SetValue(ShowFoldersProperty, value); updateViewCollection(); }
        }

		public static DependencyProperty ShowFileExtensionsProperty =
			DependencyProperty.Register("ShowFileExtensions", typeof(bool), typeof(FiledropsFileList), new UIPropertyMetadata(false));

		public bool ShowFileExtensions
		{
			get { return (bool)GetValue(ShowFileExtensionsProperty); }
			set
			{
				SetValue(ShowFileExtensionsProperty, value);
				updateViewCollection();
			}
		}

        public static DependencyProperty ShowFilesProperty =
            DependencyProperty.Register("ShowFiles", typeof(bool), typeof(FiledropsFileList), new UIPropertyMetadata(false));

        public bool ShowFiles
        {
            get { return (bool)GetValue(ShowFilesProperty); }
            set
            {
                SetValue(ShowFilesProperty, value);
                updateViewCollection();
            }
        }

		private string _directoryNameFilter;
		public string DirectoryNameFilter
		{
			get { return _directoryNameFilter; }
			set { _directoryNameFilter = value; updateViewCollection(); }
		}

		private string _fileExtensionFilter;
		public string FileExtensionFilter
		{
			get { return _fileExtensionFilter; }
			set { _fileExtensionFilter = value; updateViewCollection(); }
		}

		public static DependencyProperty LayoutProperty =
			DependencyProperty.Register("Layout", typeof(FiledropsFileListLayout), typeof(FiledropsFileList), new UIPropertyMetadata(FiledropsFileListLayout.BigIcons));

		public FiledropsFileListLayout Layout
		{
			get { return (FiledropsFileListLayout)GetValue(LayoutProperty); }
			set { SetValue(LayoutProperty, value); }
		}

		public static DependencyProperty EditProperty =
			DependencyProperty.Register("InEditMode", typeof(bool), typeof(FiledropsFileList), new UIPropertyMetadata(false));

		public bool InEditMode
		{
			get { return (bool)GetValue(EditProperty); }
			set { SetValue(EditProperty, value); }
		}
		# endregion


		public event PropertyChangedEventHandler PropertyChanged;

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (string.Equals(e.Property.Name, "RootDirectory", StringComparison.CurrentCultureIgnoreCase))
            {
				updateViewCollection();
            }
            if (string.Equals(e.Property.Name, "ShowFolders", StringComparison.CurrentCultureIgnoreCase))
            {
                updateViewCollection();
            }
            if (string.Equals(e.Property.Name, "ShowFiles", StringComparison.CurrentCultureIgnoreCase))
            {
                updateViewCollection();
            }
            base.OnPropertyChanged(e);
        }


	    public void Connect(int connectionId, object target)
	    {
	        throw new NotImplementedException();
	    }
	}

	public enum FiledropsFileListLayout : short
	{
		BigIcons, NormalIcons, SmallIcons, List, Details
	}
}
