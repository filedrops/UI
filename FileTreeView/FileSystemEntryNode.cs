using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Org.Filedrops.FileSystem.UI.FileTreeView
{
    public class FileSystemEntryNode : TreeViewItem, INotifyPropertyChanged
    {
        public FiledropsFileSystemEntry Entry { get; set; }

        public static readonly DependencyProperty ShowExtensionProperty = DependencyProperty.Register(
    "ShowExtension", typeof(Boolean), typeof(FileSystemEntryNode));

        protected string oldName;

        public bool ShowExtension
        {
            get { return (bool)GetValue(ShowExtensionProperty); }
            set
            {
                SetValue(ShowExtensionProperty, value);
            }
        }

        public Image NodeIcon { get; set; }
        public EditableTextBlock TextBlock { get; set; }

        protected Binding ShowExtBinding { get; set; }

        public FileSystemEntryNode(FiledropsFileSystemEntry entry, Binding showExt)
        {
            Style s = new Style(typeof(FileSystemEntryNode));
            s.Resources.Add(SystemColors.HighlightBrushKey, Brushes.Transparent);
            s.Resources.Add(SystemColors.HighlightTextBrushKey, Brushes.Black);
            s.Resources.Add(SystemColors.InactiveSelectionHighlightBrushKey, Brushes.Transparent);
            s.Resources.Add(SystemColors.InactiveSelectionHighlightTextBrushKey, Brushes.Black);
            Style = s;

            //FocusVisualStyle

            Entry = entry;
            ShowExtBinding = showExt;
            BindingOperations.SetBinding(this, FileSystemEntryNode.ShowExtensionProperty, showExt);

            StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal };
            NodeIcon = new Image();
            TextBlock = new EditableTextBlock();
            TextBlock.PropertyChanged += doEdit;
            sp.Children.Add(NodeIcon);
            sp.Children.Add(TextBlock);
            this.Header = sp;

            GotFocus += new RoutedEventHandler(ItemGotFocus);
            LostFocus += new RoutedEventHandler(ItemLostFocus);
            sp.MouseEnter += new MouseEventHandler(ItemMouseEnter);
            sp.MouseLeave += new MouseEventHandler(ItemMouseLeave);
        }

        public virtual bool removeNode()
        {
            return true;
        }

        public virtual void doEdit(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "IsInEditMode")
            {
                try
                {
                    this.Entry.Rename(TextBlock.Text);
                }
                catch (Exception e)
                {
                    this.TextBlock.Text = oldName;
                    MessageBox.Show("Failed to rename.");
                }
            }
        }

        public virtual void ItemGotFocus(object sender, RoutedEventArgs e)
        {
            if (IsSelected)
            {
                FontWeight = FontWeights.Bold;
                Foreground = Brushes.Gray;
            }
        }

        public virtual void ItemLostFocus(object sender, RoutedEventArgs e)
        {
            FontWeight = FontWeights.Normal;
            Foreground = Brushes.Black;
        }

        public virtual void ItemMouseEnter(object sender, MouseEventArgs e)
        {
            FontWeight = FontWeights.Bold;
        }

        public virtual void ItemMouseLeave(object sender, MouseEventArgs e)
        {
            if (!IsSelected)
            {
                FontWeight = FontWeights.Normal;
            }
        }

        public virtual void RenameNode()
        {

            // Finally make sure that we are
            // allowed to edit the TextBlock
            oldName = this.TextBlock.Text;
            if (this.TextBlock.IsEditable)
                this.TextBlock.IsInEditMode = true;

        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void checkExt(object sender, DataTransferEventArgs args)
        {

        }

        protected void Notify(string propertyName)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
