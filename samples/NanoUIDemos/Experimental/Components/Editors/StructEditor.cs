using NanoUI.Common;
using NanoUI.Components;
using System;
using System.Numerics;
using System.Reflection;

namespace NanoUIDemos.Experimental.Components.Editors
{
    // TODO : Handle structs by creating new category in propertygrid & looping properties
    /*public class StructEditor : Widget, IPropertyEditor
    {
        public StructEditor()
            : base(null)
        {
            GridLayout layout =
               new GridLayout(Orientation.Horizontal, 2,
                              LayoutAlignment.Maximum, 0, 5);

            // set columns (label, widget)
            //layout.SetColAlignment(new LayoutAlignment[] { LayoutAlignment.Maximum, LayoutAlignment.Fill });
            layout.SetColAlignment(new LayoutAlignment[] { LayoutAlignment.Minimum, LayoutAlignment.Fill });
            // SETS TO ALIGN TOP
            layout.SetRowAlignment(LayoutAlignment.Minimum);

            layout.SetSpacing(0, 10);
            Layout = layout;
        }

        //public void SetValue(PropertyInfo propertyInfo, object? obj)
        public void SetValue(PropertyInfo propertyInfo, object? value, Action<PropertyInfo, object?> action)

        {
            if (value == null)
                return;

            
        }

        public override void Draw(NvgContext ctx)
        {
            base.Draw(ctx);
        }
    }*/
}
