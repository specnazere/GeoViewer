namespace LayerSlicer.Forms
{
    internal class HeaderButton : Button
    {
        // Triangles 
        private readonly PointF[] _triangleToRightPoints = new PointF[3];
        private readonly PointF[] _triangleToBottomPoints = new PointF[3];

        private bool _toggled = false;

        public HeaderButton() : base()
        { 
            Size = new Size(GlobalConsts.ButtonHeight - GlobalConsts.BorderWidth, GlobalConsts.ButtonHeight);

            // Base setup
            FlatStyle = FlatStyle.Flat;
            TextAlign = ContentAlignment.MiddleLeft;
            BackColor = Color.White;
            ForeColor = Color.Black;
            Padding = Padding.Empty;
            Margin = Padding.Empty;

            // Inscribed triangles circle data
            float center = GlobalConsts.ButtonHeight / 2f;
            float radius = (center - GlobalConsts.BorderWidth - 1) * 0.7f;

            // Calculate triangles
            double startAngle = 0; // Rotated to right
            for (int i = 0; i < 3; i++)
            {
                double angle = startAngle + i * 2 * Math.PI / 3;
                _triangleToRightPoints[i] = new PointF(
                    center + radius * (float)Math.Cos(angle) - center * 0.1f,
                    center - radius * (float)Math.Sin(angle));
            }

            startAngle = double.Pi / 2 * 3; // Rotated to bottom
            for (int i = 0; i < 3; i++)
            {
                double angle = startAngle + i * 2 * Math.PI / 3;
                _triangleToBottomPoints[i] = new PointF(
                    center + radius * (float)Math.Cos(angle),
                    center - radius * (float)Math.Sin(angle) - center * 0.1f); // Shifted
            }

            // Handle click event
            Click += (sender, e) =>
            {
                _toggled = !_toggled;
                Invalidate();
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Draw outer border
            e.Graphics.DrawRectangle(GlobalConsts.GrayBorderPen, new Rectangle(0, 0,
                Height - 1, Height - 1));
            // Draw inner border
            e.Graphics.DrawRectangle(Pens.Black, new Rectangle(GlobalConsts.BorderWidth, GlobalConsts.BorderWidth,
                Height - GlobalConsts.BorderWidth * 2 - 1, Height - GlobalConsts.BorderWidth * 2 - 1));

            if (_toggled)
            {
                e.Graphics.FillPolygon(Brushes.Black, _triangleToBottomPoints);
            }
            else
            {
                e.Graphics.FillPolygon(Brushes.Black, _triangleToRightPoints);
            }
        }
    }
}
