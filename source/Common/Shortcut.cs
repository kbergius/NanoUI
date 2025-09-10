using System.Text;

namespace NanoUI.Common
{
    /// <summary>
    /// Shortcut for menus.
    /// </summary>
    public struct Shortcut
    {
        /// <summary>
        /// Key. Default Key.Unknown.
        /// </summary>
        public Key Key;

        /// <summary>
        /// KeyModifiers. Default KeyModifiers.None.
        /// </summary>
        public KeyModifiers Modifiers;

        public Shortcut(Key key)
        {
            Key = key;
        }

        public Shortcut(Key key, KeyModifiers modifiers)
        {
            Key = key;
            Modifiers = modifiers;
        }

        /// <summary>
        /// Check if Key + KeyModifiers match shortcut.
        /// </summary>
        public bool Match(Key key, KeyModifiers modifiers)
        {
            return key == Key && Modifiers == modifiers;
        }

        public override string ToString()
        {
            // not set
            if (Key == Key.Unknown)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            // control
            if ((Modifiers & KeyModifiers.Control) != 0)
            {
                sb.Append("Ctrl+");
            }

            // alt
            if ((Modifiers & KeyModifiers.Alt) != 0)
            {
                sb.Append("Alt+");
            }

            // shift
            if ((Modifiers & KeyModifiers.Shift) != 0)
            {
                sb.Append("Shift+");
            }

            // key
            sb.Append(Key.ToString());

            return sb.ToString();
        }
    }
}
