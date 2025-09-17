using NanoUI;
using NanoUI.Common;
using Color = NanoUI.Common.Color;
using NanoUI.Components;
using NanoUI.Components.Colors;
using NanoUI.Layouts;
using System.Numerics;

namespace NanoUIDemos.UI
{
    // This shows same set of widgets in different layouts (widgets use minimal set of configurations).
    public class UILayoutDemo : DemoBase
    {
        // SplitPanel widget provides basic 2-panel functionality (you can also manually set split)
        bool useSplitPanel = true;

        public UILayoutDemo(UIScreen screen)
            :base(screen)
        {
            var info = new UILabel(screen, "We use same widgets (minimal configuration) in all layouts to show how layout positions them.");
            info.Position = new Vector2(20);
            info.TextColor = Color.White;
            info.FontSize = 22;

            GroupLayout();
            StackLayoutHorizontal();
            StackLayoutVertical();
            GridLayout();
            FlowLayout();
            SplitLayout();
        }

        #region GroupLayout

        void GroupLayout()
        {
            if(_screen == null)
                return;

            var window = new UIWindow(_screen, "GroupLayout");
            window.Position = new Vector2(20, 170);
            window.ChildrenLayout = new GroupLayout();

            CreateLayoutWidgets(window);
        }

        #endregion

        #region StackLayouts

        void StackLayoutHorizontal()
        {
            if (_screen == null)
                return;

            var window = new UIWindow(_screen, "StackLayout-Horizontal");
            window.Position = new Vector2(20, 70);
            window.ChildrenLayout = new StackLayout(Orientation.Horizontal, LayoutAlignment.Middle) { Spacing = new Vector2(10)};
            // TODO
            window.FixedSize =  new Vector2(1100, 80);
            CreateLayoutWidgets(window);
        }

        void StackLayoutVertical()
        {
            if (_screen == null)
                return;

            var window = new UIWindow(_screen, "StackLayout-Vertical");
            window.Position = new Vector2(220, 170);
            window.ChildrenLayout = new StackLayout(Orientation.Vertical) { Spacing = new Vector2(10) };

            CreateLayoutWidgets(window);
        }

        #endregion

        #region GridLayout

        void GridLayout()
        {
            if (_screen == null)
                return;

            var window = new UIWindow(_screen, "GridLayout");
            window.Position = new Vector2(490, 170);

            var layout = new GridLayout(Orientation.Horizontal, 2, LayoutAlignment.Middle)
            { Spacing = new Vector2(5) };

            layout.SetColumnAlignments([LayoutAlignment.Minimum, LayoutAlignment.Fill]);
            layout.SetSpacing(0, 10);
            window.ChildrenLayout = layout;

            CreateLayoutWidgets(window);
        }

        #endregion

        #region FlowLayout

        void FlowLayout()
        {
            if (_screen == null)
                return;

            var window = new UIWindow(_screen, "FlowLayout");
            window.Position = new Vector2(820, 170);
            window.Size = new Vector2(300);

            window.ChildrenLayout = new FlowLayout()
            {
                Spacing = new Vector2(10),
            };

            CreateLayoutWidgets(window);

            // note: flow layout works best with same size of widgets
            foreach (var child in window.Children.AsSpan())
            {
                child.FixedSize = new Vector2(100, 25);
            }
        }

        #endregion

        #region SplitLayout

        void SplitLayout()
        {
            if (_screen == null)
                return;

            var window = new UIWindow(_screen, "SplitLayout");
            window.Position = new Vector2(490, 415);
            window.ChildrenLayout = new GroupLayout();

            if (useSplitPanel)
            {
                var splitPanel = new UISplitPanel(window) { Orientation = Orientation.Horizontal };
                splitPanel.Size = new Vector2(600, 200);

                if (splitPanel.Panel1 != null)
                {
                    splitPanel.Panel1.ChildrenLayout = new GridLayout() { DefaultColumnAlignment = LayoutAlignment.Fill, Spacing = new Vector2(10) };
                    CreateLayoutWidgets(splitPanel.Panel1);
                }
                if (splitPanel.Panel2 != null)
                {
                    splitPanel.Panel2.ChildrenLayout = new GridLayout() { DefaultColumnAlignment = LayoutAlignment.Fill, Spacing = new Vector2(10) };
                    CreateLayoutWidgets(splitPanel.Panel2);
                } 
            }
            else
            {
                var splitterContainer = new UIWidget(window);
                splitterContainer.Size = new Vector2(600, 200);

                // split layout
                splitterContainer.ChildrenLayout = new SplitLayout(Orientation.Horizontal);

                var panel1 = new UIWidget(splitterContainer);
                panel1.ChildrenLayout = new GridLayout() { DefaultColumnAlignment = LayoutAlignment.Fill, Spacing = new Vector2(10) };
                CreateLayoutWidgets(panel1);

                new UISplitter(splitterContainer, Orientation.Horizontal);

                var panel2 = new UIWidget(splitterContainer);
                panel2.ChildrenLayout = new GridLayout() { DefaultColumnAlignment = LayoutAlignment.Fill, Spacing = new Vector2(10) };
                CreateLayoutWidgets(panel2);
            }
        }

        #endregion

        #region Layout widgets

        void CreateLayoutWidgets(UIWidget parent)
        {
            // TextBox
            {
                new UILabel(parent, "TextBox:");

                var textBox = new UITextField(parent, "äöåÄÖÅ");
                textBox.Editable = true;
                textBox.TextHorizontalAlignment = TextHorizontalAlign.Left;
            }

            // float
            {
                new UILabel(parent, "NumericTextBox:");

                var floatBox = new UINumericTextBox<float>(parent, 50);
                floatBox.Editable = true;
                floatBox.Units = "GiB";
                floatBox.TextHorizontalAlignment = TextHorizontalAlign.Right;
            }

            // Checkbox
            {
                new UILabel(parent, "Checkbox:");

                var cb = new UICheckBox(parent, "Check me");
                cb.Checked = true;
            }

            new UILabel(parent, "Color picker:");

            var cp = new UIColorPicker(parent, new Color(255, 120, 0, 255));
            cp.FinalColor += (color) => { };
            // set min size so this displays correctly
            cp.MinSize = new Vector2(100, 0);

            // Simple combobox
            new UILabel(parent, "ComboBox:");

            var comboBox = new UIComboBox<int>(parent);
            // set min size so this displays correctly
            comboBox.MinSize = new Vector2(100, 0);

            for (int i = 0; i < 50; i++)
            {
                comboBox.AddItem("Item-" + i, i);
            }

            comboBox.SelectedIndex = 5;
        }

        #endregion
    }
}
