using System.Diagnostics;

using SharpDX;

namespace LayerSlicer
{
    internal static class GlobalUpdater
    {
        private static Stopwatch _clock = new Stopwatch();
        private static double _last = 0;

        static GlobalUpdater()
        {
            _clock.Start();
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        public static void Update()
        {
            // Counting dt
            var dt = _clock.ElapsedMilliseconds * 0.001f - _last;

            // Updating camera
            var moveVector = Vector3.Zero;

            if ((GetAsyncKeyState(Keys.W) & 0x8000) != 0) { moveVector.Z += 1; }
            if ((GetAsyncKeyState(Keys.S) & 0x8000) != 0) { moveVector.Z -= 1; }
            if ((GetAsyncKeyState(Keys.A) & 0x8000) != 0) { moveVector.X -= 1; }
            if ((GetAsyncKeyState(Keys.D) & 0x8000) != 0) { moveVector.X += 1; }

            if (moveVector != Vector3.Zero)
            {
                moveVector = Vector3.Normalize(moveVector);

                GlobalCamera.Shift(moveVector * (float)dt * GlobalConsts.CameraSpeed);
            }

            // Finalising
            _last += dt;
        }
    }
}
