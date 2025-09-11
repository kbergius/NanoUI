using NanoUI.Common;

namespace NanoUI.Components.Files
{
    // todo: here could be logic to create display text

    /// <summary>
    /// FileFolderInfo.
    /// </summary>
    public struct FileFolderInfo
    {
        public string Path;
        public FileFolderType FileFolderType;

        public FileFolderInfo(string path, FileFolderType fileFolderType)
        {
            Path = path;
            FileFolderType = fileFolderType;
        }
    }
}
