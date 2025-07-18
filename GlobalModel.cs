using SharpDX;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace LayerSlicer
{
    internal static class GlobalModel
    {
        private static Vector3 _position = Vector3.Zero;
        private static Vector3 _rotation = Vector3.Zero;
        private static Vector3 _scale = Vector3.One;

        public static Buffer VerticesBuffer;

        public static event EventHandler? PositionChanged;
        public static event EventHandler? RotationChanged;
        public static event EventHandler? ScaleChanged;

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
            VerticesBuffer.Dispose();
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
    }
}
