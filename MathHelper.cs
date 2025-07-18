namespace LayerSlicer
{
    internal static class MathHelper
    {
        public static float Wrap(float value, float min, float max)
        {
            float amplitude = max - min;
            while (value < min)
            {
                value += amplitude;
            }
            while (value > max)
            {
                value -= amplitude;
            }
            return value;
        }
    }
}
