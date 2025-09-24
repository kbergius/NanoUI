using NanoUI.Common;

namespace NanoUI.Components.Files
{
    // todo: here could be logic to create display text

    /// <summary>
    /// FileFolderInfo.
    /// </summary>
    public struct FileFolderInfo
    {
        /// <summary>
        /// Path
        /// </summary>
        public string Path;

        /// <summary>
        /// FileFolderType
        /// </summary>
        public FileFolderType FileFolderType;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="fileFolderType">FileFolderType</param>
        public FileFolderInfo(string path, FileFolderType fileFolderType)
        {
            Path = path;
            FileFolderType = fileFolderType;
        }
    }
}
