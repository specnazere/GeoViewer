namespace LayerSlicer.Forms
{
    internal class HeaderButton : Button
    {
        private bool _toggled = false;
        private readonly PointF[] _triangleToRightPoints = new PointF[3];
        private readonly PointF[] _triangleToBottomPoints = new PointF[3];

        public HeaderButton() : base()
        { 
            Size = new Size(GlobalConsts.ButtonHeight, GlobalConsts.ButtonHeight + GlobalConsts.BorderWidth);

            FlatStyle = FlatStyle.Flat;
            TextAlign = ContentAlignment.MiddleLeft;
            Location = new Point(0, 0);
            BackColor = Color.White;
            ForeColor = Color.Black;
            Padding = Padding.Empty;
            Margin = Padding.Empty;

            float center = GlobalConsts.ButtonHeight / 2f;
            float radius = (center - GlobalConsts.BorderWidth - 1) * 0.7f;

            // Углы для вершин треугольника (в радианах)
            double startAngle = 0; // Начинаем сверху (90 градусов)

            for (int i = 0; i < 3; i++)
            {
                double angle = startAngle + i * 2 * Math.PI / 3;
                _triangleToRightPoints[i] = new PointF(
                    center + radius * (float)Math.Cos(angle) - center * 0.1f,
                    center - radius * (float)Math.Sin(angle));
            }

            startAngle = double.Pi / 2 * 3; // Начинаем сверху (90 градусов)

            for (int i = 0; i < 3; i++)
            {
                double angle = startAngle + i * 2 * Math.PI / 3;
                _triangleToBottomPoints[i] = new PointF(
                    center + radius * (float)Math.Cos(angle),
                    center - radius * (float)Math.Sin(angle) - center * 0.1f);
            }

            Click += (sender, e) =>
            {
                _toggled = !_toggled;
                Invalidate();
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.DrawRectangle(GlobalConsts.GrayBorderPen, new Rectangle(0, 0,
                Height - 1, Height - 1));
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
