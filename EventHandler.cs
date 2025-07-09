using SharpDX;

namespace LayerSlicer
{
    internal static class EventHandler
    {
        private static bool _rotating = false;
        private static Vector2 _prevMouseLoc = Vector2.Zero;

        public static void KeyUp(object? sender, KeyEventArgs keys)
        {
            if (keys.KeyCode == Keys.Escape)
            {
                Program.Close();
            }
        }

        public static void MouseUp(object? sender, MouseEventArgs mouse)
        {
            if (mouse.Button == MouseButtons.Left)
            {
                _rotating = false;
            }
        }

        public static void MouseDown(object? sender, MouseEventArgs mouse)
        {
            if (mouse.Button == MouseButtons.Left)
            {
                _rotating = true;
            }
        }

        public static void WheelScroll(object? sender, MouseEventArgs mouse)
        {
            GlobalCamera.Fov *= (float)Math.Pow(GlobalConsts.FovSensitivity, mouse.Delta / GlobalConsts.StandartMouseWheelStep);
        }

        public static void MouseMove(object? sender, MouseEventArgs mouse)
        {
            Vector2 newMouseLoc = new Vector2(mouse.Location.X, mouse.Location.Y);
            Vector2 deltaMouseLoc = (_prevMouseLoc - newMouseLoc) * GlobalConsts.DegreesToRadians;

            if (_rotating)
            {
                GlobalCamera.GlobalRotate(deltaMouseLoc.Y, deltaMouseLoc.X);
            }

            _prevMouseLoc = newMouseLoc;
        }
    }
}
