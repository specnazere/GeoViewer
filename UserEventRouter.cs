using SharpDX;
using SharpDX.DirectInput;
using System.Runtime.InteropServices;

namespace LayerSlicer
{
    internal static class UserEventRouter
    {
        private static bool _isInFlyMode = false;
        private static bool _globalCameraRotating = false;
        private static bool _cameraMoving = false;

        private static DirectInput _directInput = new();
        private static Mouse _mouse;
        private static Vector2 _prevMouseLoc = Vector2.Zero;
        private static Vector2 _cursorLockPosition;

        public static bool IsTexting = false;

        [DllImport("user32.dll")]
        static extern bool ShowCursor(bool bShow);

        [DllImport("user32.dll")]
        static extern bool ClipCursor(ref System.Drawing.Rectangle rect);

        public static bool IsInFlyMode 
        { 
            get => _isInFlyMode; 
        }

        static UserEventRouter()
        {
            _mouse = new Mouse(_directInput);
            _mouse.Properties.AxisMode = DeviceAxisMode.Relative; // Относительные координаты
            _mouse.Acquire();
        }

        public static void KeyUp(object? sender, KeyEventArgs keys)
        {
            if (IsTexting) return;

            switch ((int)keys.KeyCode)
            {
            case (int)Keys.Escape:
                Program.Close();
                break;

            case (int)Keys.R:
                // Reseting model
                GlobalModel.Position = Vector3.Zero;
                GlobalModel.Rotation = Vector3.Zero;
                GlobalModel.Scale = Vector3.One;

                // Reseting camera
                GlobalCamera.Position = GlobalConsts.StandartCameraPostion;
                GlobalCamera.Rotation = Vector3.Zero;
                break;

            case (int)Keys.F:
                ShowCursor(_isInFlyMode);
                _isInFlyMode = !_isInFlyMode;
                if (_isInFlyMode)
                {
                    Form renderForm = MainWindow.GetRenderForm();
                    _cursorLockPosition = new Vector2(
                        renderForm.Left + renderForm.Width / 2,
                        renderForm.Top + renderForm.Height / 2);
                    _prevMouseLoc = _cursorLockPosition;
                    Cursor.Position = new System.Drawing.Point(
                        (int)_prevMouseLoc.X, (int)_prevMouseLoc.Y);
                }
                break;

            default:
                break;
            }
        }

        public static void KeyDown(object? sender, KeyEventArgs keys)
        {
            switch ((int)keys.KeyCode)
            {
            default:
                break;
            }
        }

        public static void MouseUp(object? sender, MouseEventArgs mouse)
        {
            switch (mouse.Button)
            {
            case MouseButtons.Left:
                _globalCameraRotating = false;
                break;

            case MouseButtons.Middle:
                _cameraMoving = false;
                break;

            default:
                break;
            }
        }

        public static void MouseDown(object? sender, MouseEventArgs mouse)
        {
            switch (mouse.Button)
            {
            case MouseButtons.Left:
                _globalCameraRotating = true;
                break;

            case MouseButtons.Middle:
                _cameraMoving = true;
                break;

            default:
                break;
            }
        }

        public static void WheelScroll(object? sender, MouseEventArgs mouse)
        {
            GlobalCamera.Fov *= (float)Math.Pow(
                GlobalConsts.FovSensitivity, mouse.Delta / GlobalConsts.StandartMouseWheelStep);
        }

        public static void MouseMove(object? sender, MouseEventArgs mouse)
        {
            // Mouse updating
            Form renderForm = MainWindow.GetRenderForm();
            Vector2 newMouseLoc = new Vector2(
                mouse.Location.X + renderForm.Left, mouse.Location.Y + renderForm.Top) +
                GlobalConsts.Windows10MainFormPosition;
            Vector2 deltaMouseLoc = (_prevMouseLoc - newMouseLoc) * GlobalConsts.DegreesToRadians;

            // Per input updates
            if (_isInFlyMode)
            {
                // Rotating camera
                var state = _mouse.GetCurrentState();
                GlobalCamera.LocalRotate(
                    deltaMouseLoc.Y * GlobalConsts.MouseRotateSensitivity, 
                    deltaMouseLoc.X * GlobalConsts.MouseRotateSensitivity, 0);

                // Move mouse
                Cursor.Position = new System.Drawing.Point(
                    (int)_cursorLockPosition.X, (int)_cursorLockPosition.Y);
                _prevMouseLoc = _cursorLockPosition;
                return; // Return to not change prev mouse position
            }
            else
            {
                if (_globalCameraRotating)
                {
                    GlobalCamera.GlobalRotate(
                        deltaMouseLoc.Y * GlobalConsts.MouseRotateSensitivity,
                        deltaMouseLoc.X * GlobalConsts.MouseRotateSensitivity);
                }
                if (_cameraMoving)
                {
                    GlobalCamera.Shift(new Vector3(deltaMouseLoc.X, -deltaMouseLoc.Y, 0));
                }
            }

            // Finalizing
            _prevMouseLoc = newMouseLoc;
        }
    }
}
