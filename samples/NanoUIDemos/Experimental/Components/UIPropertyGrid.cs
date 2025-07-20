using NanoUI.Common;
using NanoUI.Components;
using NanoUI.Layouts;
using NanoUI.Components.Scrolling;
using NanoUIDemos.Experimental.Components.Editors;
using NanoUIDemos.Experimental.Utils;
using NanoUI.Nvg;
using System.Numerics;
using System.Reflection;

namespace NanoUIDemos.Experimental.Components
{
    #region UIPropertyGridPanel

    public class UIPropertyGridPanel : UIWidget
    {
        public UIPropertyGridPanel(UIWidget parent)
            : base(parent)
        {
            ChildrenLayout = new StackLayout(Orientation.Vertical, LayoutAlignment.Fill);
        }

        // we must have labels width specified so editors can check

        public int LabelsWidth;

        #region Events

        // we must clear pointer focus on category (collapsable) panel titlebars
        // note : titlebar is button that reacts to "hover" event
        public override bool OnPointerMove(Vector2 p, Vector2 rel)
        {
            foreach(var child in Children.AsReadOnlySpan())
            {
                child.PointerFocus = false;
            }

            return base.OnPointerMove(p, rel);
        }

        #endregion

        #region Layout

        public override void PerformLayout(NvgContext ctx)
        {
            // todo:
            Size = new Vector2(Parent.Size.X, Size.Y);

            if (Parent is UIScrollPanel scrollPanel)
            {
                Size -= new Vector2(scrollPanel.VerticalScrollbar.Dimension, 0);
            }
            
            base.PerformLayout(ctx);
        }

        #endregion
    }

    #endregion

    // todo : Quaternion Editor : from/to euler angles (Vector3)
    // todo : get more attributes from propertyinfo
    // todo : there should be editors based on fieldName for eaxmple:
    // - FontFaceId => DropdownEditor (not numeric int editor)
    // todo: don't use GetValueFunc & SetValueFunc
    // - action PropertyChanged & let user handle when obvject changes outside
    // - don't store object here

    // note : grid doesn't store any reference to the analyzed object
    // since structs would be copy anyway.
    // Instead this uses delegates (GetValueFunc, SetValueFunc)
    public class UIPropertyGrid : UIWidget
    {
        UIScrollPanel _scroll;
        UIPropertyGridPanel _propertyGridPanel;

        // Prototypes
        Dictionary<Type, IPropertyEditor> _prototypes = new();

        // if new type is same as old type we don't need to create grid again
        Type _currentType;

        public Func<PropertyInfo, object?> GetValueFunc;
        public Action<PropertyInfo, object?> SetValueFunc;

        public UIPropertyGrid(UIWidget parent)
            : base(parent)
        {
            // init editors first - so we can clear children
            InitEditors();

            _scroll = new UIScrollPanel(this, ScrollbarType.Vertical);
            _propertyGridPanel = new UIPropertyGridPanel(_scroll);

            LabelsWidth = 120;
        }

        #region Properties

        // TODO : more properties???
        public int LabelsWidth
        {
            get => _propertyGridPanel.LabelsWidth;
            set => _propertyGridPanel.LabelsWidth = value;
        }

        #endregion

        #region Methods

        public void AddEnumEditor<T>() where T : struct, Enum
        {
            // add
            AddEditor<T>(new EnumEditor<T>(this));
        }

        public void AddEditor<T>(IPropertyEditor editor)
        {
            // add/replace
            _prototypes[typeof(T)] = editor;

            // remove from children
            if (editor is UIWidget w && Children.Contains(w))
            {
                Children.Remove(w);
            }
        }

        // note: we don't store object in grid since structs creates copy of the original
        public void Set<T>(T obj)
        {
            if (obj == null)
                return;

            Type newType = obj.GetType();

            if (_currentType == newType)
            {
                return;
            }

            _currentType = newType;

            // note: we don't need to specially dispose editors (unhook events), since they are
            // disposed as underlying widgets are disposed
            // and we do not create any event listeners directly to current object

            // clear previous
            _propertyGridPanel.Children.Clear();

            Category cat;
            List<PropertyInfo> infos;

            foreach (var kvp in CategorySorter.GetCategories(_currentType))
            {
                cat = kvp.Key;
                infos = kvp.Value;

                // Content panel where we property labels & editors
                // todo : category panel could be collapsed by default (except first)?
                var categoryPanel = CategorySorter.CreateCategoryPanel(cat.DisplayText, _propertyGridPanel);
                var contentPanel = categoryPanel.Content;

                // Loop properties in category
                foreach (PropertyInfo property in infos)
                {
                    // get params
                    var name = property.Name;
                    var propertyType = property.PropertyType;

                    // note: we set value dynamically in editor's Draw function
                    // so it supports both editor change & outside propertygrid change

                    // this could be too nested???
                    if (propertyType == obj.GetType())
                        continue;

                    // we must test first since we create label first
                    if (_prototypes.ContainsKey(propertyType))
                    {
                        // create label first
                        var label = new UILabel(contentPanel, name);

                        // set fixed width - so all editors align horizontally
                        label.FixedSize = new Vector2(LabelsWidth, 0);

                        // create editor
                        CreateEditor(property, contentPanel);
                    }
                }
            }

            // layout update
            RequestLayoutUpdate(this);
        }

        #endregion

        #region Layout

        public override void PerformLayout(NvgContext ctx)
        {
            _scroll.FixedSize = Size;
            base.PerformLayout(ctx);
        }

        #endregion

        #region Private

        void InitEditors()
        {
            // add to dictionary for lookup
            // todo : set min & max values etc
            _prototypes.Add(typeof(string), new StringEditor(this));
            _prototypes.Add(typeof(bool), new BoolEditor(this));
            _prototypes.Add(typeof(int), new NumericEditor<int>(this));
            _prototypes.Add(typeof(uint), new NumericEditor<uint>(this));
            _prototypes.Add(typeof(Color), new ColorEditor(this));
            _prototypes.Add(typeof(Vector2), new Vector2Editor(this));
            _prototypes.Add(typeof(float), new NumericEditor<float>(this));
            _prototypes.Add(typeof(double), new NumericEditor<double>(this));
            // TODO: more numeric

            // enums
            _prototypes.Add(typeof(PointerType), new EnumEditor<PointerType>(this));
            _prototypes.Add(typeof(TextHorizontalAlign), new EnumEditor<TextHorizontalAlign>(this));
            _prototypes.Add(typeof(TextVerticalAlign), new EnumEditor<TextVerticalAlign>(this));

            // we clear children since we don't use these prototypes
            Children.Clear();
        }

        void CreateEditor(PropertyInfo property, UIWidget parent)
        {
            // get params
            var propertyType = property.PropertyType;

            if (_prototypes.TryGetValue(propertyType, out var prototype))
            {
                IPropertyEditor editor;

                // special handling for numeric editors since we pass format
                // also thts should be read from propertyInfo attributes
                if(prototype is INumericEditor numericEditor)
                {
                    // create new with format
                    editor = numericEditor.Clone(parent, NumericFormat.NONE);
                }
                else
                {
                    editor = prototype.Clone(parent);
                }
                

                // we set id so user can search for spesific editor in propertygrid
                // id is #<propertyname>
                editor.Name = property.Name;

                // init editor with callbacks to get/set value
                editor.InitEditor(
                    property,
                    GetValueFunc,
                    SetValueFunc);
            }
        }

        #endregion
    }
}