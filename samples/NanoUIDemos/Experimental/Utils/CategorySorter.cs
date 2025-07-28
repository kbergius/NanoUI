using NanoUI;
using NanoUI.Common;
using NanoUI.Layouts;
using NanoUIDemos.Experimental.Components;
using System.ComponentModel;
using System.Reflection;

namespace NanoUIDemos.Experimental.Utils
{
    // todo : there could be also used specified PropertyInfo list sorter
    public static class CategorySorter
    {
        static Category _miscCategory;

        // categories are struct buffer, so user get ref for category struct when editing category
        static Category[] _categories = Array.Empty<Category>();

        // todo: user can add / remove?
        public static ReadOnlySpan<Category> CategoryList => new(_categories);

        // result
        static SortedDictionary<Category, List<PropertyInfo>> _sortedRes = new(new CategoryComparer());

        static CategorySorter()
        {
            // init default categories
            foreach (var categoryId in Globals.GetDefaultCategoryIds())
            {
                int index = _categories.Length;
                Category category = new();

                category.Id = categoryId;
                category.DisplayText = categoryId;
                category.SortKey = index;

                Array.Resize(ref _categories, index + 1);

                _categories[index] = category;
            }

            // special handling for "not found category"
            _miscCategory = new();
            _miscCategory.Id = Globals.CATEGORY_MISC;
            _miscCategory.DisplayText = Globals.CATEGORY_MISC;
            _miscCategory.SortKey = 1000;

            Array.Resize(ref _categories, _categories.Length + 1);

            _categories[_categories.Length - 1] = _miscCategory;
        }

        // note : we get only properties with get & set (not fields)
        public static SortedDictionary<Category, List<PropertyInfo>> GetCategories(Type type)
        {
            // clear previous
            _sortedRes.Clear();

            // Create category - set first
            Category defaultCat = new Category
            {
                Id = type.Name,
                DisplayText = type.Name,
                SortKey = -1
            };

            Category cat;

            foreach (PropertyInfo prop in type.GetProperties())
            {
                if (!IsDisplayProperty(prop))
                {
                    continue;
                }

                var categoryAttr = prop.GetCustomAttribute<CategoryAttribute>();

                if (categoryAttr == null)
                {
                    cat = defaultCat;
                }
                else
                {
                    // we have attribute
                    cat = GetCategory(categoryAttr.Category);
                }

                if (!_sortedRes.TryGetValue(cat, out var list))
                {
                    list = new List<PropertyInfo>();
                    _sortedRes[cat] = list;
                }

                list.Add(prop);
            }

            return _sortedRes;
        }

        // get category if defined in category list
        // if not found (no sort order defined) -> use misc
        static ref Category GetCategory(string categoryAttr)
        {
            for(int i = 0; i < _categories.Length; i++)
            {
                if (_categories[i].Id == categoryAttr)
                    return ref _categories[i];
            }

            return ref _miscCategory;
        }

        // check if we display property
        static bool IsDisplayProperty(PropertyInfo prop)
        {
            if (prop.GetGetMethod() == null || prop.GetSetMethod() == null)
            {
                return false;
            }

            // must have public get/set && non-static
            // we checked already nulls so "!"
            if (!prop.GetGetMethod()!.IsPublic ||
                prop.GetGetMethod()!.IsStatic ||
                !prop.GetSetMethod()!.IsPublic ||
                prop.GetSetMethod()!.IsStatic)
            {
                return false;
            }

            // MARKED NON BROWSABLE
            // Specifies whether a property or event should be displayed in a
            // Properties window.
            var browsableAttr = prop.GetCustomAttribute<BrowsableAttribute>();
            if (browsableAttr != null && !browsableAttr.Browsable)
            {
                return false;
            }

            return true;
        }

        internal static UIPropertyGridCategoryPanel CreateCategoryPanel(string category, UIPropertyGridPanel parent)
        {
            // Create category panel to hold properties (in content)
            var categoryPanel = new UIPropertyGridCategoryPanel(parent);
            categoryPanel.Header.Caption = category;

            // Define content
            var contentPanel = categoryPanel.Content;

            GridLayout layout =
                new GridLayout(Orientation.Horizontal, 2,
                               LayoutAlignment.Fill)
                { Spacing = new System.Numerics.Vector2(8)};

            // set columns (label, widget)
            layout.SetColumnAlignments(new[]{
                LayoutAlignment.Minimum,
                LayoutAlignment.Fill });

            // rows align top
            layout.DefaultRowAlignment = LayoutAlignment.Minimum;

            contentPanel.ChildrenLayout = layout;

            return categoryPanel;
        }
    }

    internal class CategoryComparer : IComparer<Category>
    {
        public int Compare(Category a, Category b)
        {
            return a.SortKey.CompareTo(b.SortKey);
        }
    }
}
