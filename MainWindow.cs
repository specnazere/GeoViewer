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
            _renderForm.KeyUp += UserEventRouter.KeyUp;
            _renderForm.KeyDown += UserEventRouter.KeyDown;
            _renderForm.MouseDown += UserEventRouter.MouseDown;
            _renderForm.MouseUp += UserEventRouter.MouseUp;
            _renderForm.MouseMove += UserEventRouter.MouseMove;
            _renderForm.MouseWheel += UserEventRouter.WheelScroll;
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
