using System.ComponentModel;

namespace LayerSlicer.Forms
{
    internal class SliderWrap : Control
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public BrushSlider Slider { get; private set; }

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

        private bool _integer = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Integer
        {
            get => Slider.Integer;
            set => Slider.Integer = value;
        }

        public event EventHandler ValueChanged;

        public SliderWrap()
        {
            // Initialize components
            Slider = new BrushSlider
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
                if (Integer)
                {
                    if (int.TryParse(HandInput.Text, out int value))
                    {
                        Slider.Value = Math.Clamp(value, Slider.Minimum, Slider.Maximum);
                    }
                    else
                    {
                        HandInput.Text = Slider.Value.ToString();
                    }
                }
                else
                {
                    if (float.TryParse(HandInput.Text, out float value))
                    {
                        Slider.Value = Math.Clamp(value, Slider.Minimum, Slider.Maximum);
                    }
                    else
                    {
                        HandInput.Text = Slider.Value.ToString();
                    }
                }
                isUpdating = false;
            };

            HandInput.GotFocus += (s, e) =>
            {
                UserEventRouter.IsTexting = true;
            };
            HandInput.LostFocus += (s, e) =>
            {
                UserEventRouter.IsTexting = false;
            };
            HandInput.KeyUp += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
                {
                    MainWindow.GetRenderForm().ActiveControl = null;
                }
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