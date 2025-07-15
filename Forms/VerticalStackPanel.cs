namespace LayerSlicer.Forms
{
    public class VerticalStackPanel : Panel
    {
        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            int y = 0;
            foreach (Control control in Controls)
            {
                if (!control.Visible) continue;

                control.Location = new Point(control.Location.X, y);
                y += control.Height;
            }

            if (AutoSize) Height = y;
        }
    }
}
