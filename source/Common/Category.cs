namespace NanoUI.Common
{
    // this is used in category panel (property grid) to combine properties to categories
    public struct Category
    {
        // this is for objects to refer in Catergory attribute
        public string Id;
        public string DisplayText;

        // used for sorting
        public int SortKey;
    }
}