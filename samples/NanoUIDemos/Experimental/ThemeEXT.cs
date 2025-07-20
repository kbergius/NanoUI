using NanoUI;
using NanoUI.Common;
using NanoUIDemos.Experimental.Components;

namespace NanoUIDemos.Experimental
{
    public class ThemeEXT : UITheme
    {
        public ThemeEXT()
        {
        
        }

        public UIDial Dial
        {
            get => Get<UIDial>();
            set => Set(value);
        }

        public UIGraph Graph
        {
            get => Get<UIGraph>();
            set => Set(value);
        }

        public UIPerformanceGraph PerformanceGraph
        {
            get => Get<UIPerformanceGraph>();
            set => Set(value);
        }

        public UIRoundMeter RoundMeter
        {
            get => Get<UIRoundMeter>();
            set => Set(value);
        }

        public UISearchBox SearchBox
        {
            get => Get<UISearchBox>();
            set => Set(value);
        }

        public UISeparator Separator
        {
            get => Get<UISeparator>();
            set => Set(value);
        }

        public UISwitchBox SwitchBox
        {
            get => Get<UISwitchBox>();
            set => Set(value);
        }

        public UIToleranceBar ToleranceBar
        {
            get => Get<UIToleranceBar>();
            set => Set(value);
        }

        public void PopulateExt()
        {
            // Dial
            Dial.MarkerColor = new Color(250, 50, 50, 150);
            Dial.HighlightColor = Common.AccentColor;
            Dial.FaceColor = new Color(80, 80, 80, 100);
            Dial.FaceColor2 = Color.Transparent;

            // Graph
            Graph.FillColor = Color.DarkBlue;
            Graph.FillColor2 = Color.Red;
            Graph.StrokeColor = Color.LightBlue;
            Graph.StrokeWidth = 4;
            Graph.AnimationThreshold = 0.05;

            // PerformanceGraph
            PerformanceGraph.FillColor = Common.AccentColor;
            PerformanceGraph.Mode = PerfGraphMode.FPS;

            // RoundMeter
            RoundMeter.MeterBackgroundColor = Color.Black;
            RoundMeter.CrownColor = Color.White;
            RoundMeter.CrownColor2 = new Color(60, 60, 60, 250);
            RoundMeter.OverThresholdColor = Color.Red;
            RoundMeter.UnderThresholdColor = Color.LightGreen;
            RoundMeter.NeedleColor = new Color(255, 120, 120, 255);
            RoundMeter.NeedleColor2 = new Color(200, 20, 20, 255);
            RoundMeter.NeedleHatColor = Color.White;
            RoundMeter.NeedleHatColor2 = Color.Black;

            // Separator
            Separator.SeparatorColor = new Color(128, 128, 128, 160);

            // SwitchBox
            SwitchBox.CheckedColor = new Color(139, 0, 0, 255);
            SwitchBox.CheckedColor2 = new Color(139, 0, 0, 128);
            SwitchBox.KnobColor = new Color(80, 80, 80, 100);
            SwitchBox.KnobColor2 = Color.Transparent;

            // ToleranceBar
            ToleranceBar.ValueColor = Common.AccentColor;
        }
    }
}