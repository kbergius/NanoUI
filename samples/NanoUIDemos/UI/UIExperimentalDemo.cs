using NanoUI;
using NanoUI.Common;
using NanoUI.Components;
using NanoUI.Layouts;
using NanoUI.Components.Scrolling;
using NanoUIDemos.Experimental.Components;
using System.Numerics;
using System.Reflection;

namespace NanoUIDemos.UI
{
    public class UIExperimentalDemo : DemoBase
    {
        // store reference, since this is updated
        UIRoundMeter? _speedMeter;

        public UIExperimentalDemo(UIScreen screen)
            :base(screen)
        {
            // SearchBox, SwitchBoxes, Dials, ToleranceBar
            TestMiscWidgets(screen);

            TextArea();

            TestGraphs(screen);

            TestPropertyGrid(screen);
        }

        #region MiscWidgets

        void TestMiscWidgets(UIScreen screen)
        {
            var window = new UIWindow(screen, "Search, Switch, Dial ...")
            {
                Position = new Vector2(30),
                ChildrenLayout = new GroupLayout()
            };
            
            var lblEvent = new UILabel(window, "Event Value")
            {
                TextColor = Color.Red
            };
            
            var lbl = new UILabel(window, "SearchBox");

            var search = new UISearchBox(window);

            search.Search += (val) =>
            {
                lblEvent.Caption = "Search: " + val;
            };

            new UILabel(window, "SwitchBoxes");

            var switchPanel = new UIWidget(window)
            {
                ChildrenLayout = new GridLayout(Orientation.Horizontal, 2)
            };

            // horizontal
            new UISwitchBox(switchPanel)
            {
                Size = new Vector2(80, 30),
                Checked = true
            };

            // vertical
            new UISwitchBox(switchPanel)
            {
                Size = new Vector2(28, 60),
                Checked = true,
                Orientation = Orientation.Vertical
            };

            new UILabel(window, "Dial and text box");

            var dialWidget = new UIWidget(window)
            {
                ChildrenLayout = new StackLayout(Orientation.Horizontal, LayoutAlignment.Middle) { Spacing = new Vector2(20) }
            };

            var dial = new UIDial(dialWidget)
            {
                Value = 0,
                FixedSize = new Vector2(80, 0),
            };

            var dialText = new UITextField(dialWidget)
            {
                FixedSize = new Vector2(60, 25),
                Text = "0",
                TextHorizontalAlignment = TextHorizontalAlign.Right
            };

            dial.ValueChanged += (value) =>
            {
                dial.HighlightRange = new MinMax(0.0f, value);
                value = value * 100;

                dialText.Text = string.Format("{0:0}", value);
            };

            dial.FinalValue += (value) =>
            {
                value = value * 100;

                lblEvent.Caption = "Dial Final : " + string.Format("{0:0}", value);
            };

            new UILabel(window, "ToleranceBar");

            new UIToleranceBar(window)
            {
                FixedSize = new Vector2(230, 50),
                Value = 45
            };
        }

        #endregion

        #region TextArea

        const string _text =
            "Lorem ipsum dolor sit amet\n" +
            "consectetur adipiscing elit,\n" +
            "sed do eiusmod tempor incididunt ut\n" +
            "labore et dolore magna aliqua.\n\n" +
            "Ut enim ad minim veniam.\n";

        void TextArea()
        {
            if (_screen == null)
                return;

            var window = new UIWindow(_screen, "TextArea")
            {
                Position = new Vector2(30, 380),
                ChildrenLayout = new GroupLayout()
            };
            
            var scrollPanel = new UIScrollPanel(window)
            {
                Size = new Vector2(250, 200)
            };
            
            var textArea = new UITextArea(scrollPanel)
            {
                Selectable = true,
                Padding = 10,
                SelectionColor = Color.Blue,
                CaretColor = Color.Red
            };
            
            // add text
            for (int i = 0; i < 13; i++)
            {
                textArea.Append("LINES " + i + ":\n" + _text);
            }
        }

        #endregion

        #region Graphs, Meters

        void TestGraphs(UIScreen screen)
        {
            var window = new UIWindow(screen, "Graphs & meters")
            {
                Position = new Vector2(350, 30),
                ChildrenLayout = new GroupLayout()
            };
            
            // Graph
            new UILabel(window, "Simple graph");

            var graph = new UIGraph(window);
            graph.Size = new Vector2(280, 200);

            // generate example values
            List<float> values = new();

            for (int i = 0; i < 360;)
            {
                float rad = float.DegreesToRadians(i);

                values.Add(0.5f + 0.4f * MathF.Sin(rad));

                i += 5;
            }

            graph.Values = values;

            // SpeedMeter
            var lblSpeed = new UILabel(window, "SpeedMeter")
            {
                TextColor = Color.LightGreen
            };
            
            _speedMeter = new UIRoundMeter(window)
            {
                UnitsText = "km/h",
                Caption = "Speed",
                MinValue = 0,
                MaxValue = 200,
                Threshold = 120,
                Size = new Vector2(160)
            };          

            _speedMeter.ValueChanged += (val) =>
            {
                lblSpeed.Caption = "SpeedMeter: " + val.ToString();
            };

            _speedMeter.OverThreshold += (val) =>
            {
                if (val)
                    lblSpeed.TextColor = Color.Red;
                else
                    lblSpeed.TextColor = Color.LightGreen;
            };

            // Perf Graph
            new UILabel(window, "Perf Graph");

            var perfGraph = new UIPerformanceGraph(window)
            {
                Size = new Vector2(200, 40)
            };
        }

        #endregion

        #region PropertyGrid

        object? _propertGridObj;

        void TestPropertyGrid(UIScreen screen)
        {
            var window = new UIWindow(screen, "PropertyGrid")
            {
                Position = new Vector2(720, 30),
                ChildrenLayout = new GroupLayout()
            };
            
            // test object
            _propertGridObj = new UILabel(window, "TestObject ");

            // wrap actions to reflect get/set methods
            var propertyGrid = new UIPropertyGrid(window)
            {
                Size = new Vector2(370, 500),
                GetValueFunc = GetValue,
                SetValueFunc = SetValue
            };            

            propertyGrid.Set(_propertGridObj);
        }

        public object? GetValue(PropertyInfo property)
        {
            // note : for structs this should be dynamic
            // classes work with static _currentObj
            if (_propertGridObj != null)
            {
                return property?.GetValue(_propertGridObj);
            }

            return null;
        }

        public void SetValue(PropertyInfo property, object? value)
        {
            // note : for structs this should be dynamic,
            // classes work with static _currentObj
            if (_propertGridObj != null && value != null)
            {
                property?.SetValue(_propertGridObj, value);
            }
        }

        #endregion

        public override void Update(float deltaSeconds)
        {
            if (_speedMeter != null)
            {
                _speedMeter.CurrentValue += deltaSeconds * 20;

                if (_speedMeter.CurrentValue >= _speedMeter.MaxValue)
                    _speedMeter.CurrentValue = _speedMeter.MinValue;
            }

            base.Update(deltaSeconds);
        }
    }
}
