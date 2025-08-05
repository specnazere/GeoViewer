using SharpDX;

namespace LayerSlicer.Lighting.Structs
{
    internal class DirectionalLightObject
    {
        private Vector4 _direction;
        private Vector4 _color;

        public Vector4 Direction
        {
            get => _direction;
            set
            {
                _direction = value;
                DataChanged?.Invoke(null, EventArgs.Empty);
            }
        }
        public Vector4 Color
        {
            get => _color;
            set
            {
                _color = value;
                DataChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public event EventHandler? DataChanged;

        public DirectionalLightStruct AsStruct()
        {
            return new DirectionalLightStruct { Direction = Direction, Color = Color };
        }
    }

    internal struct DirectionalLightStruct
    {
        public Vector4 Direction;
        public Vector4 Color;
    }
}
