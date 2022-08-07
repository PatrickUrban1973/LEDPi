using System;
using System.Text;
using LEDPiLib.Modules.Helper;

namespace LEDPiLib.Modules.Model.MatrixRain
{
    public class Symbol
    {
        private readonly int x;
        private int y;
        private readonly int speed;
        private readonly int switchInterval ;
        private readonly bool first;
        private readonly float opacity;
        private string value;
        
        public Symbol(int x, int y, int speed, bool first, float opacity)
        {
            this.x = x;
            this.y = y;
            this.speed = speed;
            this.first = first;
            this.opacity = opacity;

            switchInterval = MathHelper.GlobalRandom().Next(50, 70);
        }

        public string Value 
        { 
            get
            {
                return value;
            }
        }

        public int X
        {
            get
            {
                return x;
            }
        }

        public int Y
        {
            get
            {
                return y;
            }
        }

        public bool First
        {
            get
            {
                return first;
            }
        }

        public float Opacity
        {
            get
            {
                return opacity;
            }
        }

        public void SetToRandomSymbol(int frameCount)
        {
            int charType = MathHelper.GlobalRandom().Next(0, 5);

            if (frameCount % switchInterval == 0)
            {
                if (charType > 1)
                {
                    var characterCode = Convert.ToInt32((3000 + MathHelper.GlobalRandom().Next(40, 97)).ToString(), 16);
                    value = ((char)characterCode).ToString();
                }
                else
                {
                    value = MathHelper.GlobalRandom().Next(0, 10).ToString();
                }
            }
        }

        public void Rain(int maxHeight)
        {
            y = y >= maxHeight ? 0 : y + speed;
        }
    }
}