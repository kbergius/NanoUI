using NanoUI.Components;
using NanoUIDemos.Experimental.Components.Editors;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUIDemos.Experimental.Components
{
    public class UIPropertyGridCategoryPanel : UICollapsablePanel
    {
        // Parent is UIPropertyGridPanel
        // We must get labels with there
        UIPropertyGridPanel _propertyGridPanel;

        public UIPropertyGridCategoryPanel(UIPropertyGridPanel widget)
            : base(widget)
        {
            _propertyGridPanel = widget;
        }

        #region Events

        public override void OnCollapseChanged(bool collapsed)
        {
            Content.Visible = !collapsed;

            // This should be because we must find vscroll
            if (Parent.Parent != null)
            {
                RequestLayoutUpdate(Parent.Parent);
            }
            else
            {
                RequestLayoutUpdate(Parent);
            }
        }

        #endregion

        #region Layout

        public override void PerformLayout(NvgContext ctx)
        {
            // set fixed width so we can calculate editors widths
            FixedSize = new Vector2(Parent.Size.X, 0);

            // set editor widths
            foreach (var child in Content.Children.AsReadOnlySpan())
            {
                if (child is IPropertyEditor)
                {
                    child.FixedSize = new Vector2(GetEditorWidth(), 0);
                }
            }

            base.PerformLayout(ctx);
        }

        int GetEditorWidth()
        {
            // todo: magical numbers
            return (int)(FixedSize.X - _propertyGridPanel.LabelsWidth - 20);
        }

        #endregion
    }
}
