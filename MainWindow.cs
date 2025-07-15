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
            _renderForm.KeyUp += UserEventHandler.KeyUp;
            _renderForm.KeyDown += UserEventHandler.KeyDown;
            _renderForm.MouseDown += UserEventHandler.MouseDown;
            _renderForm.MouseUp += UserEventHandler.MouseUp;
            _renderForm.MouseMove += UserEventHandler.MouseMove;
            _renderForm.MouseWheel += UserEventHandler.WheelScroll;
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
