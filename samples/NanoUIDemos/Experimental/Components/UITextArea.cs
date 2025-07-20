using NanoUI.Common;
using Color = NanoUI.Common.Color;
using NanoUI.Components;
using NanoUI.Components.Scrolling;
using NanoUI.Nvg;
using NanoUI.Utils;
using System.Numerics;
using System.Text;

namespace NanoUIDemos.Experimental.Components
{
    // this is barebones text lines display widget.
    // limitations:
    // - 1 font, font size, color
    // - only plain text
    // todo widget Padding
    // todo: add rich text support with (limited) markdown syntax & parser (changes in parsing, layouting & drawing etc)
    public class UITextArea : UIWidget
    {
        const char NEW_LINE = '\n';
        const int MAX_GLYPHS_IN_LINE = 1024;
        const float CARET_WIDTH = 5f;

        public struct Block
        {
            public Vector2 Offset;
            // note: this could be size (Vector2) to support different font sizes
            public int Width;
            public string Text;
        }

        List<Block> _blocks = new();
        
        Vector2 _offset;
        Vector2 _maxSize;

        BlockPosition _selectionStart;
        BlockPosition _selectionEnd;

        // we show caret only when pointer button down (selection/dragging in process)
        bool _showCaret;

        struct BlockPosition
        {
            // block index in blocks list
            public int Index;
            // char index in block
            public int CharIndex;

            public BlockPosition(int val)
            {
                Index = CharIndex = val;
            }

            public BlockPosition(int index, int charIndex)
            {
                Index = index;
                CharIndex = charIndex;
            }

            public void Clear()
            {
                Index = -1;
                CharIndex = -1;
            }

            public bool IsValid() => Index >= 0 && CharIndex >= 0;
        }

        public UITextArea() { }
        public UITextArea(UIWidget parent)
            :base(parent)
        {
            _offset = Vector2.Zero;
            _maxSize = Vector2.Zero;
            
            ThemeType = typeof(UITextField);

            // we init selection with "no selection"
            ClearSelection();
        }

        #region Properties

        public Color SelectionColor { get; set; }
        public Color CaretColor { get; set; }
        public int Padding { get; set; }
        public bool Selectable { get; set; }

        public bool HasSelection
        {
            get
            {
                return _selectionStart.IsValid() && _selectionEnd.IsValid() &&
                    (_selectionStart.Index != _selectionEnd.Index || _selectionStart.CharIndex != _selectionEnd.CharIndex);
            }
        }

        #endregion

        #region Methods

        public void AppendLine(string text)
        {
            Append(text + NEW_LINE);
        }

        public void Append(string text)
        {
            NvgContext ctx = NvgContext.Instance;

            ctx.FontSize(FontSize);
            ctx.FontFaceId(FontFaceId);

            ReadOnlySpan<char> chars = new ReadOnlySpan<char>(text.ToCharArray());
            int index = 0;

            do
            {
                int begin = index;

                while (index < chars.Length - 1 && chars[index] != NEW_LINE)
                    index++;

                var line = chars.Slice(begin, index - begin);

                if (line.IsEmpty)
                    continue;
                
                int width = (int)ctx.TextBounds(new Vector2(0, 0), line, out _);
                _blocks.Add(new Block { Offset = _offset, Width = width, Text = line.ToString() });

                _offset.X += width;
                _maxSize = Vector2.Max(_maxSize, _offset);

                if (chars[index] == NEW_LINE)
                {
                    _offset = new Vector2(0, _offset.Y + FontSize);
                    _maxSize = Vector2.Max(_maxSize, _offset);
                }
            } while (index++ < chars.Length);

            // perform layout in scroll, that calls perform layout here
            if (Parent is UIScrollPanel scroll)
                scroll.PerformLayout(ctx);
        }

        public void Clear()
        {
            _blocks.Clear();

            _offset = _maxSize = Vector2.Zero;

            ClearSelection();
        }

        public void ClearSelection()
        {
            _selectionStart.Clear();
            _selectionEnd.Clear();
        }

        #endregion

        #region Layout

        public override Vector2 PreferredSize(NvgContext ctx)
        {
            // max size is (re)calculated when we append text
            return _maxSize + new Vector2(Padding * 2);
        }

        #endregion

        #region Events

        public override bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
            _showCaret = down && Selectable;

            if (down && button == PointerButton.Left && Selectable)
            {
                // reset selection
                _selectionStart = _selectionEnd =
                    PositionToBlock(p - Position - new Vector2(Padding));

                RequestFocus();

                // set drag widget
                Screen?.SetDragWidget(this);
                
                return true;
            }
            else if (!down && button == PointerButton.Left && Selectable)
            {
                // if there is no real selection (no drag)
                // we must clear the selection in case user clicked (clears selection)
                // note: needed for scrollbars to work
                // todo: should we also clear selection, when focus lost?
                if(!HasSelection)
                {
                    ClearSelection();
                }
            }

            return false;
        }

        // todo: must handle also scrolling, if pointer moves outside visible area
        // todo: should we also clear selection, if not focused anymore?
        public override bool OnPointerDrag(Vector2 p, Vector2 rel)
        {
            // check if selection inited
            if (Selectable && _selectionStart.IsValid())
            {
                _selectionEnd = PositionToBlock(p - Position - new Vector2(Padding));
                
                return true;
            }

            return false;
        }

        public override bool OnKeyUpDown(Key key, bool down, KeyModifiers modifiers)
        {
            if (Selectable && Focused)
            {
                // copy to clipboard
                if (key == Key.C && (modifiers & KeyModifiers.Control) != 0 && down && HasSelection)
                {
                    BlockPosition start = _selectionStart;
                    BlockPosition end = _selectionEnd;

                    if (start.Index > end.Index || (start.Index == end.Index && start.CharIndex > end.CharIndex))
                        MathUtils.Swap(ref start, ref end);

                    // collect clipboard string
                    StringBuilder result = new StringBuilder();

                    for (int i = start.Index; i <= end.Index; i++)
                    {
                        // line changes
                        if (i > start.Index && _blocks[i].Offset.Y != _blocks[i - 1].Offset.Y)
                            result.Append(NEW_LINE);

                        Block block = _blocks[i];

                        // check which part of the block text to append
                        if (i == start.Index && i == end.Index)
                        {
                            int blocksStart = start.CharIndex;
                            int length = end.CharIndex - blocksStart;

                            result.Append(block.Text.Substring(blocksStart, length));
                        }
                        else if (i == start.Index)
                        {
                            int blocksStart = start.CharIndex;
                            int length = block.Text.Length - 1 - blocksStart;

                            result.Append(block.Text.Substring(blocksStart, length));
                        }
                        else if (i == end.Index)
                        {
                            int blocksStart = 0;
                            int length = end.CharIndex - blocksStart;

                            result.Append(block.Text.Substring(blocksStart, length));
                        }
                        else
                        {
                            result.Append(_blocks[i].Text);
                        }
                    }

                    // set result to clipboard
                    Screen?.SetClipboardString(result.ToString());

                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Drawing

        public override void Draw(NvgContext ctx)
        {
            // calculate blocks start & end index (do not issue draw commands for invisible blocks)
            int blocksStart = 0;
            int blocksEnd = _blocks.Count - 1;

            if (Parent is UIScrollPanel scroll)
            {
                int windowOffset = (int)-Position.Y;
                int windowSize = (int)scroll.Size.Y;

                // reset
                blocksStart = int.MaxValue;
                blocksEnd = int.MinValue;

                for (int i = 0; i < _blocks.Count; i++)
                {
                    Block block = _blocks[i];

                    if (block.Offset.Y + FontSize > windowOffset)
                    {
                        // get the minimum visible block index
                        blocksStart = Math.Min(blocksStart, i);
                    }

                    if (windowOffset + windowSize > block.Offset.Y)
                    {
                        // get the maximum visible block index
                        blocksEnd = Math.Max(blocksEnd, i);
                    }
                }

                // Validity checks
                if (blocksStart >= _blocks.Count)
                    blocksStart = 0;

                if (blocksEnd < 0)
                    blocksEnd = _blocks.Count - 1;
            }

            // Background
            DrawBackgroundBrush(ctx);

            // do we have selection
            // note: we don't use HasSelection property since it checks also that _selectionStart != _selectionEnd
            bool hasSelection = _selectionStart.IsValid() && _selectionEnd.IsValid();

            // Caret
            Vector2 caretEnd = BlockToPosition(ctx, _selectionEnd);
            caretEnd += Position + new Vector2(Padding);

            // caret uses selection end, so we just check it
            if (_showCaret && hasSelection)
            {
                ctx.BeginPath();
                ctx.MoveTo(caretEnd.X, caretEnd.Y);
                ctx.LineTo(caretEnd.X, caretEnd.Y + FontSize);
                ctx.StrokeColor(CaretColor);
                ctx.StrokeWidth(CARET_WIDTH);
                ctx.Stroke();
            }

            Vector2 caretStart = BlockToPosition(ctx, _selectionStart);
            caretStart += Position + new Vector2(Padding);
            bool flip = false;

            if (caretStart.Y > caretEnd.Y ||
                (caretStart.Y == caretEnd.Y && caretStart.X > caretEnd.X))
            {
                MathUtils.Swap(ref caretStart, ref caretEnd);
                flip = true;
            }

            // draw selection for selecion start & end when selection deals with partial block selection
            if (hasSelection)
            {
                ctx.BeginPath();
                ctx.FillColor(SelectionColor);

                if (caretEnd.Y == caretStart.Y)
                {
                    ctx.Rect(caretStart.X, caretStart.Y,
                            caretEnd.X - caretStart.X,
                            FontSize);
                }
                else
                {
                    ctx.Rect(caretStart.X, caretStart.Y,
                            _blocks[flip ? _selectionEnd.Index : _selectionStart.Index].Width -
                            (caretStart.X - Position.X - Padding),
                            FontSize);

                    ctx.Rect(Position.X + Padding, caretEnd.Y,
                            caretEnd.X - Position.X - Padding, FontSize);
                }

                ctx.Fill();
            }

            // draw blocks
            ctx.FontFaceId(FontFaceId);
            ctx.FontSize(FontSize);
            ctx.TextAlign(TextAlignment.Left | TextAlignment.Top);

            // note: blocksEnd is last index
            for (var i = blocksStart; i <= blocksEnd; i++)
            {
                Block block = _blocks[i];
               
                Vector2 offset = block.Offset + Position + new Vector2(Padding);

                // this draws selection for whole block
                if (hasSelection && offset.Y > caretStart.Y && offset.Y < caretEnd.Y)
                {
                    ctx.FillColor(SelectionColor);
                    ctx.BeginPath();
                    ctx.Rect(offset.X, offset.Y, block.Width, FontSize);
                    ctx.Fill();
                }

                ctx.FillColor(TextColor);
                ctx.Text(offset.X, offset.Y, block.Text);
            }
        }

        #endregion

        #region Private

        BlockPosition PositionToBlock(Vector2 pos)
        {
            NvgContext ctx = NvgContext.Instance;
            
            int index = int.MaxValue;
            
            // get the start index where to start looking
            for(int i = 0; i < _blocks.Count; i++)
            {
                Block block = _blocks[i];

                if (block.Offset.Y + FontSize > pos.Y)
                {
                    index = Math.Min(index, i);
                }
            }

            int charIndex = 0;

            if (index == int.MaxValue)
            {
                // there is no blocks?
                if (_blocks.Count == 0)
                    return new BlockPosition(-1);

                // we use last block
                index = _blocks.Count - 1;
                Block block = _blocks[index];

                ctx.TextGlyphPositions(block.Offset.X, block.Offset.Y, block.Text, MAX_GLYPHS_IN_LINE, out var glyphs);
                
                // after last char
                charIndex = glyphs.Length;
            }
            else
            {
                // special handling for first
                float firstOffsetY = _blocks[index].Offset.Y;

                for (var i = index; i < _blocks.Count && _blocks[i].Offset.Y == firstOffsetY; i++)
                {
                    Block block = _blocks[i];

                    ctx.FontSize(FontSize);
                    ctx.FontFaceId(FontFaceId);
                    
                    // get glyphs
                    ctx.TextGlyphPositions(block.Offset.X, block.Offset.Y, block.Text, MAX_GLYPHS_IN_LINE, out var glyphs);
                    
                    // get char index
                    for (int j = 0; j < glyphs.Length; j++)
                    {
                        if (glyphs[j].MinX + glyphs[j].MaxX < pos.X * 2)
                            charIndex = j + 1;
                    }
                }
            }

            return new BlockPosition(index, charIndex);
        }

        Vector2 BlockToPosition(NvgContext ctx, BlockPosition blockPos)
        {
            if (blockPos.Index < 0 || blockPos.Index >= _blocks.Count)
                return new Vector2(-1);
            
            // get the block
            Block block = _blocks[blockPos.Index];
            
            ctx.FontSize(FontSize);
            ctx.FontFaceId(FontFaceId);
            
            // get glyph positions
            ctx.TextGlyphPositions(block.Offset.X, block.Offset.Y, block.Text, MAX_GLYPHS_IN_LINE, out var glyphs);

            int glyphCount = glyphs.Length;

            // check invalid char index
            if (blockPos.CharIndex == glyphCount)
                return block.Offset + new Vector2(glyphs[blockPos.CharIndex - 1].MaxX + 1, 0);
            else if (blockPos.CharIndex > glyphCount)
                return new Vector2(-1);

            return block.Offset + new Vector2(glyphs[blockPos.CharIndex].X, 0);
        }

        #endregion
    }
}