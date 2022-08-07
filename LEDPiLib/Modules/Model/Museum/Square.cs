using System;
using System.Collections.Generic;
using System.Numerics;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model.Common;
using SixLabors.ImageSharp;

namespace LEDPiLib.Modules.Model.Museum
{
    public class Square : DrawObjectBase
    {
        private readonly Vector2 size;
        private readonly bool odd;
        
        public Square(Vector2 position, float size,bool odd, Vector2 maxBounds):base(position, maxBounds)
        {
            this.size = new Vector2(size, size);
            this.odd = odd;
        }

        public override void Move()
        {
        }

        public bool Hit(Tuple<Vector2, Vector2> bulletSize)
        {
            if (isOverlapping(position.X, position.X + size.X, bulletSize.Item1.X, bulletSize.Item1.X + bulletSize.Item2.X))
            {
                return position.Y + size.Y >= bulletSize.Item1.Y;
            }

            return false;
        }

        private int explosionSize = 2;
        public List<DrawObjectBase> Explode()
        {
            List<DrawObjectBase> ret = new List<DrawObjectBase>();

            for (int x = 0; x <= size.X; x += explosionSize)
            {
                for (int y = 0; y <= size.Y; y += explosionSize)
                {
                    ret.Add(new Explosion( new Vector2(position.X + x, position.Y + y), explosionSize, odd ? Color.Blue : Color.LightBlue, maxBounds));
                }
            }

            return ret;
        }
        
        private bool isOverlapping (float start1, float end1, float start2, float end2)
        {
            return Math.Max (0, Math.Min (end1, end2) - Math.Max (start1, start2) + 1) > 0;
        }
        
        public override void Draw(LEDEngine3D engine3D)
        {
            engine3D.DrawFilledRectangle(new Rectangle(new Vector2(position.X, position.Y),
                size, odd ? Color.Blue : Color.LightBlue));
        }
    }
}