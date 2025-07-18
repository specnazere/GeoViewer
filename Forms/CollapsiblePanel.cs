namespace LayerSlicer.Forms
{
    public class CollapsiblePanel : Panel
    {
        protected string _title;
        protected Panel _content;
        protected bool _isCollapsed;
        protected Label _header;
        protected Button _button;

        protected bool _selfSizeChanging = false;

        protected int _lastInnerWidth = 0;

        public event EventHandler? ContainmentHeightChange;

        public CollapsiblePanel(string title)
        {
            _title = title;
            Height = GlobalConsts.ButtonHeight;
            Padding = Padding.Empty;
            Margin = Padding.Empty;

            _button = new HeaderButton();
            _button.FlatAppearance.BorderSize = 0;
            _button.Click += (s, e) => Toggle();

            // Header button setup
            _header = new Label
            {
                TextAlign = ContentAlignment.MiddleLeft,
                Text = $" {title}",
                Location = new Point(_button.Width + GlobalConsts.BorderWidth - 1, 0),
                Height = GlobalConsts.ButtonHeight,
                Width = Width - _button.Width - GlobalConsts.BorderWidth,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.Black,
                ForeColor = Color.White,
                Font = GlobalConsts.BasicFont,
                Padding = Padding.Empty,
                Margin = Padding.Empty,
                Parent = this,
            };

            // Content panel setup
            _content = new VerticalStackPanel
            {
                Location = new Point(GlobalConsts.LayerStepWidth, _button.Height + GlobalConsts.Padding),
                Width = Width - GlobalConsts.Padding - GlobalConsts.LayerStepWidth,
                Height = 0,
                Padding = Padding.Empty,
                Margin = Padding.Empty,
                Parent = this,
            };

            SizeChanged += OnSizeChanged;

            Controls.Add(_header);
            Controls.Add(_button);
            Controls.Add(_content);

            Toggle();

            OnSizeChanged(null, EventArgs.Empty);
        }

        public void Toggle()
        {
            _content.Visible = _isCollapsed;
            _isCollapsed = !_isCollapsed;
            RecalibrateHeight();
        }

        public void Add(Control element)
        {
            int height = _content.Height;
            element.Width = Width - GlobalConsts.LayerStepWidth;
            _content.Controls.Add(element);
            element.Parent = _content;

            if (element.GetType() == typeof(CollapsiblePanel))
            {
                ((CollapsiblePanel)element).ContainmentHeightChange += (sender, e) =>
                {
                    RecalibrateHeight();
                };
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!_isCollapsed)
            {
                e.Graphics.DrawLine(GlobalConsts.GrayBorderPen, 0, 0, 0, Height);
            }
        }

        protected virtual void RecalibrateHeight()
        {
            int y = 0;
            foreach (Control control in _content.Controls)
            {
                if (!control.Visible) continue;

                control.Location = new Point(control.Location.X, y);
                y += control.Height;
            }

            _selfSizeChanging = true;
            Height = y + _button.Height + GlobalConsts.Padding;
            _selfSizeChanging = false;
            _content.Height = y + GlobalConsts.Padding;

            ContainmentHeightChange?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnSizeChanged(object? sender, EventArgs e)
        {
            if (_selfSizeChanging) 
                return;

            RecalibrateHeight();
            UpdateInnersWidth();
        }

        protected void UpdateInnersWidth()
        {
            int innerWidth = GetInnerWidth();
            if (_lastInnerWidth == innerWidth) return;

            _header.Width = Width - _button.Width;
            _content.Width = innerWidth;

            foreach (Control control in _content.Controls)
            {
                control.Width = innerWidth;

                if (control.GetType() == typeof(SelfStretchingLabel))
                {
                    control.Width -= GlobalConsts.Padding + GlobalConsts.LayerStepWidth;
                }
            }
            _lastInnerWidth = innerWidth;

            RecalibrateHeight();
        }

        protected virtual int GetInnerWidth()
        {
            return Width - GlobalConsts.LayerStepWidth;
        }
    }
}
