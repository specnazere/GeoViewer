using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace LayerSlicer
{
    internal static class GlobalModel
    {
        // Matrix parameters
        private static Vector3 _position = Vector3.Zero;
        private static Vector3 _rotation = Vector3.Zero;
        private static Vector3 _scale = Vector3.One;

        public static Buffer VerticesByTrianglesBuffer;
        public static Buffer VerticesByLinesBuffer;
        public static Buffer BoundBuffer;

        public static event EventHandler? PositionChanged;
        public static event EventHandler? RotationChanged;
        public static event EventHandler? ScaleChanged;

        public static Vector4[] CubeVerticesByTriangles = {
                new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f, -1.0f, 1.0f), // Front
                new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f, -1.0f, 1.0f),
                new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f, -1.0f, 1.0f),
                new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f, -1.0f, 1.0f),
                new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f, -1.0f, 1.0f),
                new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f, -1.0f, 1.0f),

                new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f,  1.0f, 1.0f), // Back
                new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f,  1.0f, 1.0f),
                new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f,  1.0f, 1.0f),
                new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f,  1.0f, 1.0f),
                new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f,  1.0f, 1.0f),
                new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f,  1.0f, 1.0f),

                new Vector4(-1.0f, 1.0f, -1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), new Vector4(0.0f,  1.0f, 0.0f, 1.0f), // Top
                new Vector4(-1.0f, 1.0f,  1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), new Vector4(0.0f,  1.0f, 0.0f, 1.0f),
                new Vector4( 1.0f, 1.0f,  1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), new Vector4(0.0f,  1.0f, 0.0f, 1.0f),
                new Vector4(-1.0f, 1.0f, -1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), new Vector4(0.0f,  1.0f, 0.0f, 1.0f),
                new Vector4( 1.0f, 1.0f,  1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), new Vector4(0.0f,  1.0f, 0.0f, 1.0f),
                new Vector4( 1.0f, 1.0f, -1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), new Vector4(0.0f,  1.0f, 0.0f, 1.0f),

                new Vector4(-1.0f,-1.0f, -1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), new Vector4(0.0f, -1.0f, 0.0f, 1.0f), // Bottom
                new Vector4( 1.0f,-1.0f,  1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), new Vector4(0.0f, -1.0f, 0.0f, 1.0f),
                new Vector4(-1.0f,-1.0f,  1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), new Vector4(0.0f, -1.0f, 0.0f, 1.0f),
                new Vector4(-1.0f,-1.0f, -1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), new Vector4(0.0f, -1.0f, 0.0f, 1.0f),
                new Vector4( 1.0f,-1.0f, -1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), new Vector4(0.0f, -1.0f, 0.0f, 1.0f),
                new Vector4( 1.0f,-1.0f,  1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), new Vector4(0.0f, -1.0f, 0.0f, 1.0f),

                new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector4(-1.0f, 0.0f, 0.0f, 1.0f), // Left
                new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),

                new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f), new Vector4( 1.0f, 0.0f, 0.0f, 1.0f), // Right
                new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f), new Vector4( 1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f), new Vector4( 1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f), new Vector4( 1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f), new Vector4( 1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f), new Vector4( 1.0f, 0.0f, 0.0f, 1.0f),
            };

        public static Vector4[] CubeVerticesByLines = {
                new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, -1.0f, 1.0f), // Front
                new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, -1.0f, 1.0f),

                new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, -1.0f, 1.0f),
                new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, -1.0f, 1.0f),

                new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, -1.0f, 1.0f),
                new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, -1.0f, 1.0f),

                new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, -1.0f, 1.0f),
                new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, -1.0f, 1.0f),

                new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, -1.0f, 1.0f),
                new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, -1.0f, 1.0f),


                new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f,  1.0f, 1.0f), // Back
                new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f,  1.0f, 1.0f),

                new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f,  1.0f, 1.0f),
                new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f,  1.0f, 1.0f),

                new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f,  1.0f, 1.0f),
                new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f,  1.0f, 1.0f),

                new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f,  1.0f, 1.0f),
                new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f,  1.0f, 1.0f),

                new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f,  1.0f, 1.0f),
                new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f,  1.0f, 1.0f),


                new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f,  1.0f, 0.0f, 1.0f), // Top
                new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f,  1.0f, 0.0f, 1.0f),

                new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f,  1.0f, 0.0f, 1.0f),
                new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f,  1.0f, 0.0f, 1.0f),

                new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f,  1.0f, 0.0f, 1.0f),
                new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f,  1.0f, 0.0f, 1.0f),

                new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f,  1.0f, 0.0f, 1.0f),
                new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f,  1.0f, 0.0f, 1.0f),

                new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f,  1.0f, 0.0f, 1.0f),
                new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f,  1.0f, 0.0f, 1.0f),


                new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, -1.0f, 0.0f, 1.0f), // Bottom
                new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, -1.0f, 0.0f, 1.0f),

                new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, -1.0f, 0.0f, 1.0f),
                new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, -1.0f, 0.0f, 1.0f),

                new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, -1.0f, 0.0f, 1.0f),
                new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, -1.0f, 0.0f, 1.0f),

                new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, -1.0f, 0.0f, 1.0f),
                new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, -1.0f, 0.0f, 1.0f),

                new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, -1.0f, 0.0f, 1.0f),
                new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, -1.0f, 0.0f, 1.0f),


                new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(-1.0f, 0.0f, 0.0f, 1.0f), // Left
                new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),

                new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),

                new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),

                new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),

                new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),


                new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4( 1.0f, 0.0f, 0.0f, 1.0f), // Right
                new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4( 1.0f, 0.0f, 0.0f, 1.0f),

                new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4( 1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4( 1.0f, 0.0f, 0.0f, 1.0f),

                new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4( 1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4( 1.0f, 0.0f, 0.0f, 1.0f),

                new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4( 1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4( 1.0f, 0.0f, 0.0f, 1.0f),

                new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4( 1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4( 1.0f, 0.0f, 0.0f, 1.0f),
            };

        public static Vector4[] CubeBound = {
                new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, -1.0f, 1.0f), // Front
                new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, -1.0f, 1.0f),

                new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, -1.0f, 1.0f),
                new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, -1.0f, 1.0f),

                new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, -1.0f, 1.0f),
                new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, -1.0f, 1.0f),

                new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, -1.0f, 1.0f),
                new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, -1.0f, 1.0f),


                new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f,  1.0f, 1.0f), // Back
                new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f,  1.0f, 1.0f),

                new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f,  1.0f, 1.0f),
                new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f,  1.0f, 1.0f),

                new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f,  1.0f, 1.0f),
                new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f,  1.0f, 1.0f),

                new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f,  1.0f, 1.0f),
                new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f,  1.0f, 1.0f),


                new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f,  1.0f, 0.0f, 1.0f), // Top
                new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f,  1.0f, 0.0f, 1.0f),

                new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f,  1.0f, 0.0f, 1.0f),
                new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f,  1.0f, 0.0f, 1.0f),

                new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f,  1.0f, 0.0f, 1.0f),
                new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f,  1.0f, 0.0f, 1.0f),

                new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f,  1.0f, 0.0f, 1.0f),
                new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f,  1.0f, 0.0f, 1.0f),


                new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, -1.0f, 0.0f, 1.0f), // Bottom
                new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, -1.0f, 0.0f, 1.0f),

                new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, -1.0f, 0.0f, 1.0f),
                new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, -1.0f, 0.0f, 1.0f),

                new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, -1.0f, 0.0f, 1.0f),
                new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, -1.0f, 0.0f, 1.0f),

                new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, -1.0f, 0.0f, 1.0f),
                new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, -1.0f, 0.0f, 1.0f),


                new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(-1.0f, 0.0f, 0.0f, 1.0f), // Left
                new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),

                new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),

                new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),

                new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),


                new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4( 1.0f, 0.0f, 0.0f, 1.0f), // Right
                new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4( 1.0f, 0.0f, 0.0f, 1.0f),

                new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4( 1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4( 1.0f, 0.0f, 0.0f, 1.0f),

                new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4( 1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4( 1.0f, 0.0f, 0.0f, 1.0f),

                new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4( 1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4( 1.0f, 0.0f, 0.0f, 1.0f),
            };

        public static Vector4 FrontColor
        {
            get
            {
                return CubeVerticesByTriangles[GlobalConsts.VertexColorOffset + GlobalConsts.PerSideVectors * 0];
            }
            set
            {
                for (
                    int i = GlobalConsts.VertexColorOffset + GlobalConsts.PerSideVectors * 0;
                    i < GlobalConsts.PerSideVectors + GlobalConsts.VertexColorOffset;
                    i += GlobalConsts.PerVertexVectors)
                {
                    CubeVerticesByTriangles[i] = value;
                }
                UpdateTrianglesBuffer();
            }
        }

        public static Vector4 BackColor
        {
            get
            {
                return CubeVerticesByTriangles[GlobalConsts.VertexColorOffset + GlobalConsts.PerSideVectors * 1];
            }
            set
            {
                for (
                    int i = GlobalConsts.VertexColorOffset + GlobalConsts.PerSideVectors * 1;
                    i < GlobalConsts.PerSideVectors + GlobalConsts.VertexColorOffset;
                    i += GlobalConsts.PerVertexVectors)
                {
                    CubeVerticesByTriangles[i] = value;
                }
                UpdateTrianglesBuffer();
            }
        }

        public static Vector4 TopColor
        {
            get
            {
                return CubeVerticesByTriangles[GlobalConsts.VertexColorOffset + GlobalConsts.PerSideVectors * 2];
            }
            set
            {
                for (
                    int i = GlobalConsts.VertexColorOffset + GlobalConsts.PerSideVectors * 2;
                    i < GlobalConsts.PerSideVectors + GlobalConsts.VertexColorOffset;
                    i += GlobalConsts.PerVertexVectors)
                {
                    CubeVerticesByTriangles[i] = value;
                }
                UpdateTrianglesBuffer();
            }
        }

        public static Vector4 BottomColor
        {
            get
            {
                return CubeVerticesByTriangles[GlobalConsts.VertexColorOffset + GlobalConsts.PerSideVectors * 3];
            }
            set
            {
                for (
                    int i = GlobalConsts.VertexColorOffset + GlobalConsts.PerSideVectors * 3;
                    i < GlobalConsts.PerSideVectors + GlobalConsts.VertexColorOffset;
                    i += GlobalConsts.PerVertexVectors)
                {
                    CubeVerticesByTriangles[i] = value;
                }
                UpdateTrianglesBuffer();
            }
        }

        public static Vector4 LeftColor
        {
            get
            {
                return CubeVerticesByTriangles[GlobalConsts.VertexColorOffset + GlobalConsts.PerSideVectors * 4];
            }
            set
            {
                for (
                    int i = GlobalConsts.VertexColorOffset + GlobalConsts.PerSideVectors * 4;
                    i < GlobalConsts.PerSideVectors + GlobalConsts.VertexColorOffset;
                    i += GlobalConsts.PerVertexVectors)
                {
                    CubeVerticesByTriangles[i] = value;
                }
                UpdateTrianglesBuffer();
            }
        }

        public static Vector4 RightColor
        {
            get
            {
                return CubeVerticesByTriangles[GlobalConsts.VertexColorOffset + GlobalConsts.PerSideVectors * 5];
            }
            set
            {
                for (
                    int i = GlobalConsts.VertexColorOffset + GlobalConsts.PerSideVectors * 5;
                    i < GlobalConsts.PerSideVectors + GlobalConsts.VertexColorOffset;
                    i += GlobalConsts.PerVertexVectors)
                {
                    CubeVerticesByTriangles[i] = value;
                }
                UpdateTrianglesBuffer();
            }
        }

        private static void UpdateTrianglesBuffer()
        {
            var context = Program.GetDevice().ImmediateContext;

            // Получаем DataStream для записи
            context.MapSubresource(
                VerticesByTrianglesBuffer,
                MapMode.WriteDiscard, // Перезаписываем весь буфер (быстро)
                MapFlags.None,
                out DataStream dataStream
            );

            // Записываем новые вершины
            dataStream.WriteRange(CubeVerticesByTriangles); // newVertices — массив новых вершин

            // Разблокируем буфер
            context.UnmapSubresource(VerticesByTrianglesBuffer, 0);
        }

        public static Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                PositionChanged?.Invoke(null, EventArgs.Empty);
            }
        }
        public static Vector3 Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                _rotation.Y = MathHelper.Wrap(value.Y,
                    -GlobalConsts.AngleWrapBound, GlobalConsts.AngleWrapBound);
                _rotation.X = Math.Clamp(value.X,
                    -GlobalConsts.PitchLimit, GlobalConsts.PitchLimit);
                _rotation.Z = MathHelper.Wrap(value.Z,
                    -GlobalConsts.AngleWrapBound, GlobalConsts.AngleWrapBound);
                RotationChanged?.Invoke(null, EventArgs.Empty);
            }
        }
        public static Vector3 Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                ScaleChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static Matrix Matrix
        {
            get
            { 
                return Matrix.Transformation(
                    Vector3.Zero,
                    Quaternion.Identity,
                    _scale,
                    Vector3.Zero,
                    Quaternion.RotationYawPitchRoll(_rotation.Z, _rotation.X, _rotation.Y),
                    _position);
            }
        }

        public static void Dispose()
        {
            VerticesByTrianglesBuffer.Dispose();
        }

        public static void SetPosition(float? X, float? Y, float? Z)
        {
            _position.X = X.GetValueOrDefault(_position.X); 
            _position.Y = Y.GetValueOrDefault(_position.Y); 
            _position.Z = Z.GetValueOrDefault(_position.Z);
            PositionChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void SetRotation(float? X, float? Y, float? Z)
        {
            _rotation.X = X.GetValueOrDefault(_rotation.X);
            _rotation.Y = Y.GetValueOrDefault(_rotation.Y);
            _rotation.Z = Z.GetValueOrDefault(_rotation.Z);
            RotationChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void SetScale(float? X, float? Y, float? Z)
        {
            _scale.X = X.GetValueOrDefault(_scale.X);
            _scale.Y = Y.GetValueOrDefault(_scale.Y);
            _scale.Z = Z.GetValueOrDefault(_scale.Z);
            ScaleChanged?.Invoke(null, EventArgs.Empty);
        }

        public static Matrix GetRotationMatrix()
        {
            return Matrix.RotationY(_rotation.Y) *
                Matrix.RotationX(_rotation.X) *
                Matrix.RotationZ(_rotation.Z);
        }
    }
}
