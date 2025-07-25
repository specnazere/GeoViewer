using System.ComponentModel;

namespace LayerSlicer.Forms
{
    internal class SliderWrap : Control
    {

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FloatSlider Slider { get; private set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TextBox HandInput { get; private set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Button ResetButton { get; private set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float Value 
        {
            get => Slider.Value;
            set => Slider.Value = value;
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float SetReset
        {
            set => Slider.SetReset = value;
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float Minimum
        {
            get => Slider.Minimum;
            set => Slider.Minimum = value;
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float Maximum
        {
            get => Slider.Maximum;
            set => Slider.Maximum = value;
        }

        private bool _isVerticalLayout = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsVerticalLayout 
        { 
            get => _isVerticalLayout;
            set
            {
                _isVerticalLayout = value;
                OnSizeChanged();
            }
        }

        public event EventHandler ValueChanged;

        public SliderWrap()
        {
            // Initialize components
            Slider = new FloatSlider
            {
                Minimum = -100,
                Maximum = 100,
                Value = 0,
                Dock = DockStyle.None
            };
            Slider.ValueChanged += (sender, e) => ValueChanged?.Invoke(sender, e);

            HandInput = new TextBox
            {
                TextAlign = HorizontalAlignment.Center,
                Font = GlobalConsts.BasicFont,
                Text = "0",
                Dock = DockStyle.None,
            };

            ResetButton = new Button
            {
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Text = "RESET",
                Font = GlobalConsts.BasicFont,
                Dock = DockStyle.None,
                Height = HandInput.Height,
            };

            /*
            // Setup layout
            if (IsVerticalLayout)
            {
                Slider.Height = 70;
                HandInput.Height = 20;
                ResetButton.Height = 30;
            }
            else
            {
                Slider.Width = 70;
                HandInput.Width = 50;
                ResetButton.Width = 60;
            }
            */

            // Add controls
            Controls.Add(Slider);
            Controls.Add(HandInput);
            Controls.Add(ResetButton);

            // Event handlers
            bool isUpdating = false;

            Slider.ValueChanged += (s, e) =>
            {
                if (isUpdating) return;

                isUpdating = true;
                HandInput.Text = Slider.Value.ToString();
                isUpdating = false;
            };

            HandInput.TextChanged += (s, e) =>
            {
                if (isUpdating) return;

                isUpdating = true;
                if (float.TryParse(HandInput.Text, out float value))
                {
                    Slider.Value = Math.Clamp(value, Slider.Minimum, Slider.Maximum);
                }
                else
                {
                    HandInput.Text = Slider.Value.ToString();
                }
                isUpdating = false;
            };

            ResetButton.Click += (s, e) => Slider.Reset();
            SizeChanged += (s, e) => OnSizeChanged();

            Size = new Size(100, 100);
        }

        public void Reset()
        {
            Slider.Reset();
        }

        public void OnSizeChanged()
        {
            if (_isVerticalLayout)
            {
                Slider.Horisontal = false;
                ResetButton.Width = Width;
                HandInput.Width = Width;
                Slider.Width = Width;

                Slider.Height = Height - HandInput.Height - ResetButton.Height - Padding.Vertical;
                Slider.Location = new Point(0, 0);
                HandInput.Location = new Point(0, Height - ResetButton.Height);
                ResetButton.Location = new Point(0, Height - ResetButton.Height - ResetButton.Height);
            }
            else
            {
                Slider.Horisontal = true;
                Height = ResetButton.Height * 2 + Padding.Vertical;
                ResetButton.Width = Width / 2;
                HandInput.Width = Width / 2;
                Slider.Width = Width;

                Slider.Height = HandInput.Height - Padding.Vertical;
                Slider.Location = new Point(0, 0);
                HandInput.Location = new Point(0, Height / 2);
                ResetButton.Location = new Point(Width / 2, Height / 2);
            }
        }
    }
}