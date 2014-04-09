using Com.Xploreplus.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Org.Filedrops.FileSystem.UI.FileTreeView
{
    public class FileTree : TreeView, INotifyPropertyChanged
    {

        public List<FiledropsFileSystemEntry> RootDirectories { get; set; }

		public FileSystemEntryNode SelectedFileSystemEntryNode { get; set; }

		public static DependencyProperty SelectedFileSystemEntryProperty =
    DependencyProperty.Register("SelectedFileSystemEntry", typeof(FiledropsFileSystemEntry), typeof(FileTree), new UIPropertyMetadata(null));
        public FiledropsFileSystemEntry SelectedFileSystemEntry
		{
			get
			{
                return (FiledropsFileSystemEntry)GetValue(SelectedFileSystemEntryProperty);
			}
			set
			{
				SetValue(SelectedFileSystemEntryProperty, value);
			}
		}

        public bool ShowFiles { get; set; }

        public bool ShowRoot { get; set; }

        public bool ShowMenu
        {
            get;
            set;
        }

        public Dictionary<string, ContextMenu> Menus{get; set;} //key=tag

        public EventHandler DoubleClickHandler { get; set; }

        protected Binding showExtBinding;

        public static readonly DependencyProperty ShowFileExtensionsProperty = DependencyProperty.Register(
 "ShowFileExtensions", typeof(Boolean), typeof(FileSystemEntryNode));

        // Declare the event 
        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event 
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public bool ShowFileExtensions
        {
            get { return (bool)GetValue(ShowFileExtensionsProperty); }
            set
            {
                SetValue(ShowFileExtensionsProperty, value);
                OnPropertyChanged("ShowFileExtensions");
            }
        }


        public FileTree()
        {
            this.SelectedItemChanged += trv_SelectedItemChanged;
            this.KeyDown += treeView1_KeyDown;
            RootDirectories = new List<FiledropsFileSystemEntry>();        
            InitMenus();
            this.SelectedItemChanged += FileTree_SelectedItemChanged;
            this.PreviewMouseRightButtonDown += FileTree_MouseRightButtonDown;
            this.PreviewMouseLeftButtonDown += FileTree_MouseLeftButtonDown;
            this.PreviewMouseMove += FileTree_MouseMove;
            showExtBinding = new Binding()
            {
                Source = this,
                Path = new PropertyPath("ShowFileExtensions")
            };            
        }

        /// <summary>
        /// Initialize context menus
        /// </summary>
        public virtual void InitMenus()
        {
            Menus = new Dictionary<string, ContextMenu>();

            ContextMenu filecm = new ContextMenu();
            MenuItem RenameFile = new MenuItem();

            RenameFile.Click += RenameNode_Click;
            RenameFile.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Rename, 16);
            RenameFile.Name = "RenameFile";
            RenameFile.Header = "Rename";
            MenuItem RemoveFile = new MenuItem();
            RemoveFile.Click += RemoveFile_Click;
            RemoveFile.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Delete, 16);
            RemoveFile.Name = "RemoveFile";
            RemoveFile.Header = "Remove";
            filecm.Items.Add(RenameFile);
            filecm.Items.Add(RemoveFile);

            Menus.Add("File", filecm);

            ContextMenu foldercm = new ContextMenu();

            // The menu item for adding a file to the project
            MenuItem AddFile = new MenuItem();
            AddFile.Name = "AddFile";
            AddFile.Header = "Add File";
            AddFile.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FileIcons.Add, 16);
            AddFile.Click += AddFileToFolder_Click;

            // The menu item for adding a folder to the folder
            MenuItem AddFolder = new MenuItem();
            AddFolder.Name = "AddFolder";
            AddFolder.Header = "Add Folder";
            AddFolder.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FolderIcons.Add, 16);
            AddFolder.Click += AddFolderToFolder_Click;

            // The menu item for expanding all the folders and files in the folder
            MenuItem ExpandAll = new MenuItem();
            ExpandAll.Name = "ExpandAll";
            ExpandAll.Header = "Expand All";
            ExpandAll.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.ResizeIcons.Expand, 16);
            ExpandAll.Click += ExpandAll_Click;

            // The menu item for collapsing all the folders and files in the folder
            MenuItem CollapseAll = new MenuItem();
            CollapseAll.Name = "CollapseAll";
            CollapseAll.Header = "Collapse All";
            CollapseAll.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.ResizeIcons.Collapse, 16);
            CollapseAll.Click += CollapseAll_Click;

            // The menu item for renaming the folder
            MenuItem RenameFolder = new MenuItem();
            RenameFolder.Name = "RenameFolder";
            RenameFolder.Header = "Rename";
            RenameFolder.Click += RenameNode_Click;
            RenameFolder.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FolderIcons.Rename, 16);

            // The menu item for removing the folder 
            MenuItem RemoveFolder = new MenuItem();
            RemoveFolder.Name = "RemoveFolder";
            RemoveFolder.Header = "Remove";
            RemoveFolder.Icon = ModernImageLibrary.GetImage((int)ModernImageLibrary.FolderIcons.Delete, 16);
            RemoveFolder.Click += RemoveFolder_Click;

            // adding all the menu items to the context menu of folder
            foldercm.Items.Add(AddFile);
            foldercm.Items.Add(AddFolder);

            // Adds a nice seperator between Add and Expand
            foldercm.Items.Add(new Separator());

            foldercm.Items.Add(ExpandAll);
            foldercm.Items.Add(CollapseAll);

            // Adds a nice seperator between Collapse and Rename
            foldercm.Items.Add(new Separator());

            foldercm.Items.Add(RenameFolder);
            foldercm.Items.Add(RemoveFolder);

            Menus.Add("Folder", foldercm);
            
        }


        public virtual FolderTreeNode getSampleFolderNode(FiledropsDirectory dir)
        {
            return new RootNode(dir, showExtBinding);
        }

        public virtual FolderTreeNode addRoot(FiledropsDirectory dir)
        {
            FolderTreeNode node = getSampleFolderNode(dir);
            this.RootDirectories.Add(node.Entry);
            this.buildFolderRoot(node);
            if (ShowRoot)
            {                
                this.Items.Add(node);
            }
            return node;
        }


        public virtual void buildFolderRoot(FolderTreeNode node)
        {
            List<FiledropsFileSystemEntry> entries = (node.Entry as FiledropsDirectory).GetEntries();
            foreach (FiledropsFileSystemEntry entry in entries)
            {
                if (entry.EntryType == FileSystemEntryType.File)
                {
                    if (this.ShowFiles)
                    {
                        FileTreeNode item = node.CreateFileNode(entry as FiledropsFile);
                        item.FontWeight = FontWeights.Normal;
                        if (ShowRoot)
                            node.Items.Add(item);
                        else
                            this.Items.Add(item);
                    }
                }
                else
                {
                    FolderTreeNode item = node.createFolderNode(entry as FiledropsDirectory, null);
                    item.Tag = "Folder";
                    item.FontWeight = FontWeights.Normal;
                    if (ShowRoot)
                        node.Items.Add(item);
                    else
                        this.Items.Add(item);
                }
            }
            
        }


        static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {

            while (source != null && !(source is TreeViewItem))
            {
                if (source is Visual || source is Visual3D)
                {
                    source = VisualTreeHelper.GetParent(source);
                }
                else
                {
                    source = LogicalTreeHelper.GetParent(source);
                }
            }
            return source as TreeViewItem; 
        }

        private void FileTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<Object> e)
        {
/*
            if (e.OldValue != null)
            {
                ((FileSystemEntryNode)e.OldValue).FontWeight = FontWeights.Normal;
            }
            ((FileSystemEntryNode)e.NewValue).FontWeight = FontWeights.Bold;
 */ 
        }

        private void FileTree_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);
            if (treeViewItem != null)
            {
                treeViewItem.IsSelected = true;
                e.Handled = true;
            }
        }

        private Point mouseStartPosition;

        /// <summary>
        /// Stores a potential beginning of drag point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileTree_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position
            mouseStartPosition = e.GetPosition(null);
        }

        /// <summary>
        /// checks on drag
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileTree_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = mouseStartPosition - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                TreeViewItem selected = this.SelectedItem as TreeViewItem;
                if (selected is FileTreeNode)
                {
                    FileTreeNode ftn = SelectedItem as FileTreeNode;
                    //DataObject dragData = new DataObject("File", (ftn.Entry as FiledropsFile));
                    DataObject dragData = new DataObject(DataFormats.FileDrop, new string[] { ftn.Entry.FullName });
                    DragDrop.DoDragDrop(selected, dragData, DragDropEffects.Move);
                }
            }
        }



        /// <summary>
        /// Command which adds a file to a folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void AddFileToFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderTreeNode ftn = this.SelectedFileSystemEntryNode as FolderTreeNode;
            ftn.CreateFile();
        }
        
        /// <summary>
        /// Command which adds a new subfolder to a folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void AddFolderToFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderTreeNode ftn = this.SelectedFileSystemEntryNode as FolderTreeNode;
            ftn.createFolder();
        }

        /// <summary>
        /// Command which removes a folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void RemoveFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderTreeNode ftn = this.SelectedFileSystemEntryNode as FolderTreeNode;
            bool success = ftn.removeNode();
            if (success)
            {
                removeNode(ftn);
            }
        }

        /// <summary>
        /// Command which removes a file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void RemoveFile_Click(object sender, RoutedEventArgs e)
        {
            FileTreeNode ftn = this.SelectedFileSystemEntryNode as FileTreeNode;
            bool success = ftn.removeNode();
            if (success)
            {
                removeNode(ftn);
            }
            else
            {
                //TODO: log exception
            }
        }

        public void RenameNode_Click(object sender, RoutedEventArgs e)
        {
            FileSystemEntryNode node = this.SelectedFileSystemEntryNode as FileSystemEntryNode;
            node.RenameNode();
        }

        public void removeNode(FileSystemEntryNode item)
        {
            if (item.Parent == this)
            {
                this.Items.Remove(item);
            }
            else
            {
                TreeViewItem parent = item.Parent as TreeViewItem;
                parent.Items.Remove(item);
            }
        }

        

        private void trv_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            FileSystemEntryNode selectedItem = this.SelectedItem as FileSystemEntryNode;
            //selectedItem.FontStyle = FontStyles.Italic;
            this.SelectedFileSystemEntryNode = selectedItem;
            if(selectedItem != null)
            {
			    this.SelectedFileSystemEntry = selectedItem.Entry;
                if (selectedItem.Focus())
                {
                    if (selectedItem.Tag == null)
                        return;
                    ContextMenu cm;
                    Menus.TryGetValue(selectedItem.Tag.ToString(), out cm);
                    selectedItem.ContextMenu = cm;
                }
            }
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
                SetCurrentItemInEditMode();
            if (e.Key == Key.F5)
                refreshTree();
            if (e.Key == Key.Delete)
                deleteCurrentItem();
        }

        private void deleteCurrentItem()
        {
            FileSystemEntryNode entrynode  = this.SelectedItem as FileSystemEntryNode;
            if (entrynode != null)
            {
                bool success = entrynode.removeNode();
                if (success)
                {
                    removeNode(entrynode);
                }
                else
                {
                    //TODO: log exception
                }
            }
        }

        private void refreshTree()
        {
            foreach (TreeViewItem item in this.Items)
            {
                FolderTreeNode node = item as FolderTreeNode;
                if (node != null)
                {                    
                    node.refresh();
                }
            }
        }

        private void SetCurrentItemInEditMode()
        {
            // Make sure that the SelectedItem is actually a TreeViewItem
            // and not null or something else
            if (this.SelectedItem is FileSystemEntryNode)
            {
                FileSystemEntryNode tvi = this.SelectedItem as FileSystemEntryNode;
                tvi.RenameNode();                             
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void ExpandAll_Click(object sender, EventArgs args)
        {
            TreeViewItem node = this.SelectedItem as TreeViewItem;
            node.ExpandSubtree();
        }

        protected void CollapseAll_Click(object sender, EventArgs args)
        {
            TreeViewItem node = this.SelectedItem as TreeViewItem;
            CollapseNode(node);
        }

        protected void CollapseNode(TreeViewItem node)
        {
            if (node != null && node.Items != null && node.Items.Count > 0)
            {
                foreach (TreeViewItem item in node.Items)
                {
                    if(item != null) CollapseNode(item);
                }
                node.IsExpanded = false;
            }
        }

        /// <summary>
        /// Tries to select a path in the tree
        /// todo" add option for multiple roots?
        /// </summary>
        /// <param name="path"></param>
        public void selectPath(string path)
        {
            if (this.RootDirectories.Count > 0)
            {
                FiledropsDirectory dir = this.RootDirectories[0].FileSystem.ConstructDirectory(path);
                FiledropsDirectory rootdir = this.RootDirectories[0].FileSystem.WorkingDirectory;
                if (dir.Exists() && dir.FullName.Contains(rootdir.FullName))
                {
                    List<FiledropsDirectory> dirlist = new List<FiledropsDirectory>();
                    FiledropsDirectory currentdir = dir;
                    while (currentdir.FullName != rootdir.FullName)
                    {
                        dirlist.Add(currentdir);
                    }
                    //open the path to the folder
                    FolderTreeNode currentnode = this.Items[0] as FolderTreeNode;
                    int i = 0;
                    while (currentnode != null && currentnode.Entry.FullName != dir.FullName)
                    {
                        currentnode = currentnode.ExpandToChild(dirlist[i].FullName);
                    }
                    currentnode.IsSelected = true;
                }
            }
        }
    }
}
