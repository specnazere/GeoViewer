using SharpDX;

namespace LayerSlicer
{
    internal static class UserEventHandler
    {
        private static bool _rotating = false;
        private static bool _mouseMoving = false;

        private static Vector2 _prevMouseLoc = Vector2.Zero;

        public static void KeyUp(object? sender, KeyEventArgs keys)
        {
            switch ((int)keys.KeyCode)
            {
            case (int)Keys.Escape:
                Program.Close();
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
                _rotating = false;
                break;

            case MouseButtons.Middle:
                _mouseMoving = false;
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
                _rotating = true;
                break;

            case MouseButtons.Middle:
                _mouseMoving = true;
                break;

            default:
                break;
            }
        }

        public static void WheelScroll(object? sender, MouseEventArgs mouse)
        {
            GlobalCamera.Fov *= (float)Math.Pow(GlobalConsts.FovSensitivity, mouse.Delta / GlobalConsts.StandartMouseWheelStep);
        }

        public static void MouseMove(object? sender, MouseEventArgs mouse)
        {
            // Mouse updating
            Vector2 newMouseLoc = new Vector2(mouse.Location.X, mouse.Location.Y);
            Vector2 deltaMouseLoc = (_prevMouseLoc - newMouseLoc) * GlobalConsts.DegreesToRadians;

            // Per input updates
            if (_rotating)
            {
                GlobalCamera.GlobalRotate(
                    deltaMouseLoc.Y * GlobalConsts.MouseRotateSensitivity, 
                    deltaMouseLoc.X * GlobalConsts.MouseRotateSensitivity);
            }
            if (_mouseMoving)
            {
                GlobalCamera.Shift(new Vector3(deltaMouseLoc.X, -deltaMouseLoc.Y, 0));
            }

            // Finalizing
            _prevMouseLoc = newMouseLoc;
        }
    }
}
