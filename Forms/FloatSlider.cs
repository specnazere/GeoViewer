using System.ComponentModel;

namespace LayerSlicer.Forms
{
    public class FloatSlider : Control
    {
        private float _min = 0;
        private float _max = 100;
        private float _value = 50;
        private float _fillValue = 0.5f;
        private bool _dragging = false;
        private Rectangle _fillRect;

        public event EventHandler? ValueChanged;

        public FloatSlider()
        {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer | 
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint, true);
            Height = 30;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var trackRect = new Rectangle(new Point(0, (int)(Height * (1 - _fillValue))),
                new Size(Width, (int)(Height * _fillValue)));
            e.Graphics.FillRectangle(Brushes.White, trackRect);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _dragging = true;
                UpdateValue(e.Y);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_dragging) UpdateValue(e.Y);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _dragging = false;
        }

        private void UpdateValue(float mouseY)
        {
            float newValue = _min + (1 - mouseY / Height) * (_max - _min);
            Value = Math.Max(_min, Math.Min(_max, newValue));
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float Minimum
        {
            get => _min;
            set 
            { 
                _min = value; Invalidate(); 
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float Maximum
        {
            get => _max;
            set 
            { 
                _max = value; Invalidate(); 
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    _fillValue = (value - _min) / (_max - _min);
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }
    }
}
