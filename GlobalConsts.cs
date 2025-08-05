using SharpDX;
using System.Runtime.InteropServices;

namespace LayerSlicer
{
    internal static class GlobalConsts
    {
        public const float DegreesToRadians = float.Pi / 180;

        public const float PitchLimit = float.Pi / 2;
        public const float AngleWrapBound = float.Pi;

        public const float FovUpperLimit = float.Pi / 1.5f; // 135 degrees
        public const float FovDownLimit = float.Pi / 18f; // 10 degrees
        public const float FovBaseValue = float.Pi / 2; // 90 degrees
        public const double FovSensitivity = 0.9; // Value in range (0, 1)

        public const float StandartMouseWheelStep = 120;

        public const float MouseMoveSensitivity = 0.03f;
        public const float ModelRotateSensitivity = 30f;
        public const float MouseRotateSensitivity = 0.2f;

        public const float CameraSpeed = 2.5f;

        public const int StandartWidth = 900;
        public const int StandartHeight = 800;

        public const int InteractFormWidth = 260;
        public const int ButtonHeight = 25;
        public const int Padding = 2;
        public const int LayerStepWidth = 5;
        public const int BorderWidth = 1;
        public const int ScrollBarWidth = 30;

        public static readonly Font BasicFont = new("Consolas", 12);

        public static readonly Pen WhiteBorderPen = new(System.Drawing.Color.White, 1);
        public static readonly Pen GrayBorderPen = new(System.Drawing.Color.Gray, 1);

        public const int ScrollingSensitivity = -15; // Negative to invert direction

        public static readonly Vector3 StandartCameraPostion = new(0, 0, -5);

        // Crutch to remove
        public static readonly Vector2 Windows10MainFormPosition = new(8f, 31f);

        public const int MaxLightsPerType = 32;

        public const int Float4BytesSize = 16;
        public const int Float4x4BytesSize = Float4BytesSize * 4;

        public static readonly Vector4 BaseAmbiendLight = new(0.2f, 0.2f, 0.2f, 1);

        public const int BasicToonShaderSteps = 8;

        public const int PerVertexVectors = 3;
        public const int PerTriangleVertices = 3;
        public const int PerLineVertices = 2;
        public const int PerSideTriangles = 2;
        public const int PerSideVectors = PerVertexVectors * PerTriangleVertices * PerSideTriangles;
        public const int VertexPositionOffset = 0;
        public const int VertexColorOffset = 1;
        public const int VertexNormalOffset = 2;

        public const string SizeRegex = @"([0-9]+) x ([0-9]+)";
        public const string FloatRegex = @"-?[0-9]+.-?[0-9]+";

        public enum PixelShaderType
        {
            Toon, Phong, Mesh, Strict
        }

        public enum OverlapType
        {
            None, Full, External
        }

        public enum DataType
        {
            Grid, Surface
        }

        public enum VertexShaderType
        {
            ExternalColorOverlap, HeightColorOverlap
        }
    }
}
