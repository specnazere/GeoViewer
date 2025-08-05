namespace LayerSlicer.Forms
{
    public class CollapsiblePanel : Panel
    {
        // Main class elements
        protected Panel _content;
        protected Label _header;
        protected Button _button;

        // Flag fields
        protected bool _isCollapsed;
        protected bool _isSelfSizeChanging = false;

        // Saving last state to not overcalculate some times
        protected int _lastInnerWidth = 0;

        public event EventHandler? ContainmentHeightChange;
        public event EventHandler? Toggled;

        public CollapsiblePanel(string title, bool state = false)
        {
            // Base setup
            Height = GlobalConsts.ButtonHeight;
            Padding = Padding.Empty;
            Margin = Padding.Empty;

            // Button setup
            _button = new HeaderButton();
            _button.FlatAppearance.BorderSize = 0;
            _button.Click += (s, e) => Toggle();

            // Header setup
            _header = new Label
            {
                TextAlign = ContentAlignment.MiddleLeft,
                Text = $"{title}",
                Location = new Point(_button.Width + GlobalConsts.BorderWidth - 1, 0),
                Height = GlobalConsts.ButtonHeight,
                Width = Width - _button.Width - GlobalConsts.BorderWidth,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.Black,
                ForeColor = Color.White,
                Font = GlobalConsts.BasicFont,
                Padding = new Padding(GlobalConsts.Padding + GlobalConsts.BorderWidth, 0, 0, 0),
                Margin = Padding.Empty,
                Parent = this,
            };

            // Content panel setup
            _content = new Panel
            {
                Location = new Point(GlobalConsts.LayerStepWidth, _button.Height + GlobalConsts.Padding),
                Width = Width - GlobalConsts.Padding - GlobalConsts.LayerStepWidth,
                Height = 0,
                Padding = Padding.Empty,
                Margin = Padding.Empty,
                Parent = this,
            };

            // Event handlers setup
            SizeChanged += OnSizeChanged;

            // Insert elements
            Controls.Add(_header);
            Controls.Add(_button);
            Controls.Add(_content);

            // Activate for first time
            Toggle();

            if (state)
            {
                Toggle();
                ((HeaderButton)_button).OnClick();
            }
        }

        public void Toggle()
        {
            // Change values
            _content.Visible = _isCollapsed;
            _isCollapsed = !_isCollapsed;

            // Call fuction to recalculate whole object height
            RecalibrateHeight();
            Toggled?.Invoke(this, EventArgs.Empty);
        }

        public void Add(Control element)
        {
            // Prepare new element
            element.Width = Width - GlobalConsts.LayerStepWidth;
            _content.Controls.Add(element);

            // Special behavior for some classes
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

        internal virtual void RecalibrateHeight()
        {
            int y = 0;
            foreach (Control control in _content.Controls)
            {
                if (!control.Visible) continue;

                control.Location = new Point(control.Location.X, y);
                y += control.Height;
            }

            _isSelfSizeChanging = true;
            Height = y + _button.Height + GlobalConsts.Padding;
            _isSelfSizeChanging = false;
            _content.Height = y + GlobalConsts.Padding;

            ContainmentHeightChange?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnSizeChanged(object? sender, EventArgs e)
        {
            if (_isSelfSizeChanging) 
                return;

            UpdateInnersWidth();
            RecalibrateHeight();
        }

        protected void UpdateInnersWidth()
        {
            // No fixed function so it can be overwrited in some cases
            int innerWidth = GetInnerWidth();
            if (_lastInnerWidth == innerWidth) return;

            _header.Width = Width - _button.Width;
            _content.Width = innerWidth;

            foreach (Control control in _content.Controls)
            {
                control.Width = innerWidth;

                // Special behavior for some classes
                if (control.GetType() == typeof(SelfStretchingLabel))
                {
                    control.Width -= GlobalConsts.Padding + GlobalConsts.LayerStepWidth;
                }
            }
            _lastInnerWidth = innerWidth;
        }

        protected virtual int GetInnerWidth()
        {
            return Width - GlobalConsts.LayerStepWidth;
        }

        public bool IsCollapsed()
        {
            return _isCollapsed;
        }

        public void Remove(Control? value)
        {
            _content.Controls.Remove(value);
            RecalibrateHeight();
        }
    }
}
