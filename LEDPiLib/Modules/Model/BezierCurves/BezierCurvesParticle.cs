using System;
using System.Collections.Generic;
using System.Text;

namespace LEDPiLib.Modules.Model.BezierCurves
{
    public class BezierCurvesParticle
    {
        private float dx;
        private float dy;


        public float X { get; set; }
        public float Y { get; set; }

        public static float RenderWidth { get; set; } = 0;
        public static float RenderHeight { get; set; } = 0;

        public BezierCurvesParticle(float x, float y)
        {
            X = x;
            Y = y;
            dx = new Random().Next(-8, 8);
            dy = new Random().Next(-8, 8);
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
