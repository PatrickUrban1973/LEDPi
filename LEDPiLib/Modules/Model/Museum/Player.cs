using System;
using System.Numerics;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model.Common;
using SixLabors.ImageSharp;

namespace LEDPiLib.Modules.Model.Museum
{
    public class Player : DrawObjectBase
    {
        private bool directionLeft;
        
        public Player(Vector2 position, Vector2 maxBounds):base( new Vector2(position.X, maxBounds.Y), maxBounds)
        {
            directionLeft = Convert.ToBoolean(MathHelper.GlobalRandom().Next(0, 2));
        }

        public override void Move()
        {
            if (position.X - 3 < 0 || position.X + 3 > maxBounds.X)
                directionLeft = !directionLeft;

            position.X = directionLeft ? position.X + 1 : position.X - 1;
        }

        public Bullet Action()
        {
            Bullet ret = null;
            
            if (MathHelper.GlobalRandom().Next(0, 55) == 4)
            {
                ret =new Bullet(new Vector2(position.X, position.Y - 5), maxBounds);
            }

            return ret;
        }

        public override void Draw(LEDEngine3D engine3D)
        {
            engine3D.DrawFilledRectangle(new Rectangle(new Vector2(position.X - 2, position.Y),
                new Vector2(5, 1), Color.Green));
            engine3D.DrawFilledRectangle(new Rectangle(new Vector2(position.X - 2, position.Y - 1),
                new Vector2(5, 1), Color.Green));
            engine3D.DrawFilledRectangle(new Rectangle(new Vector2(position.X - 1, position.Y - 2),
                new Vector2(3,1), Color.Green));
            engine3D.DrawFilledRectangle(new Rectangle(new Vector2(position.X, position.Y - 3),
                new Vector2(1,1), Color.Green));
            engine3D.DrawFilledRectangle(new Rectangle(new Vector2(position.X, position.Y - 4),
                new Vector2(1,1), Color.Green));
        }
    }
}