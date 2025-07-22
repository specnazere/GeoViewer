using SharpDX;

namespace LayerSlicer
{
    internal static class GlobalCamera
    {
        // Camera values
        private static float _fov = float.Pi / 2f; // 90 degrees

        private static Vector3 _position = Vector3.Zero;
        private static Vector3 _rotation = Vector3.Zero;

        public static event EventHandler? PositionChanged;
        public static event EventHandler? RotationChanged;

        public static float Fov
        {
            get => _fov;
            set
            {
                _fov = float.Clamp(value, GlobalConsts.FovDownLimit, GlobalConsts.FovUpperLimit);
            }
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
                // Wraping and clamping angles
                _rotation.Y = MathHelper.Wrap(value.Y,
                    -GlobalConsts.AngleWrapBound, GlobalConsts.AngleWrapBound);
                _rotation.X = Math.Clamp(value.X, 
                    -GlobalConsts.PitchLimit, GlobalConsts.PitchLimit);
                _rotation.Z = MathHelper.Wrap(value.Z,
                    -GlobalConsts.AngleWrapBound, GlobalConsts.AngleWrapBound);
                RotationChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static void GlobalRotate(float dPitch, float dYaw)
        {
            dPitch = ClampPitch(dPitch);
            
            // Transform position
            Position = (Vector3)Vector3.Transform(_position, 
                Matrix.RotationY(_rotation.Y) * 
                Matrix.RotationX(-dPitch) *
                Matrix.RotationY(-(_rotation.Y + dYaw)));

            // Applying rotation
            _rotation.Y = MathHelper.Wrap(_rotation.Y + dYaw,
                -GlobalConsts.AngleWrapBound, GlobalConsts.AngleWrapBound);
            _rotation.X += dPitch;
            RotationChanged?.Invoke(null, EventArgs.Empty);
        }
        
        public static void LocalRotate(float dPitch, float dYaw, float dRoll)
        {
            dPitch = ClampPitch(dPitch);

            _rotation.Y = MathHelper.Wrap(_rotation.Y + dYaw,
                -GlobalConsts.AngleWrapBound, GlobalConsts.AngleWrapBound);
            _rotation.X += dPitch;
            _rotation.Z = MathHelper.Wrap(_rotation.Z + dRoll,
                -GlobalConsts.AngleWrapBound, GlobalConsts.AngleWrapBound);
            RotationChanged?.Invoke(null, EventArgs.Empty);
        }

        private static float ClampPitch(float dPitch)
        {
            // dPitch limiting to not overcome camera's pitch limits
            dPitch = Math.Clamp(dPitch,
               -GlobalConsts.PitchLimit - _rotation.X,
                GlobalConsts.PitchLimit - _rotation.X);
            return dPitch;
        }

        public static Matrix GetMatrix()
        {
            return Matrix.Multiply(
                Matrix.Translation(-_position) * 
                    Matrix.RotationY(_rotation.Y) * 
                    Matrix.RotationX(_rotation.X) *
                    Matrix.RotationZ(_rotation.Z), 
                Matrix.PerspectiveFovLH(_fov, 
                    Program.GetRenderSize().Width / 
                    (float)Program.GetRenderSize().Height, 0.1f, 100.0f));
        }

        public static Matrix GetRotationMatrix()
        {
            return  Matrix.RotationY(-_rotation.Y) *
                Matrix.RotationX(-_rotation.X) *
                Matrix.RotationZ(-_rotation.Z);
        }

        public static Matrix GetProjMatrix()
        {
            return Matrix.PerspectiveFovLH(_fov,
                Program.GetRenderSize().Width /
                (float)Program.GetRenderSize().Height, 0.1f, 100.0f);
        }

        internal static void GlobalShift(float X, float Y, float Z)
        {
            _position.X += X;
            _position.Y += Y;
            _position.Z += Z;

            PositionChanged?.Invoke(null, EventArgs.Empty);
        }

        internal static void Shift(Vector3 dPos)
        {
            // Shift modyfing
            var rotatedShift = Vector3.Transform(dPos,
                Matrix.RotationX(-_rotation.X) * 
                Matrix.RotationY(-_rotation.Y));

            // Applying shift
            _position.X += rotatedShift.X;
            _position.Y += rotatedShift.Y;
            _position.Z += rotatedShift.Z;
            PositionChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
