using LEDPiLib.Modules.Helper;
using SixLabors.ImageSharp;
using System.Collections.Generic;
using System.Numerics;

namespace LEDPiLib.Modules.Model.Pong
{
    public class Digit
    {
        private Dictionary<int, Rectangle> digitPoints = new Dictionary<int, Rectangle>();

        public Digit(Vector2 pos)
        {
            int index = 0;
            for(int y = 0; y < 10; y+=2)
            {
                for(int x = 0; x < 6; x+=2)
                {
                    digitPoints.Add(index++, new Rectangle(new Vector2(pos.X + x, pos.Y + y), new Vector2(1,1), Color.White));
                }
            }
        }

        public void Draw(int digit, LEDEngine3D engine3D)
        {
            List<Rectangle> drawPoints = new List<Rectangle>();

            if (digit == 0)
            {
                drawPoints.Add(digitPoints[0]);
                drawPoints.Add(digitPoints[1]);
                drawPoints.Add(digitPoints[2]);
                drawPoints.Add(digitPoints[3]);
                drawPoints.Add(digitPoints[5]);
                drawPoints.Add(digitPoints[6]);
                drawPoints.Add(digitPoints[8]);
                drawPoints.Add(digitPoints[9]);
                drawPoints.Add(digitPoints[11]);
                drawPoints.Add(digitPoints[12]);
                drawPoints.Add(digitPoints[13]);
                drawPoints.Add(digitPoints[14]);
            }
            if (digit ==1)
            {
                drawPoints.Add(digitPoints[2]);
                drawPoints.Add(digitPoints[5]);
                drawPoints.Add(digitPoints[8]);
                drawPoints.Add(digitPoints[11]);
                drawPoints.Add(digitPoints[14]);
            }
            if (digit == 2)
            {
                drawPoints.Add(digitPoints[0]);
                drawPoints.Add(digitPoints[1]);
                drawPoints.Add(digitPoints[2]);
                drawPoints.Add(digitPoints[5]);
                drawPoints.Add(digitPoints[6]);
                drawPoints.Add(digitPoints[7]);
                drawPoints.Add(digitPoints[8]);
                drawPoints.Add(digitPoints[9]);
                drawPoints.Add(digitPoints[12]);
                drawPoints.Add(digitPoints[13]);
                drawPoints.Add(digitPoints[14]);
            }
            if (digit == 3)
            {
                drawPoints.Add(digitPoints[0]);
                drawPoints.Add(digitPoints[1]);
                drawPoints.Add(digitPoints[2]);
                drawPoints.Add(digitPoints[5]);
                drawPoints.Add(digitPoints[6]);
                drawPoints.Add(digitPoints[7]);
                drawPoints.Add(digitPoints[8]);
                drawPoints.Add(digitPoints[11]);
                drawPoints.Add(digitPoints[12]);
                drawPoints.Add(digitPoints[13]);
                drawPoints.Add(digitPoints[14]);
            }
            if (digit == 4)
            {
                drawPoints.Add(digitPoints[0]);
                drawPoints.Add(digitPoints[2]);
                drawPoints.Add(digitPoints[3]);
                drawPoints.Add(digitPoints[5]);
                drawPoints.Add(digitPoints[6]);
                drawPoints.Add(digitPoints[7]);
                drawPoints.Add(digitPoints[8]);
                drawPoints.Add(digitPoints[11]);
                drawPoints.Add(digitPoints[14]);
            }
            if (digit == 5)
            {
                drawPoints.Add(digitPoints[0]);
                drawPoints.Add(digitPoints[1]);
                drawPoints.Add(digitPoints[2]);
                drawPoints.Add(digitPoints[3]);
                drawPoints.Add(digitPoints[6]);
                drawPoints.Add(digitPoints[7]);
                drawPoints.Add(digitPoints[8]);
                drawPoints.Add(digitPoints[11]);
                drawPoints.Add(digitPoints[12]);
                drawPoints.Add(digitPoints[13]);
                drawPoints.Add(digitPoints[14]);
            }
            if (digit == 6)
            {
                drawPoints.Add(digitPoints[0]);
                drawPoints.Add(digitPoints[1]);
                drawPoints.Add(digitPoints[2]);
                drawPoints.Add(digitPoints[3]);
                drawPoints.Add(digitPoints[6]);
                drawPoints.Add(digitPoints[7]);
                drawPoints.Add(digitPoints[8]);
                drawPoints.Add(digitPoints[9]);
                drawPoints.Add(digitPoints[11]);
                drawPoints.Add(digitPoints[12]);
                drawPoints.Add(digitPoints[13]);
                drawPoints.Add(digitPoints[14]);
            }
            if (digit == 7)
            {
                drawPoints.Add(digitPoints[0]);
                drawPoints.Add(digitPoints[1]);
                drawPoints.Add(digitPoints[2]);
                drawPoints.Add(digitPoints[5]);
                drawPoints.Add(digitPoints[8]);
                drawPoints.Add(digitPoints[11]);
                drawPoints.Add(digitPoints[14]);
            }
            if (digit == 8)
            {
                drawPoints.Add(digitPoints[0]);
                drawPoints.Add(digitPoints[1]);
                drawPoints.Add(digitPoints[2]);
                drawPoints.Add(digitPoints[3]);
                drawPoints.Add(digitPoints[5]);
                drawPoints.Add(digitPoints[6]);
                drawPoints.Add(digitPoints[7]);
                drawPoints.Add(digitPoints[8]);
                drawPoints.Add(digitPoints[9]);
                drawPoints.Add(digitPoints[11]);
                drawPoints.Add(digitPoints[12]);
                drawPoints.Add(digitPoints[13]);
                drawPoints.Add(digitPoints[14]);
            }
            if (digit == 9)
            {
                drawPoints.Add(digitPoints[0]);
                drawPoints.Add(digitPoints[1]);
                drawPoints.Add(digitPoints[2]);
                drawPoints.Add(digitPoints[3]);
                drawPoints.Add(digitPoints[5]);
                drawPoints.Add(digitPoints[6]);
                drawPoints.Add(digitPoints[7]);
                drawPoints.Add(digitPoints[8]);
                drawPoints.Add(digitPoints[11]);
                drawPoints.Add(digitPoints[14]);
            }

            drawPoints.ForEach(c => engine3D.DrawFilledRectangle(c));
        }
    }
}
