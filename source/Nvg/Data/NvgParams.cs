namespace NanoUI.Nvg.Data
{
    internal struct NvgParams
    {
        public float TessTol { get; private set; }
        public float DistTol { get; private set; }
        public float FringeWidth { get; private set; }
        public float DevicePxRatio { get; private set; }

        // nvg__setDevicePixelRatio
        public void SetDevicePixelRatio(float pixelRatio)
        {
            TessTol = 0.25f / pixelRatio;
            DistTol = 0.01f / pixelRatio;
            FringeWidth = 1.0f / pixelRatio;
            DevicePxRatio = pixelRatio;
        }
    }
}