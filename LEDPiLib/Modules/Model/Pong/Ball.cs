using System.Numerics;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model.Common;

namespace LEDPiLib.Modules.Model.Pong
{
    class Ball : PongBase
    {
        public enum OutsideBounds
        {
            No,
            Left,
            Right
        }

        private bool ballDown;
        private bool ballLeft;

        private float ballx; // horizontal X speed value for the ball object 
        private float bally; // vertical Y speed value for the ball object

        private const int ballLength = 2;
        private const int ballWidth = 2;

        public Ball(Vector2 maxBounds) : base(new Vector2((maxBounds.X / 2) - 1, (maxBounds.Y / 2) - 1), new Vector2(ballWidth, ballLength), maxBounds)
        {
            ballDown = MathHelper.GlobalRandom().NextDouble() > 0.5;
            ballLeft = MathHelper.GlobalRandom().NextDouble() > 0.5;

            if (ballDown)
                bally = 1;
            else
                bally = -1;

            if (ballLeft)
                ballx = 1;
            else
                ballx = -1;
        }

        public float GetPositionY()
        {
            return rectangle.Pos.Y;
        }

        public Vector2 GetPositionYRange()
        {
            return new Vector2(rectangle.Pos.Y, rectangle.Size.Y);
        }


        public float GetPositionX()
        {
            return rectangle.Pos.X + (ballLeft ? 0 : ballWidth);
        }

        public void SetSpeed(float x, float y)
        {
            ballx += x;
            bally += y;
        }

        public bool BallLeft
        {
            get { return ballLeft; }
        }

        public OutsideBounds IsOutsideBounds()
        {
            if (rectangle.Pos.X < 0)
                return OutsideBounds.Left;

            if (rectangle.Pos.X + rectangle.Size.X > maxBounds.X)
                return OutsideBounds.Right;
            
            return OutsideBounds.No;
        }

        public void ResetBall()
        {
            rectangle.Pos.X = (maxBounds.X / 2) - 1; // reset the ball to the middle of the screen
            rectangle.Pos.Y = (maxBounds.Y / 2) - 1;
            ballx = -ballx; // change the balls direction
            ballLeft = !ballLeft;
            ballDown = MathHelper.GlobalRandom().NextDouble() > 0.5;
        }

        public void ReverseBall()
        {
            ballx = -ballx; // change the balls direction
            ballLeft = !ballLeft;
        }

        public override void Move()
        {
            rectangle.Pos.Y -= bally * (ballDown ? 1 : -1); // assign the ball TOP to ball Y integer
            rectangle.Pos.X -= ballx; // assign the ball LEFT to ball X integer

            if (rectangle.Pos.Y <= 0 || rectangle.Pos.Y + rectangle.Size.Y >= maxBounds.Y)
            {
                // then
                //reverse the speed of the ball so it stays within the screen
//                bally = -bally;
                ballDown = !ballDown;
            }
        }
    }
}
