namespace NanoUI.Common
{
    /// <summary>
    /// Used in property grid to combine properties to categories
    /// </summary>
    public struct Category
    {
        /// <summary>
        /// Category id.
        /// </summary>
        public string Id;

        /// <summary>
        /// DisplayText.
        /// </summary>
        public string DisplayText;

        /// <summary>
        /// SortKey. Used in sorting.
        /// </summary>
        public int SortKey;
    }
}
