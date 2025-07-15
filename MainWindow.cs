using LayerSlicer.Forms;

namespace LayerSlicer
{
    internal static class MainWindow
    {
        private static RenderForm _renderForm;
        private static InteractForm _interactForm;

        static MainWindow() 
        {
            // Setup render form
            _renderForm = new RenderForm();

            // Setup interact form
            _interactForm = new InteractForm();
            _renderForm.Controls.Add(_interactForm);
            _interactForm.Show();

            // Setup handlers
            _renderForm.KeyUp += EventHandler.KeyUp;
            _renderForm.KeyDown += EventHandler.KeyDown;
            _renderForm.MouseDown += EventHandler.MouseDown;
            _renderForm.MouseUp += EventHandler.MouseUp;
            _renderForm.MouseMove += EventHandler.MouseMove;
            _renderForm.MouseWheel += EventHandler.WheelScroll;
        }

        public static RenderForm GetRenderForm()
        {
            return _renderForm;
        }

        public static InteractForm GetInteractForm()
        {
            return _interactForm;
        }
    }
}
