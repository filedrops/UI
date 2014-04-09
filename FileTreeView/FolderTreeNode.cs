using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Org.Filedrops.FileSystem.UI.FileTreeView
{
    public class FolderTreeNode : FileSystemEntryNode
    {
        private BitmapImage closed;
        private BitmapImage open;

        private string name;
        private bool recursive;

        protected object dummyNode = null;

        public FolderTreeNodeFilter filter;

        public FolderTreeNode(FiledropsDirectory d, Binding showExt, FolderTreeNodeFilter filter = null, bool recursive = true)
            : base(d, showExt)
        {
            this.recursive = recursive;
            this.Tag = "Folder";
            this.Expanded += new RoutedEventHandler(ExpandEvent);
            this.Items.Add(dummyNode);
            this.filter = filter;
            name = d.Name;
            open = d.OpenIcon16x16;
            closed = d.Icon16x16;
            initLabel();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (string.Equals(e.Property.Name, "IsExpanded", StringComparison.CurrentCultureIgnoreCase))
            {
                initLabel();
            }
            base.OnPropertyChanged(e);
        }

        public void initLabel()
        {

            if (this.IsExpanded)
            {
                NodeIcon.Source = open;
            }
            else
            {
                NodeIcon.Source = closed;
            }
            this.TextBlock.Text = this.Entry.Name;
        }

        /// <summary>
        /// Creates a file in this folder
        /// </summary>
        /// <param name="name"></param>
        public virtual void CreateFile()
        {
            //call the expandevent first to avoid thread issues
            ExpandEvent(this, null);
            this.IsExpanded = true;
            FiledropsFile file = this.Entry.FileSystem.ConstructFileRecursive(this.Entry.FullName + @"\", this.Entry.Name, filter.getFirstExtension());
            this.addFileNode(file);
        }


        /// <summary>
        /// Shows an input dialog to create a new file 
        /// </summary>
        /// <param name="isFolder">true if the 'file' to create has to be a folder</param>
        /// <returns>Path of the newly created file</returns>
        public virtual void createFolder()
        {
            ExpandEvent(this, null);
            this.IsExpanded = true;
            FiledropsDirectory dir = this.Entry.FileSystem.ConstructDirectoryRecursive(this.Entry.FullName + @"\" + this.Entry.Name);
            this.addFolderNode(dir);
        }


        /// <summary>
        /// Removes the folder the node represents
        /// </summary>
        public override bool removeNode()
        {
            bool success = false;
            string messageBoxText = "Are you sure you want to delete this folder with all its files?";
            string caption = "Filedrops";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);

            // Process message box results 
            switch (result)
            {
                case MessageBoxResult.Yes:
                    success = true;
                    break;
                case MessageBoxResult.No:
                    break;
            }
            if (success)
            {
                (this.Entry as FiledropsDirectory).Delete(true);
            }

            return success;
        }

        /// <summary>
        /// Adds a folder node 
        /// </summary>
        /// <param name="dir"></param>
        public virtual void addFolderNode(FiledropsDirectory dir)
        {
            FolderTreeNode node = createFolderNode(dir, filter);

            this.Items.Add(node);
            node.RenameNode();
        }

        /// <summary>
        /// Adds a file node
        /// </summary>
        /// <param name="file"></param>
        public virtual void addFileNode(FiledropsFile file)
        {
            FileTreeNode node = CreateFileNode(file);
            this.Items.Add(node);
            node.RenameNode();
        }


        public virtual FolderTreeNode createFolderNode(FiledropsDirectory dir, FolderTreeNodeFilter filter = null, bool recursive = true)
        {
            if (filter == null)
                return new FolderTreeNode(dir, this.ShowExtBinding, this.filter, recursive);
            else
                return new FolderTreeNode(dir, this.ShowExtBinding, filter, recursive);
        }

        public virtual FileTreeNode CreateFileNode(FiledropsFile file)
        {
            return new FileTreeNode(file, this.ShowExtBinding);
        }

        protected FileTree getTree()
        {
            FileSystemEntryNode node = this;
            while (node.Parent is FileSystemEntryNode)
            {
                node = node.Parent as FileSystemEntryNode;
            }
            return node.Parent as FileTree;
        }

        public void RebuildExpandedTreeNode(FolderTreeNode item)
        {
            FileTree tree = getTree();
            if (item != null)
            {
                FiledropsDirectory dirinfo = item.Entry as FiledropsDirectory;
                if (item.Items.Count > 0 && item.Items[0] == dummyNode)
                {
                    Dictionary<string, FolderTreeNodeFilter> dirfilters = new Dictionary<string, FolderTreeNodeFilter>();
                    foreach (object node in item.Items)
                    {
                        if (node is FolderTreeNode)
                        {
                            FolderTreeNode n = node as FolderTreeNode;
                            dirfilters.Add(n.Entry.FullName, n.filter);
                        }
                    }

                    item.Items.Clear();
                    try
                    {
                        foreach (FiledropsFileSystemEntry entry in filter.getAllowedDirectoryContents(dirinfo))
                        {
                            FileSystemEntryNode node;
                            switch (entry.EntryType)
                            {
                                case FileSystemEntryType.File:
                                    if (tree.ShowFiles && this.recursive)
                                    {
                                        node = item.CreateFileNode(entry as FiledropsFile);
                                        node.FontWeight = FontWeights.Normal;
                                        item.Items.Add(node);
                                    }
                                    break;
                                case FileSystemEntryType.Folder:
                                    if (dirfilters.ContainsKey(entry.FullName))
                                    {
                                        node = item.createFolderNode(entry as FiledropsDirectory, dirfilters[entry.FullName]);
                                    }
                                    else
                                    {
                                        node = item.createFolderNode(entry as FiledropsDirectory, item.filter);                                        
                                    }
                                    node.Tag = "Folder";
                                    node.FontWeight = FontWeights.Normal;
                                    item.Items.Add(node);
                                    break;
                            }
                        }
                    }
                    catch (Exception e) { }
                }
            }
        }

        protected virtual void ExpandEvent(object sender, EventArgs args)
        {
            FileTree tree = getTree();
            Cursor cur = Cursors.Wait;
            tree.Cursor = cur;
            FolderTreeNode item = (FolderTreeNode)sender;

            RebuildExpandedTreeNode(item);

            tree.Cursor = Cursors.Arrow;
        }

        public override void doEdit(object sender, PropertyChangedEventArgs args)
        {
            bool wasexp = this.IsExpanded;
            string old = this.Entry.FullName;
            base.doEdit(sender, args);
            if (this.Entry.FullName != old)
            {
                this.Items.Clear();
                this.Items.Add(dummyNode);
                if (wasexp)
                {
                    this.ExpandEvent(this, null);
                }
                this.IsExpanded = wasexp;
                if (this.Parent is FolderTreeNode)
                {
                    (this.Parent as FolderTreeNode).refresh();
                }
                else
                {
                    this.refresh();
                }
            }
        }

        public void refresh()
        {
            //check if node still exists
            if (this.Entry.Exists())
            {
                this.Items.Clear();
                this.Items.Add(dummyNode);
                this.ExpandEvent(this, null);
            }
            else
            {
                if (this.Parent is TreeViewItem)
                {
                    (this.Parent as TreeViewItem).Items.Remove(this);
                }
                else if (this.Parent is TreeView)
                {
                    (this.Parent as TreeView).Items.Remove(this);
                }
            }
        }



        //not used
        public FolderTreeNode ExpandToChild(string path)
        {
            this.ExpandEvent(this, null);
            this.IsExpanded = true;
            foreach (TreeViewItem item in this.Items)
            {
                FileSystemEntryNode node = item as FileSystemEntryNode;
                if (node.Entry.FullName == path)
                {
                    return node as FolderTreeNode;
                }
            }
            return null;
        }


    }
}
