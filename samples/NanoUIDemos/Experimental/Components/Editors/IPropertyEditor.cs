using NanoUI.Components;
using System.Reflection;

namespace NanoUIDemos.Experimental.Components.Editors
{
    public interface IPropertyEditor
    {
        // we set id so user can search for spesific editor in propertygrid
        // id is set to <property name>
        string? Name { get; internal set; }

        void InitEditor(PropertyInfo propertyInfo,
            Func<PropertyInfo, object?> getValue,
            Action<PropertyInfo, object?> setValue);

        // we use prorotype editor when creating new editor (no need to Activator.CreateInstance)
        // parent is set to category content panel
        IPropertyEditor Clone(UIWidget parent);
    }
}
