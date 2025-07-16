using System.Drawing;

namespace LayerSlicer.Forms
{
    public class CollapsiblePanel : Panel
    {
        protected string _title;
        protected Panel _content;
        protected bool _isCollapsed;
        protected TextBox _header;
        protected Button _button;

        public event EventHandler? ContainmentHeightChange;

        public CollapsiblePanel(string title)
        {
            _title = title;
            Height = GlobalConsts.ButtonHeight;
            Padding = Padding.Empty;
            Margin = Padding.Empty;

            _button = new Button
            {
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = $"▶",
                Location = new Point(0, 0),
                Height = GlobalConsts.ButtonHeight,
                Width =  GlobalConsts.ButtonHeight - GlobalConsts.BorderWidth,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Consolas", 12),
                Padding = Padding.Empty,
                Margin = Padding.Empty,
                Parent = this,
            };

            // Header button setup
            _header = new TextBox
            {
                TextAlign = HorizontalAlignment.Left,
                Text = $" {title}",
                Location = new Point(GlobalConsts.ButtonHeight - GlobalConsts.BorderWidth, 0),
                Height = GlobalConsts.ButtonHeight,
                Width = Width - GlobalConsts.ButtonHeight + GlobalConsts.BorderWidth,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.Black,
                ForeColor = Color.White,
                ReadOnly = true,
                Font = new Font("Consolas", 12),
                Padding = Padding.Empty,
                Margin = Padding.Empty,
                Parent = this,
            };
            _button.FlatAppearance.BorderSize = 0;
            _button.Click += (s, e) => Toggle();

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
        }

        public void Toggle()
        {
            _content.Visible = _isCollapsed;
            _isCollapsed = !_isCollapsed;
            _button.Text = _isCollapsed ? $"▶" : $"▼";
            RecalibrateHeight();
        }

        public void Add(Control element)
        {
            int height = _content.Height;
            element.Location = new Point(element.Location.X + GlobalConsts.Padding, height);
            element.Width = Width - GlobalConsts.Padding - GlobalConsts.LayerStepWidth;
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
                e.Graphics.DrawLine(GlobalConsts.GrayBorderPen, GlobalConsts.Padding, 0, GlobalConsts.Padding, Height);
            }
        }

        protected virtual void RecalibrateHeight()
        {
            int y = 0;
            foreach (Control control in _content.Controls)
            {
                if (!control.Visible) continue;

                control.Location = new Point(0, y);
                y += control.Height;
            }

            Height = y + _button.Height + GlobalConsts.Padding;
            _content.Height = y + GlobalConsts.Padding;

            ContainmentHeightChange?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnSizeChanged(object? sender, EventArgs e)
        {
            _header.Width = Width - GlobalConsts.Padding - _button.Width;
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
                    control.Width -= GlobalConsts.Padding;
                }
            }
        }
    }
}
