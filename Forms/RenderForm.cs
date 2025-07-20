namespace LayerSlicer.Forms
{
    internal class RenderForm : Form
    {
        public RenderForm()
        {
            KeyPreview = true;
            Text = "SharpDX Color Transition";
            ClientSize = new Size(
                GlobalConsts.StandartWidth, 
                GlobalConsts.StandartHeight);
            StartPosition = FormStartPosition.CenterScreen;
        }
    }
}
