using SharpDX;

namespace LayerSlicer.Lighting.Structs
{
    internal class PointLightObject
    {
        private Vector4 _position;
        private Vector4 _color;

        public Vector4 Position 
        { 
            get => _position;
            set
            {
                _position = value;
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

        public PointLightStruct AsStruct()
        {
            return new PointLightStruct { Position = Position, Color = Color };
        }
    }

    internal struct PointLightStruct
    {
        public Vector4 Position;
        public Vector4 Color;
    }
}
