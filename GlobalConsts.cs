namespace LayerSlicer
{
    internal static class GlobalConsts
    {
        public const float DegreesToRadians = float.Pi / 180;

        public const float PitchLimit = float.Pi / 2;

        public const float FovUpperLimit = float.Pi / 1.5f; // 135 degrees
        public const float FovDownLimit = float.Pi / 18f; // 10 degrees
        public const double FovSensitivity = 0.9; // Value in range (0, 1)

        public const float StandartMouseWheelStep = 120;

        public const float MouseMoveSensitivity = 0.03f;
    }
}
