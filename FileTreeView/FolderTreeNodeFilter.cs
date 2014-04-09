using System.Collections.Generic;
using System.Linq;

namespace Org.Filedrops.FileSystem.UI.FileTreeView
{
    public class FolderTreeNodeFilter
    {
        /// <summary>
        /// Allowed extensions.
        /// </summary>
        public string[] Extensions { get; set; }

        /// <summary>
        /// Allowed subdirectories.
        /// </summary>
        public string[] Subdirectories { get; set; }

        public FolderTreeNodeFilter(string[] ext = null, string[] subdirs = null)
        {
            this.Extensions = ext;
            this.Subdirectories = subdirs;
        }

        public List<FiledropsFileSystemEntry> getAllowedDirectoryContents(FiledropsDirectory dir, FileSystemEntryType entryType = FileSystemEntryType.Undefined)
        {
            if (dir == null) return null;

            List<FiledropsFileSystemEntry> allowedContents = new List<FiledropsFileSystemEntry>();

            foreach(FiledropsFileSystemEntry entry in dir.GetEntries()){
                switch (entry.EntryType)
                {
                    case FileSystemEntryType.File:
                        if ((entry as FiledropsFile).Extension != null && (entry as FiledropsFile).Extension.Length > 1)
                        {
                            string strippedExtension = (entry as FiledropsFile).Extension.Substring(1);
                            if ((entryType.Equals(FileSystemEntryType.Undefined) || entryType.Equals(FileSystemEntryType.File))
                                && (Extensions == null || Extensions.Length == 0 || Extensions.Contains(strippedExtension)))
                                allowedContents.Add(entry as FiledropsFile);
                        }
                        break;
                    case FileSystemEntryType.Folder:
                        if ((entryType.Equals(FileSystemEntryType.Undefined) || entryType.Equals(FileSystemEntryType.Folder))
                            && (Subdirectories == null || Subdirectories.Length == 0 || Subdirectories.Contains(entry.Name)))
                            allowedContents.Add(entry as FiledropsDirectory);
                        break;
                }
            }

            return allowedContents;
        }

        public string getFirstExtension()
        {
            if (Extensions != null && Extensions.Length > 0) return Extensions[0];
            return null;
        }
    }
}
