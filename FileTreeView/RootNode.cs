using System;
using System.Windows.Data;

namespace Org.Filedrops.FileSystem.UI.FileTreeView
{
    public class RootNode: FolderTreeNode
    {

        public RootNode(FiledropsDirectory dir, Binding showExtBinding, FolderTreeNodeFilter filter = null)
            : base(dir, showExtBinding, filter)
        {

        }

        protected override void ExpandEvent(object sender, EventArgs args)
        {
            //only remove the dummy node
            if (this.Items.Contains(dummyNode))
                this.Items.Remove(dummyNode);
        }
    }
}
