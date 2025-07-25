using System.ComponentModel;

namespace LayerSlicer.Forms
{
    public class FloatSlider : Control
    {
        // Slider values
        private float _min = 0;
        private float _max = 100;
        private float _value = 50;
        private float _resetValue = 50;

        // Render values
        private float _fillValue = 0.5f;
        private float _postResetFillValue = 0.5f;

        // Flags
        private bool _dragging = false;

        public bool Horisontal = false;

        public event EventHandler? ValueChanged;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float Minimum
        {
            get => _min;
            set
            {
                _min = value;
                Value = _value;
                SetReset = _resetValue;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float Maximum
        {
            get => _max;
            set
            {
                _max = value;
                Value = _value;
                SetReset = _resetValue;
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float SetReset
        {
            set
            {
                _resetValue = value;
                _postResetFillValue = (_resetValue - _min) / (_max - _min);
            }
        }

        public FloatSlider()
        {
            // Base setup
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer | 
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint, true);

            // Placeholder value. don't matter, just must not be zero
            Height = 30;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Horisontal)
            {
                var trackRect = new Rectangle(
                    new Point(0, 0),
                    new Size((int)(Width * _fillValue), Height));
                e.Graphics.FillRectangle(Brushes.White, trackRect);

                // Drawing value on reset indicator
                int resetValuePosition = (int)(Width * _postResetFillValue);

                e.Graphics.DrawLine(Pens.Black,
                    resetValuePosition - 1, 0,
                    resetValuePosition - 1, Height);
                e.Graphics.DrawLine(Pens.Red,
                    resetValuePosition, 0,
                    resetValuePosition, Height);
                e.Graphics.DrawLine(Pens.Black,
                    resetValuePosition + 1, 0,
                    resetValuePosition + 1, Height);

                // Minimal value drawing
                e.Graphics.DrawString(_min.ToString(), GlobalConsts.BasicFont, Brushes.Black,
                    0, 0,
                    new StringFormat()
                    {
                        Alignment = StringAlignment.Near,
                    });

                // Maximal value drawing
                e.Graphics.DrawString(_max.ToString(), GlobalConsts.BasicFont, Brushes.White,
                    Width, 0,
                    new StringFormat()
                    {
                        Alignment = StringAlignment.Far,
                    });
            }
            else
            {
                var trackRect = new Rectangle(
                    new Point(0, (int)(Height * (1 - _fillValue))),
                    new Size(Width, (int)(Height * _fillValue) - 1));
                e.Graphics.FillRectangle(Brushes.White, trackRect);

                // Drawing value on reset indicator
                int resetValuePosition = (int)(Height * (1 - _postResetFillValue));

                e.Graphics.DrawLine(Pens.Black,
                    0, resetValuePosition - 1,
                    Width, resetValuePosition - 1);
                e.Graphics.DrawLine(Pens.Red,
                    0, resetValuePosition,
                    Width, resetValuePosition);
                e.Graphics.DrawLine(Pens.Black,
                    0, resetValuePosition + 1,
                    Width, resetValuePosition + 1);

                // Minimal value drawing
                e.Graphics.DrawString(_min.ToString(), GlobalConsts.BasicFont, Brushes.Black,
                    Width / 2, Height - GlobalConsts.BasicFont.Height,
                    new StringFormat()
                    {
                        Alignment = StringAlignment.Center,
                    });

                // Maximal value drawing
                e.Graphics.DrawString(_max.ToString(), GlobalConsts.BasicFont, Brushes.White,
                    Width / 2, 0,
                    new StringFormat()
                    {
                        Alignment = StringAlignment.Center,
                    });
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _dragging = true;
                UpdateValue(e);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_dragging)
            {
                UpdateValue(e);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _dragging = false;
        }

        private void UpdateValue(MouseEventArgs e)
        {
            if (Horisontal)
            {
                float newValue = e.X / (float)Width * (_max - _min) + _min;
                Value = Math.Max(_min, Math.Min(_max, newValue));
            }
            else
            {
                float newValue = (1 - e.Y / (float)Height) * (_max - _min) + _min;
                Value = Math.Max(_min, Math.Min(_max, newValue));
            }
        }

        public void Reset()
        {
            Value = _resetValue;
        }
    }
}
