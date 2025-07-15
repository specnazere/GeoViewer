namespace LayerSlicer.Forms
{
    public class CollapsiblePanel : Panel
    {
        protected string _title;
        protected Panel _content;
        protected bool _isCollapsed;
        protected Button _header;

        public CollapsiblePanel(string title)
        {
            _title = title;
            Height = GlobalConsts.ButtonHeight;
            Padding = Padding.Empty;
            Margin = Padding.Empty;

            // Header button setup
            _header = new Button
            {
                TextAlign = ContentAlignment.MiddleLeft,
                Text = $" {title}",
                Location = new Point(0, 0),
                FlatStyle = FlatStyle.Flat,
                Height = GlobalConsts.ButtonHeight,
                Width = Width,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Consolas", 12),
                Padding = Padding.Empty,
                Margin = Padding.Empty,
                Parent = this,
            };
            _header.FlatAppearance.BorderSize = 2;
            _header.Click += (s, e) => Toggle();

            // Content panel setup
            _content = new VerticalStackPanel
            {
                Location = new Point(GlobalConsts.LayerStepWidth, _header.Height),
                AutoSize = true,
                Width = Width - GlobalConsts.Padding - GlobalConsts.LayerStepWidth,
                Height = 0,
                Padding = Padding.Empty,
                Margin = Padding.Empty,
                Parent = this,
            };

            SizeChanged += (sender, e) =>
            {
                _header.Width = Width - GlobalConsts.Padding;
                _content.Width = Width - GlobalConsts.LayerStepWidth;

                foreach (Control control in _content.Controls)
                {
                    control.Width = Width - GlobalConsts.LayerStepWidth;

                    if (control.GetType() == typeof(SelfStretchingTextBox))
                    {
                        control.Width -= GlobalConsts.Padding;
                        control.Location = new Point(control.Location.X + GlobalConsts.Padding, control.Location.Y);
                    }
                    else if (control.GetType() == typeof(SliderGroup))
                    {
                        control.Width -= 4;
                    }

                }
            };

            Controls.Add(_content);
            Controls.Add(_header);

            Toggle();
        }

        public void Toggle()
        {
            _content.Visible = _isCollapsed;
            _isCollapsed = !_isCollapsed;
            _header.Text = _isCollapsed ? $"▶ {_title}" : $"▼ {_title}";
            Height = _header.Height + (_isCollapsed ? 0 : _content.Height);
        }

        public void Add(Control element)
        {
            int height = _content.Height;
            element.Location = new Point(element.Location.X + GlobalConsts.Padding, height);
            element.Width = Width - GlobalConsts.Padding - GlobalConsts.LayerStepWidth;
            _content.Controls.Add(element);
            element.Parent = _content;
            element.VisibleChanged += (sender, e) => RecalibrateSize();
            element.SizeChanged += (sender, e) => RecalibrateSize();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!_isCollapsed)
            {
                using (var pen = new Pen(Color.FromArgb(100, 100, 100), GlobalConsts.BorderWidth))
                {
                    e.Graphics.DrawLine(pen, GlobalConsts.Padding, 0, GlobalConsts.Padding, Height);
                    e.Graphics.DrawLine(pen, Width - 1, 0, Width - 1, Height);
                }
            }
        }

        private void RecalibrateSize()
        {
            int y = 0;
            foreach (Control control in _content.Controls)
            {
                if (!control.Visible) continue;

                control.Location = new Point(0, y);
                y += control.Height;
            }

            Height = y + _header.Height + GlobalConsts.Padding;
            _content.Height = Height;
        }
    }
}
