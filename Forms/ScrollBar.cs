using System.Security.Cryptography.Pkcs;

namespace LayerSlicer.Forms
{
    internal class ScrollBar : Control
    {
        private int _contentSize = 10;
        private int _offset;

        private int _scrollerHeight;
        private float _scale;

        private bool _dragging = false;

        public int Offset
        {
            get => (int)(_offset / _scale);
        }

        public event EventHandler? Scrolled;

        public ScrollBar()
        {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint, true);
            Dock = DockStyle.None; 

            Size = new Size(GlobalConsts.ScrollBarWidth, 10);

            _scrollerHeight = Height * Height / _contentSize;

            SizeChanged += (sender, e) => RecalculateScroll();
            VisibleChanged += (sender, e) =>
            {
                _offset = 0;
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var frameRect = new Rectangle(new Point(0, 0), new Size(Width, Height));
            e.Graphics.DrawRectangle(GlobalConsts.WhiteBorderPen, frameRect);

            var scrollerRect = new Rectangle(
                new Point(0, _offset),
                new Size(Width, _scrollerHeight));
            e.Graphics.FillRectangle(Brushes.White, scrollerRect);
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
            int bottomBound = Height - _scrollerHeight;
            mouseY -= _scrollerHeight / 2;
            _offset = (int)Math.Max(0, Math.Min(bottomBound, mouseY));

            Scrolled?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        private void RecalculateScroll()
        {
            _scrollerHeight = Height * Height / _contentSize;
            _scale = _scrollerHeight / (float)Height;
        }

        public void SetScrollable(Control container)
        {
            container.SizeChanged += (sender, e) =>
            {
                _contentSize = container.Height;
                RecalculateScroll();
            };
        }
    }
}
