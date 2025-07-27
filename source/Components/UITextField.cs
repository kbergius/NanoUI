using NanoUI.Common;
using NanoUI.Nvg;
using NanoUI.Utils;
using System;
using System.Numerics;
using System.Text;
using Color = NanoUI.Common.Color;

namespace NanoUI.Components
{
    // todo : units could also be BEFORE number (or shall we let user do this?)
    public class UITextField : UIWidget
    {
        // todo : in theme / globals?
        const int MAX_GLYPHS = 1024;

        const float CARET_WIDTH = 2;
        const float CARET_BLINKING_INTERVAL = 0.5f;

        static Vector2 VECTOR_INVALID = new Vector2(-1);

        public Action<string>? TextChanged;

        // if not committed, we must (re)calculate text (using tempText) & caret pos etc
        bool _committed;

        // we use this to temporarily modify text value in edit mode
        StringBuilder _tempText = new();

        // current pointer/caret position
        int _cursorPos = -1;

        // current selection position
        int _selectionPos = -1;

        // dynamic offset for text
        float _textOffset;

        // pointer positions - used to handle selection
        Vector2 _pointerDownPos = VECTOR_INVALID;
        Vector2 _pointerDragPos = VECTOR_INVALID;

        // these are for caret blinking
        bool _showCaret = true;
        float _accumulatedTime = 0;

        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UITextField()
        {
            // set defaults to theme impl - prevents circular reference
            UnitsColor = default;
            CaretColor = default;
            SelectionColor = default;
            PlaceHolderColor = default;
            _text = string.Empty;
        }

        public UITextField(UIWidget parent)
            :this(parent, string.Empty)
        {
        
        }

        public UITextField(UIWidget parent, string text)
            : base(parent)
        {
            _committed = true;
            _text = text;
            
            _tempText.Append(text);
            
            ThemeType = typeof(UITextField);
        }

        #region Properties

        Thickness? _padding;
        public virtual Thickness Padding
        {
            get => _padding?? GetTheme().TextField.Padding;
            set => _padding = value;
        }

        public bool Editable { get; set; }
        
        string _text;
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                TextChanged?.Invoke(_text);
            }
        }

        public string DefaultText {  get; set; } = string.Empty;
        public string Units { get; set; } = string.Empty;

        // note: this is proportional line height (default is 1)
        float? _lineHeight;
        public float LineHeight
        {
            get => _lineHeight ?? GetTheme().TextField.LineHeight;
            set => _lineHeight = value;
        }

        Color? _unitsColor;
        public Color UnitsColor
        {
            get => _unitsColor?? GetTheme().TextField.UnitsColor;
            set => _unitsColor = value;
        }

        Color? _caretColor;
        public Color CaretColor
        {
            get => _caretColor?? GetTheme().TextField.CaretColor;
            set => _caretColor = value;
        }

        Color? _selectionColor;
        public Color SelectionColor
        {
            get => _selectionColor ?? GetTheme().TextField.SelectionColor;
            set => _selectionColor = value;
        }

        Color? _placeHolderColor;
        public Color PlaceHolderColor
        {
            get => _placeHolderColor?? GetTheme().TextField.PlaceHolderColor;
            set => _placeHolderColor = value;
        }

        // this should be set in extended widget, where is logic to determine validity
        // default for plain text box is true (any chars / text length are allowed)
        protected bool IsValidFormat {  get; set; } = true;

        // this is used as a hint for user what value textbox expects
        // (shown only if value = empty)
        string _placeholderText = string.Empty;
        public string PlaceholderText
        {
            get => _placeholderText;
            set
            { 
                // prevent null
                if(value != null)
                    _placeholderText = value;
            }
        }

        #endregion

        #region Methods

        // note : we don't change this text box status (committed / focus)
        public void ResetText(string text)
        {
            // prevent circular - needed?
            if (Text == text && _tempText.ToString() == text)
                return;

            // prevent null
            if(text == null)
                text = string.Empty;

            // todo : check
            // prevent NPE when Text == null
            if ((Text != null && text.Length < Text.Length) || text.Length < _tempText.Length)
            {
                _cursorPos = text.Length;// -1;
            }

            Text = text;
            _tempText.Clear().Append(text);
            
            _selectionPos = -1;

            // fire event - in text property
            //TextChanged?.Invoke(Text);
        }

        #endregion

        #region Events

        public override void OnPointerEnter(bool enter)
        {
            base.OnPointerEnter(enter);
            
            if(enter && Editable & !Disabled)
            {
                SetPointerType((int)PointerType.IBeam);
            }
        }

        public override bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
            if (button == PointerButton.Left && down && !Focused)
            {
                RequestFocus();
            }

            if (!Disabled && Editable && Focused)
            {
                if (down)
                {
                    _pointerDownPos = p;

                    // set drag widget
                    Screen?.SetDragWidget(this);
                }
                else
                {
                    _pointerDownPos = VECTOR_INVALID;
                    _pointerDragPos = VECTOR_INVALID;
                }

                return true;
            }
            
            return false;
        }

        public override bool OnPointerDoubleClick(Vector2 p, PointerButton button)
        {
            if (!Disabled && Editable && Focused)
            {
                // Double-click - select all text
                _selectionPos = 0;
                _cursorPos = _tempText.Length;
                _pointerDownPos = VECTOR_INVALID;

                return true;
            }

            return false;
        }

        // used for the selection
        public override bool OnPointerDrag(Vector2 p, Vector2 rel)
        {
            _pointerDragPos = p;

            if (Editable && Focused)
            {
                return true;
            }

            return false;
        }

        public override bool OnFocusChanged(bool focused)
        {
            base.OnFocusChanged(focused);

            if (!Disabled && Editable)
            {
                // reset caret blinking values
                _showCaret = true;
                _accumulatedTime = 0;

                if (focused)
                {
                    _tempText.Clear().Append(Text);
                    _committed = false;
                    _cursorPos = 0;

                    // begin to receive OnKeyChar evernts
                    Screen?.StartTextInput();
                }
                else
                {
                    if (IsValidFormat)
                    {
                        if (_tempText.Length == 0)
                            Text = DefaultText;
                        else
                            Text = _tempText.ToString();
                    }

                    _committed = true;
                    _cursorPos = -1;
                    _selectionPos = -1;
                    _textOffset = 0;

                    Screen?.StopTextInput();
                }
            }

            return true;
        }

        public override bool OnKeyUpDown(Key key, bool down, KeyModifiers modifiers)
        {
            if (!Focused)
                return false;

            if (Editable)
            {
                if (down)
                {
                    if (key == Key.Left)
                    {
                        if ((modifiers & KeyModifiers.Shift) != 0)
                        {
                            if (_selectionPos == -1)
                                _selectionPos = _cursorPos;
                        }
                        else
                        {
                            _selectionPos = -1;
                        }

                        if (_cursorPos > 0)
                            _cursorPos--;
                    }
                    else if (key == Key.Right)
                    {
                        if ((modifiers & KeyModifiers.Shift) != 0)
                        {
                            if (_selectionPos == -1)
                                _selectionPos = _cursorPos;
                        }
                        else
                        {
                            _selectionPos = -1;
                        }

                        if (_cursorPos < _tempText.Length)
                            _cursorPos++;
                    }
                    else if (key == Key.Home)
                    {
                        if ((modifiers & KeyModifiers.Shift) != 0)
                        {
                            if (_selectionPos == -1)
                                _selectionPos = _cursorPos;
                        }
                        else
                        {
                            _selectionPos = -1;
                        }

                        _cursorPos = 0;
                    }
                    else if (key == Key.End)
                    {
                        if ((modifiers & KeyModifiers.Shift) != 0)
                        {
                            if (_selectionPos == -1)
                                _selectionPos = _cursorPos;
                        }
                        else
                        {
                            _selectionPos = -1;
                        }

                        _cursorPos = _tempText.Length;
                    }
                    else if (key == Key.Backspace)
                    {
                        if (!DeleteSelection())
                        {
                            if (_cursorPos > 0)
                            {
                                // remove 1 char
                                _tempText.Remove(_cursorPos - 1, 1);

                                _cursorPos--;

                                TextChanged?.Invoke(_tempText.ToString());
                            }
                        }
                    }
                    else if (key == Key.Delete)
                    {
                        if (!DeleteSelection())
                        {
                            if (_cursorPos < _tempText.Length)
                            {
                                // remove 1 char
                                _tempText.Remove(_cursorPos, 1);

                                TextChanged?.Invoke(_tempText.ToString());
                            }
                        }
                    }
                    else if (key == Key.Enter)
                    {
                        // fire focus change - calls this OnFocusChanged(false)
                        // that commits & then this can get focus again
                        Parent?.RequestFocus();
                    }
                    else if (key == Key.A && (modifiers & KeyModifiers.Control) != 0)
                    {
                        _cursorPos = _tempText.Length;
                        _selectionPos = 0;
                    }
                    else if (key == Key.X && (modifiers & KeyModifiers.Control) != 0)
                    {
                        CopySelection();
                        DeleteSelection();
                    }
                    else if (key == Key.C && (modifiers & KeyModifiers.Control) != 0)
                    {
                        CopySelection();
                    }
                    else if (key == Key.V && (modifiers & KeyModifiers.Control) != 0)
                    {
                        DeleteSelection();
                        PasteFromClipboard();
                    }
                }

                return true;
            }

            return base.OnKeyUpDown(key, down, modifiers);
        }

        // todo : ahould we chacek valid format before?
        public override bool OnKeyChar(char c)
        {
            if (Editable && Focused)
            {
                DeleteSelection();

                _tempText.Insert(_cursorPos, c.ToString());
                _cursorPos++;

                TextChanged?.Invoke(_tempText.ToString());

                return true;
            }

            return false;
        }

        #endregion

        #region Layout

        public override Vector2 PreferredSize(NvgContext ctx)
        {
            ctx.FontFaceId(FontFaceId);
            ctx.FontSize(FontSize);
            ctx.TextLineHeight(LineHeight);
            // todo: check: we probably don't need to check horizontal alignment
            ctx.TextAlign(TextAlignment.Left | TextAlignment.Middle);

            Vector2 size = new Vector2(0, FontSize * LineHeight);

            float uw = 0;

            if (!string.IsNullOrEmpty(Units))
            {
                uw = ctx.TextBounds(0, 0, Units, out _);
            }

            float ts = ctx.TextBounds(0, 0, Text, out _);
            size.X = size.Y + ts + uw;

            size = Vector2.Max(MinSize, size);

            if (FixedSize.X > 0)
                size.X = FixedSize.X;

            if (FixedSize.Y > 0)
                size.Y = FixedSize.Y;

            return size;
        }

        #endregion

        #region Drawing

        public override void Draw(NvgContext ctx)
        {
            // Background
            if (Focused && !IsValidFormat)
            {
                GetTheme().Common.BackgroundInvalid?.Draw(ctx, Position, Size, null);
            }
            else
            {
                // use default
                DrawBackgroundBrush(ctx);
            }

            // Text
            ctx.FontFaceId(FontFaceId);
            ctx.FontSize(FontSize);
            ctx.TextLineHeight(LineHeight);

            Vector2 drawPos = new Vector2(Position.X, Position.Y + Size.Y * 0.5f);

            float xSpacing = Padding.Horizontal;

            float unitWidth = 0;

            // Units
            if (!string.IsNullOrEmpty(Units))
            {
                unitWidth = ctx.TextBounds(0, 0, Units, out _);

                ctx.FillColor(UnitsColor);
                ctx.TextAlign(TextAlignment.Right | TextAlignment.Middle);
                ctx.Text(Position.X + Size.X - xSpacing, drawPos.Y, Units);

                unitWidth += 2;
            }

            switch (TextHorizontalAlignment)
            {
                case TextHorizontalAlign.Left:
                    ctx.TextAlign(TextAlignment.Left | TextAlignment.Middle);
                    drawPos.X += xSpacing;
                    break;
                case TextHorizontalAlign.Right:
                    ctx.TextAlign(TextAlignment.Right | TextAlignment.Middle);
                    drawPos.X += Size.X - unitWidth - xSpacing;
                    break;
                case TextHorizontalAlign.Center:
                    ctx.TextAlign(TextAlignment.Center | TextAlignment.Middle);
                    drawPos.X += Size.X * 0.5f;
                    break;
                default:
                    break;
            }

            // Text
            
            // we must check both enabled & committed status
            ctx.FillColor(Disabled ? TextDisabledColor :
                (_committed? !string.IsNullOrEmpty(Text) : _tempText.Length > 0)? 
                TextColor : PlaceHolderColor);

            // clip visible text area
            float clipX = Position.X + xSpacing - 1.0f;
            float clipY = Position.Y + 1.0f;
            float clipWidth = Size.X - unitWidth - 2 * xSpacing + 2.0f;
            float clipHeight = Size.Y - 3.0f;

            ctx.SaveState();
            ctx.IntersectScissor(clipX, clipY, clipWidth, clipHeight);

            Vector2 oldDrawPos = drawPos;
            drawPos.X += _textOffset;

            // if not focused (no editing) - just draw
            if (_committed)
            {
                // no draw if value & placeholder empty
                if(!string.IsNullOrEmpty(Text) || !string.IsNullOrEmpty(PlaceholderText))
                {
                    ctx.Text(drawPos.X, drawPos.Y, string.IsNullOrEmpty(Text) ? PlaceholderText : Text);
                }                
            }
            else
            {
                // shall we blink caret? if not _showCaret is allways true
                if (Globals.BLINK_TEXTBOX_CARET)
                {
                    // set caret blinking values
                    _accumulatedTime += Screen.DeltaSeconds;

                    if (_accumulatedTime > CARET_BLINKING_INTERVAL)
                    {
                        // reset & change caret visibility
                        _accumulatedTime = 0;
                        _showCaret = !_showCaret;
                    }
                }

                // current editing value
                ReadOnlySpan<char> text = _tempText.ToString();

                // changes - must calculate new values
                ctx.TextBounds(drawPos.X, drawPos.Y, text, out Rect textBound);

                // we have increased font size height with height modifier 
                float lineh = Size.Y;

                // find glyphpositions
                ctx.TextGlyphPositions(drawPos, text, MAX_GLYPHS, out var glyphs);

                float maxX = textBound.X + textBound.Width;
                
                // update cusror pos
                UpdateCursor(maxX, glyphs);

                // compute text offset
                // note: we must ensure so that pointer type & pointer type text editing stays visible in text box
                if (textBound.Width + unitWidth < Size.X)
                {
                    // reset text offset (no need to set any offset)
                    _textOffset = 0;
                }
                else
                {
                    // calculate text offset
                    int prevCPos = _cursorPos > 0 ? _cursorPos - 1 : 0;
                    int nextCPos = _cursorPos < glyphs.Length ? _cursorPos + 1 : glyphs.Length;

                    float prevCX = CursorIndexToPosition(prevCPos, maxX, glyphs);
                    float nextCX = CursorIndexToPosition(nextCPos, maxX, glyphs);

                    if (nextCX > clipX + clipWidth)
                        _textOffset -= nextCX - (clipX + clipWidth) + 1;

                    if (prevCX < clipX)
                        _textOffset += clipX - prevCX + 1;
                }

                drawPos.X = oldDrawPos.X + _textOffset;

                // draw text with offset
                ctx.Text(drawPos.X, drawPos.Y, text);
                ctx.TextBounds(drawPos.X, drawPos.Y, text, out textBound);

                maxX = textBound.X + textBound.Width;

                // recompute glyph positions
                ctx.TextGlyphPositions(drawPos, text, MAX_GLYPHS, out glyphs);

                if (_cursorPos > -1)
                {
                    // Selection
                    if (_selectionPos > -1)
                    {
                        float cursorX = CursorIndexToPosition(_cursorPos, maxX, glyphs);

                        float selectionX = CursorIndexToPosition(_selectionPos, maxX, glyphs);

                        if (cursorX > selectionX)
                            MathUtils.Swap(ref cursorX, ref selectionX);

                        // draw selection
                        ctx.BeginPath();
                        ctx.FillColor(SelectionColor);
                        ctx.Rect(cursorX, drawPos.Y - lineh * 0.5f, selectionX - cursorX, lineh);
                        ctx.Fill();
                    }

                    if (_showCaret)
                    {
                        // caret x position
                        float caretX;

                        if (text.Length == 0)
                        {
                            caretX = drawPos.X;
                        }
                        else
                        {
                            caretX = CursorIndexToPosition(_cursorPos, maxX, glyphs);
                        }

                        // draw caret
                        ctx.BeginPath();
                        ctx.MoveTo(caretX, drawPos.Y - lineh * 0.5f);
                        ctx.LineTo(caretX, drawPos.Y + lineh * 0.5f);
                        ctx.StrokeColor(CaretColor);
                        ctx.StrokeWidth(CARET_WIDTH);
                        ctx.Stroke();
                    }
                }
            }

            ctx.RestoreState();

            base.Draw(ctx);

            // Border
            this.DrawBorder(ctx, true);
        }

        #endregion

        #region Private

        // copy selection into clipboard
        bool CopySelection()
        {
            if (_selectionPos > -1)
            {
                int begin = _cursorPos;
                int end = _selectionPos;

                if (begin > end)
                    MathUtils.Swap(ref begin, ref end);

                ReadOnlySpan<char> textSpan = _tempText.ToString();
                Screen?.SetClipboardString(textSpan.Slice(begin, end - begin));

                return true;
            }

            return false;
        }

        // paste from clipboard (if there is any text)
        void PasteFromClipboard()
        {
            string cbstr = Screen.GetClipboardString();

            if (!string.IsNullOrEmpty(cbstr))
            {
                _tempText.Insert(_cursorPos, cbstr);
                TextChanged?.Invoke(_tempText.ToString());
            }
        }

        // delete selection or if no selection 1 char (if not in after last char)
        bool DeleteSelection()
        {
            if (_selectionPos > -1)
            {
                int begin = _cursorPos;
                int end = _selectionPos;

                if (begin > end)
                    MathUtils.Swap(ref begin, ref end);

                if (begin == end - 1)
                    _tempText.Remove(begin, 1);
                else
                    _tempText.Remove(begin, end - begin);

                TextChanged?.Invoke(_tempText.ToString());

                _cursorPos = begin;
                _selectionPos = -1;

                return true;
            }

            return false;
        }

        void UpdateCursor(float lastX, ReadOnlySpan<GlyphPosition> glyphs)
        {
            // handle pointer type events
            if (_pointerDownPos.X != -1)
            {
                if ((Screen.KeyModifiers & KeyModifiers.Shift) != 0)
                {
                    if (_selectionPos == -1)
                        _selectionPos = _cursorPos;
                }
                else
                    _selectionPos = -1;

                _cursorPos = PositionToCursorIndex(_pointerDownPos.X, lastX, glyphs);
                _pointerDownPos = VECTOR_INVALID;
            }
            else if (_pointerDragPos.X != -1)
            {
                if (_selectionPos == -1)
                    _selectionPos = _cursorPos;

                _cursorPos = PositionToCursorIndex(_pointerDragPos.X, lastX, glyphs);
            }
            else
            {
                // set pointer to last character
                if (_cursorPos == -2)
                    _cursorPos = glyphs.Length;
            }

            if (_cursorPos == _selectionPos)
                _selectionPos = -1;
        }

        // lastX = glyphs last x
        static float CursorIndexToPosition(int index, float lastX, ReadOnlySpan<GlyphPosition> glyphs)
        {
            if (index == glyphs.Length)
                return lastX; // after last character
            else
                return glyphs[index].X;
        }

        // posX = pointer position (pointer drag, pointer down)
        // lastX = glyphs last x
        static int PositionToCursorIndex(float posX, float lastX, ReadOnlySpan<GlyphPosition> glyphs)
        {
            int cursorId = 0;
            float caretX = glyphs.Length > 0? glyphs[cursorId].X : 0;

            for (int j = 1; j < glyphs.Length; j++)
            {
                if (MathF.Abs(caretX - posX) > MathF.Abs(glyphs[j].X - posX))
                {
                    cursorId = j;
                    caretX = glyphs[cursorId].X;
                }
            }

            if (MathF.Abs(caretX - posX) > MathF.Abs(lastX - posX))
                cursorId = glyphs.Length;

            return cursorId;
        }

        #endregion
    }
}
