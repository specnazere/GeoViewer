using System.Reflection.Metadata.Ecma335;

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
        public const float MouseRotateSensitivity = 0.75f;

        public const float CameraSpeed = 2.5f;

        public const int StandartWidth = 900;
        public const int StandartHeight = 600;

        public const int InteractFormWidth = 250;
        public const int ButtonHeight = 25;
        public const int Padding = 2;
        public const int LayerStepWidth = 5;
        public const int BorderWidth = 1;

        public static readonly Font BasicFont = new Font("Consolas", 12);
    }
}
