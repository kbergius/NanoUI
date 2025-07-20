namespace NanoUI.Fonts.Data
{
    // note: we use same iterator whole time, so we must reset values
    internal struct FontTextIter
    {
        public float X, Y, NextX, NextY;
        public uint Codepoint;
        public int PrevGlyphIndex;
        
        // current pos (char) in string
        public int CurrentPos;
        // next char pos
        public int NextPos;
        
        public FontGlyphBitmap BitmapOption;

        public void Reset(float x, float y, FontGlyphBitmap bitmapOption)
        {
            X = NextX = x;
            Y = NextY = y;
            CurrentPos = NextPos = 0;
            Codepoint = 0;
            PrevGlyphIndex = -1;
            BitmapOption = bitmapOption;
        }
    }
}