using System;
using System.Numerics;

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

        bool ballDown = true;
        bool ballLeft = true;

        private float ballx = 1; // horizontal X speed value for the ball object 
        private float bally = 1; // vertical Y speed value for the ball object

        private const int ballLength = 2;
        private const int ballWidth = 2;
        private Random random = new Random();

        public Ball(Vector2D maxBounds) : base(new Vector2D((maxBounds.vector.X / 2) - 1, (maxBounds.vector.Y / 2) - 1), new Vector2D(ballWidth, ballLength), maxBounds)
        {
            ballDown = random.NextDouble() > 0.5;
            ballLeft = random.NextDouble() > 0.5;

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
            return rectangle.Pos.vector.Y;
        }

        public Vector2 GetPositionYRange()
        {
            return new Vector2(rectangle.Pos.vector.Y, rectangle.Size.vector.Y);
        }


        public float GetPositionX()
        {
            return rectangle.Pos.vector.X + (ballLeft ? 0 : ballWidth);
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
            if (rectangle.Pos.vector.X < 0)
                return OutsideBounds.Left;

            if (rectangle.Pos.vector.X + rectangle.Size.vector.X > maxBounds.vector.X)
                return OutsideBounds.Right;
            
            return OutsideBounds.No;
        }

        public void ResetBall()
        {
            rectangle.Pos.vector.X = (maxBounds.vector.X / 2) - 1; // reset the ball to the middle of the screen
            rectangle.Pos.vector.Y = (maxBounds.vector.Y / 2) - 1;
            ballx = -ballx; // change the balls direction
            ballLeft = !ballLeft;
            ballDown = random.NextDouble() > 0.5;
        }

        public void ReverseBall()
        {
            ballx = -ballx; // change the balls direction
            ballLeft = !ballLeft;
        }

        public override void Move()
        {
            rectangle.Pos.vector.Y -= bally * (ballDown ? 1 : -1); // assign the ball TOP to ball Y integer
            rectangle.Pos.vector.X -= ballx; // assign the ball LEFT to ball X integer

            if (rectangle.Pos.vector.Y <= 0 || rectangle.Pos.vector.Y + rectangle.Size.vector.Y >= maxBounds.vector.Y)
            {
                // then
                //reverse the speed of the ball so it stays within the screen
//                bally = -bally;
                ballDown = !ballDown;
            }
        }
    }
}
