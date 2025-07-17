using System.Windows.Forms;

namespace LayerSlicer.Forms
{
    internal class SelfStretchingTextBox : TextBox
    {
        public SelfStretchingTextBox() : base()
        {
            TextChanged += RecalculateSize;
            SizeChanged += RecalculateSize;
        }
        
        private static void RecalculateSize(object? sender, EventArgs e)
        {
            var txt = (TextBox)sender;
            if (!txt.IsDisposed)
            {
                using (var g = txt.CreateGraphics())
                {
                    var size = g.MeasureString(txt.Text, txt.Font, txt.Width + 1);
                    txt.Height = (int)Math.Ceiling(size.Height) + GlobalConsts.Padding;
                }
            }
        }
    }
}
