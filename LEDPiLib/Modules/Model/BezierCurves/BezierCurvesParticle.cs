using LEDPiLib.Modules.Helper;

namespace LEDPiLib.Modules.Model.BezierCurves
{
    public class BezierCurvesParticle
    {
        private float dx;
        private float dy;


        public float X { get; private set; }
        public float Y { get; private set; }

        public static float RenderWidth { get; set; }
        public static float RenderHeight { get; set; }

        public BezierCurvesParticle(float x, float y)
        {
            X = x;
            Y = y;
            dx = MathHelper.GlobalRandom().Next(-8, 9);
            dy = MathHelper.GlobalRandom().Next(-8, 9);
        }

        public void Update()
        {
            X += dx;
            Y += dy;

            if (X >= RenderWidth || X < 0)
            {
                dx *= -1;
            }

            if (Y >= RenderHeight || Y < 0)
            {
                dy *= -1;
            }
        }
    }
}
