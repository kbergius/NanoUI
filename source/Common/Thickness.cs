namespace NanoUI.Common
{
    // used for margins, paddings etc
    // note: this could be a Vector2, but for possible future use could be extended
    // (separate left/right & top/bottom values)
    public struct Thickness
    {
        public float Horizontal { get; set; }
        public float Vertical { get; set; }

        public Thickness(float val)
        {
            Horizontal = Vertical = val;
        }

        public Thickness(float horizontal, float vertical)
        {
            Horizontal = horizontal;
            Vertical = vertical;
        }

        public float this[int index]
        {
            get => index == 1? Vertical : Horizontal;
            set
            {
                if(index == 1)
                    Vertical = value;
                else 
                    Horizontal = value;
            }
        }
    }
}