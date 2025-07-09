using SharpDX;

namespace LayerSlicer
{
    internal static class GlobalCamera
    {
        private static Vector3 _position = Vector3.Zero;
        private static Vector3 _rotation = Vector3.Zero;
        private static float _fov = float.Pi / 2f; // 90 degrees

        private static Matrix _positionMatrix = Matrix.Identity;
        private static Matrix _rotationMatrix = Matrix.Identity;
        private static Matrix _lookAtMatrix = Matrix.Identity;
        private static Matrix _perspectiveMatrix = Matrix.Identity;
        private static Matrix _projMatrix = Matrix.Identity;

        static GlobalCamera()
        {
            UpdatePerspectiveMatrix();
            UpdatePositionMatrix();
            UpdateRotationMatrix();
        }

        public static Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                UpdatePositionMatrix();
            }
        }

        public static Vector3 Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                UpdateRotationMatrix();
            }
        }

        public static float Fov
        {
            get => _fov;
            set
            {
                _fov = float.Clamp(value, GlobalConsts.FovDownLimit, GlobalConsts.FovUpperLimit);
                UpdatePerspectiveMatrix();
            }
        }

        public static void UpdateProjectionMatrix()
        {
            _projMatrix = Matrix.Multiply(_lookAtMatrix, _perspectiveMatrix);
        }

        public static void UpdatePerspectiveMatrix()
        {
            _perspectiveMatrix = Matrix.PerspectiveFovLH(_fov, Program.GetRenderSize().Width / (float)Program.GetRenderSize().Height, 0.1f, 100.0f);
            UpdateProjectionMatrix();
        }

        public static void UpdateLookAtMatrix()
        {
            _lookAtMatrix = _positionMatrix * _rotationMatrix;
            UpdateProjectionMatrix();
        }

        public static void UpdateRotationMatrix()
        {
            _rotationMatrix = Matrix.RotationY(_rotation.Y) * Matrix.RotationX(_rotation.X);
            UpdateLookAtMatrix();
        }

        public static void UpdatePositionMatrix()
        {
            _positionMatrix = Matrix.Translation(-_position);
            UpdateLookAtMatrix();
        }

        public static void GlobalRotate(float Pitch, float Yaw)
        {
            // PitchLimiting
            if (_rotation.X + Pitch > GlobalConsts.PitchLimit)
            {
                Pitch = GlobalConsts.PitchLimit - _rotation.X;
            }
            else if (_rotation.X + Pitch < -GlobalConsts.PitchLimit)
            {
                Pitch = GlobalConsts.PitchLimit + _rotation.X;
            }
            
            // Transform position
            var NewPosition = Vector3.Transform(_position, 
                Matrix.RotationY(_rotation.Y) * 
                Matrix.RotationX(-Pitch) *
                Matrix.RotationY(-(_rotation.Y + Yaw)));

            // Applying position
            _position.X = NewPosition.X;
            _position.Y = NewPosition.Y;
            _position.Z = NewPosition.Z;

            // Applying rotation
            _rotation.Y += Yaw;
            _rotation.X += Pitch;
            
            // Updating matrices
            UpdateRotationMatrix();
            UpdatePositionMatrix();
        }

        public static Matrix GetMatrix()
        {
            return _projMatrix;
        }
    }
}
