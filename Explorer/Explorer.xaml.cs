using Com.Xploreplus.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Org.Filedrops.FileSystem.UI
{
    public delegate BitmapImage GetIcon(FiledropsFileSystemEntry entry, string type = null, int size = 16, bool expanded = false);
}

namespace Org.Filedrops.FileSystem.UI.Explorer
{   

    /// <summary>
    /// Interaction logic for Explorer.xaml
    /// </summary>
    public partial class Explorer : UserControl
    {

        public static DependencyProperty CurrentEntryProperty =
            DependencyProperty.Register("CurrentEntry", typeof(FiledropsFileSystemEntry), typeof(Explorer), new UIPropertyMetadata(null));
        public FiledropsFileSystemEntry CurrentEntry
        {
            get { return (FiledropsFileSystemEntry)GetValue(CurrentEntryProperty); }
            set { SetValue(CurrentEntryProperty, value);}
        }

        /// <summary>
        /// Show folders in list
        /// </summary>
        public Boolean ShowFoldersList
        {
            get { return (Boolean)this.GetValue(ShowFoldersListProperty); }
            set { this.SetValue(ShowFoldersListProperty, value);
            }
        }
        public static readonly DependencyProperty ShowFoldersListProperty = DependencyProperty.Register(
          "ShowFoldersList", typeof(Boolean), typeof(Explorer));


        /// <summary>
        /// Show files in tree and list
        /// </summary>
        public Boolean ShowFiles
        {
            get { return (Boolean)this.GetValue(ShowFilesProperty); }
            set { this.SetValue(ShowFilesProperty, value);}
        }
        public static readonly DependencyProperty ShowFilesProperty = DependencyProperty.Register(
          "ShowFiles", typeof(Boolean), typeof(Explorer), new PropertyMetadata(false));

        /// <summary>
        /// Show files in tree
        /// </summary>
        public Boolean ShowFilesList
        {
            get { return (Boolean)this.GetValue(ShowFilesListProperty); }
            set { this.SetValue(ShowFilesListProperty, value); }
        }
        public static readonly DependencyProperty ShowFilesListProperty = DependencyProperty.Register(
          "ShowFilesList", typeof(Boolean), typeof(Explorer), new PropertyMetadata(false));

        /// <summary>
        /// Show files in tree
        /// </summary>
        public Boolean ShowFilesTree
        {
            get { return (Boolean)this.GetValue(ShowFilesTreeProperty); }
            set { this.SetValue(ShowFilesTreeProperty, value); }
        }
        public static readonly DependencyProperty ShowFilesTreeProperty = DependencyProperty.Register(
          "ShowFilesTree", typeof(Boolean), typeof(Explorer), new PropertyMetadata(false));


        public string ExtensionFilter
        {
            get { return (string)this.GetValue(ExtensionFilterProperty); }
            set 
            { 
                this.SetValue(ExtensionFilterProperty, value);
                ListDisplay.FileExtensionFilter = value;
            }
        }
        public static readonly DependencyProperty ExtensionFilterProperty = DependencyProperty.Register(
          "ExtensionFilter", typeof(string), typeof(Explorer));
        public Explorer()
        {
            InitializeComponent();
            this.entrylist = new LinkedList<string>();
            this.ListDisplay.viewComponent.SelectionChanged += updateEntry;
            this.ListDisplay.viewComponent.MouseDoubleClick += updateDirectoryList;
            this.TreeDisplay.SelectedItemChanged += updateDirectoryTree;

            this.Prev.Content = ModernImageLibrary.GetImage((int) ModernImageLibrary.ArrowIcons.Previous, 16);
            this.Next.Content = ModernImageLibrary.GetImage((int)ModernImageLibrary.ArrowIcons.Next, 16);
            this.Up.Content = ModernImageLibrary.GetImage((int)ModernImageLibrary.ArrowIcons.Up, 16);
            this.Up.Click += goUp;
            this.Prev.Click += prev_click;
            this.Next.Click += next_click;


            updateButtons(this, null);

            this.PathLookup.KeyDown += checkPath;
            
        }


        public void checkPath(object sender, KeyEventArgs args)
        {       
            if (args.Key == Key.Enter)
            {
                FiledropsDirectory dir = this.TreeDisplay.RootDirectories[0].FileSystem.ConstructDirectory(this.PathLookup.Text);
                if (dir.Exists())
                {
                    this.ListDisplay.RootDirectory = dir;
                }
            }            
        }

        public void goUp(object sender, EventArgs args)
        {
            FiledropsDirectory dir = this.ListDisplay.RootDirectory as FiledropsDirectory;
            if (dir != null && dir.FullName != dir.FileSystem.WorkingDirectory.FullName && dir.Parent != null)
            {
                this.ListDisplay.RootDirectory = dir.Parent;
            }
            updateDirectoryList(this, null);
        }

        public void updateEntry(object sender, EventArgs args)
        {
            FiledropsFileSystemEntry entry = this.ListDisplay.SelectedItem as FiledropsFileSystemEntry;
            if (entry != null)
            {
                this.CurrentEntry = entry;
            }

        }

        public void updateDirectoryTree(object sender, EventArgs args)
        {
            FiledropsDirectory dir = this.TreeDisplay.SelectedFileSystemEntry as FiledropsDirectory;
            if (dir != null)
            {
                this.checkNewEntry();
                updateButtons(sender, args);
            }                
        }

        public void updateDirectoryList(object sender, EventArgs args)
        {
            FiledropsDirectory dir = this.ListDisplay.RootDirectory as FiledropsDirectory;
            if (dir != null)
            {
                this.checkNewEntry();
                updateButtons(sender, args);
            }      
        }

        public void updateButtons(object sender, EventArgs args)
        {
            FiledropsDirectory dir = ListDisplay.RootDirectory as FiledropsDirectory;
            if (dir != null)
            {
                this.Up.IsEnabled = (dir.FullName != dir.FileSystem.WorkingDirectory.FullName && dir.Parent != null);
                this.Next.IsEnabled = currentIndex > 0;
                this.Prev.IsEnabled = currentIndex < entrylist.Count - 1;
            }
            else
            {
                this.Up.IsEnabled = false;
                this.Prev.IsEnabled = false;
                this.Next.IsEnabled = false;
            }
        }

        public void AddRoot(FiledropsDirectory dir)
        {
            TreeDisplay.addRoot(dir);
        }

        public string getInput()
        {
            return System.IO.Path.Combine(this.currentDirName, this.FileSelectBox.Text).TrimEnd(new [] {'/', '\\'});
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (string.Equals(e.Property.Name, "ShowFiles", StringComparison.CurrentCultureIgnoreCase))
            {
                this.ListDisplay.ShowFiles = this.ShowFiles;
                this.TreeDisplay.ShowFiles = this.ShowFiles;
            }
            if (string.Equals(e.Property.Name, "ShowFilesTree", StringComparison.CurrentCultureIgnoreCase))
            {
                this.TreeDisplay.ShowFiles = this.ShowFilesTree;
            }
            if (string.Equals(e.Property.Name, "ShowFoldersList", StringComparison.CurrentCultureIgnoreCase))
            {
                this.ListDisplay.ShowFolders = this.ShowFoldersList;
            }
            if (string.Equals(e.Property.Name, "ShowFilesList", StringComparison.CurrentCultureIgnoreCase))
            {
                this.ListDisplay.ShowFiles = this.ShowFilesList;
            } 
            base.OnPropertyChanged(e);
        }

        /*
         * previous-next logic
         */

        private string currentDirName;
        private int currentIndex = 0;
        private LinkedList<string> entrylist;

        private void checkNewEntry()
        {
            FiledropsDirectory dir = this.ListDisplay.RootDirectory as FiledropsDirectory;
            string entry = dir.FullName;

            //put the previous entry first
            if (currentDirName != null)
            {
                entrylist.Remove(currentDirName);
                entrylist.AddFirst(currentDirName);
            }

            if (entrylist.Contains(entry))
            {
                entrylist.Remove(entry);
                entrylist.AddFirst(entry);
            }
            else
            {
                if (entrylist.Count == 10)
                {
                    entrylist.RemoveLast();
                }
                entrylist.AddFirst(entry);
            }
            currentIndex = 0;
            currentDirName = entry;
        }

        private void prev_click(object sender, EventArgs args)
        {
            if (currentIndex < entrylist.Count - 1)
            {
                currentIndex++;
                currentDirName = entrylist.ElementAt(currentIndex);
            }
            this.ListDisplay.RootDirectory = this.TreeDisplay.RootDirectories[0].FileSystem.ConstructDirectory(this.currentDirName);
            updateButtons(this, null);
        }

        private void next_click(object sender, EventArgs args)
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                currentDirName = entrylist.ElementAt(currentIndex);
            }
            this.ListDisplay.RootDirectory = this.TreeDisplay.RootDirectories[0].FileSystem.ConstructDirectory(this.currentDirName);
            updateButtons(this, null);
        }

        public void Connect(int connectionId, object target)
        {
            throw new NotImplementedException();
        }
    }
}
