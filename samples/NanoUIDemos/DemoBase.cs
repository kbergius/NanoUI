using NanoUI;
using NanoUI.Common;
using NanoUI.Components;
using NanoUI.Components.Bars;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUIDemos
{
    public abstract class DemoBase
    {
        protected UIScreen _screen;
        public UIScreen Screen => _screen;

        public DemoBase(UIScreen screen)
        {
            _screen = screen;

            // we use this for all titlebars
            UITitlebar.ButtonClicked += OnTitlebarButtonClicked;
        }

        #region Inputs

        public virtual bool OnPointerUpDown(Vector2 pointerPos, PointerButton button, bool down)
        {
            if(Screen == null)
                return false;

            return Screen.OnPointerUpDown(pointerPos, button, down);
        }

        public virtual bool OnPointerMove(Vector2 pointerPos, Vector2 rel)
        {
            if (Screen == null)
                return false;

            return Screen.OnPointerMove(pointerPos, rel);
        }

        public virtual bool OnPointerDoubleClick(Vector2 pointerPos, PointerButton button)
        {
            if (Screen == null)
                return false;

            return Screen.OnPointerDoubleClick(pointerPos, button);
        }

        public virtual bool OnPointerScroll(Vector2 pointerPos, Vector2 scroll)
        {
            if (Screen == null)
                return false;

            return Screen.OnPointerScroll(pointerPos, scroll);
        }

        public virtual bool OnKeyUpDown(Key key, bool down, KeyModifiers modifiers)
        {
            if (Screen == null)
                return false;

            return Screen.OnKeyUpDown(key, down, modifiers);
        }

        public virtual bool OnKeyChar(char c)
        {
            if (Screen == null)
                return false;

            return Screen.OnKeyChar(c);
        }

        public virtual void ScreenResize(Vector2 size, NvgContext ctx)
        {
            Screen?.OnScreenResize(size, ctx);
        }

        public virtual bool OnFileDrop(string filename)
        {
            if (Screen == null)
                return false;

            return Screen.OnFileDrop(filename);
        }

        #endregion

        // we use this for all titlebars & their button actions
        void OnTitlebarButtonClicked(UIWidget titlebar, int buttonIcon)
        {
            var themeIcons = _screen.Theme.Fonts;

            if(buttonIcon == themeIcons.IconClose)
            {
                // we call close that can be overridden in widget implementation OR here (show message box ...)
                // note: Close() in base Widget just disposes widget
                titlebar?.Parent?.Close();
            }
        }

        public virtual void Update(float deltaSeconds)
        {
            Screen?.Update(deltaSeconds);
        }

        public virtual void Draw(NvgContext ctx)
        {
            Screen?.Draw(ctx);
        }

        public virtual void Dispose()
        {
            Screen?.Dispose();
        }
    }
}