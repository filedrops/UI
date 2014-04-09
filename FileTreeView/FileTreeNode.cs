using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace Org.Filedrops.FileSystem.UI.FileTreeView
{
    public class FileTreeNode : FileSystemEntryNode
    {

        public FileTreeNode(FiledropsFile fi, Binding showext)
            : base(fi, showext)
        {
            initLabel();
            this.Tag = "File";
        }


        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (string.Equals(e.Property.Name, "ShowExtension", StringComparison.CurrentCultureIgnoreCase))
            {
                if(this.IsLoaded)
                    initLabel();
            }
            base.OnPropertyChanged(e);
        }

        /// <summary>
        /// Initializes the label for the filenode
        /// </summary>
        /// <param name="showext">show extension or not</param>
        public void initLabel()
        {
            bool showext = this.ShowExtension;
            if (showext)
            {
                this.TextBlock.Text = this.Entry.Name;
            }
            else
            {
                this.TextBlock.Text = Entry.NameWithoutExtension;
            }
            NodeIcon.Source = Entry.Icon16x16;
        }


        /// <summary>
        /// removes the file
        /// </summary>
        /// <summary>
        /// Removes the folder the node represents
        /// </summary>
        public override bool removeNode()
        {
            bool success = false;
            string messageBoxText = "Are you sure you want to delete this file?";
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
                this.Entry.Delete();
            }
            return success;
        }


        public override void doEdit(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "IsInEditMode")
                try
                {
                    this.Entry.Rename(TextBlock.Text + (this.Entry as FiledropsFile).Extension);
                }
                catch
                {
                    this.TextBlock.Text = oldName;
                    MessageBox.Show("Failed to rename.");
                }
                finally 
                {
                    if (this.Parent != null)
                    {
                        (this.Parent as FolderTreeNode).refresh();
                    }
                }

        }
    }
}
