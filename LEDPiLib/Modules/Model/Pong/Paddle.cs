using System.Numerics;
using LEDPiLib.Modules.Model.Common;

namespace LEDPiLib.Modules.Model.Pong
{
    class Paddle : PongBase
    {
        private bool left;
        private Ball ball;
        private float speed = .9f; // integer called speed holding value of 5

        private const int paddleLength = 10;
        private const int paddleWidth = 2;
        private float startPos;

        public Paddle(bool left, ref Ball ball, Vector2 maxBounds) : base ((left ? new Vector2(1, (maxBounds.Y / 2) - (paddleLength / 2)) : new Vector2(maxBounds.X - paddleWidth - 2, (maxBounds.Y / 2) - (paddleLength / 2))), new Vector2(paddleWidth, paddleLength), maxBounds)
        {
            this.ball = ball;
            this.left = left;

            startPos = rectangle.Pos.Y;
        }

        public void Reset()
        {
            rectangle.Pos.Y = startPos;
        }

        public override void Move()
        {
            if (left == !ball.BallLeft)
            {
                rectangle.Pos.Y += 1;
            }
            else
            {
                rectangle.Pos.Y -= speed * (rectangle.Pos.Y + (rectangle.Size.Y / 2) > ball.GetPositionY() ? 1 : -1);
            }

            if (rectangle.Pos.Y <= 0)
            {
                rectangle.Pos.Y = 0;
            }
            else if (rectangle.Pos.Y + rectangle.Size.Y >= maxBounds.Y)
            {
                rectangle.Pos.Y = maxBounds.Y - rectangle.Size.Y - 1;
            }

            hitBall();
        }

        private void hitBall()
        {
            float selfX = rectangle.Pos.X + (left ? rectangle.Size.X + 1 : -1);

            if (left == ball.BallLeft && selfX - 1 <= ball.GetPositionX() && selfX + 1 >= ball.GetPositionX())
            {
                float minPaddleY = rectangle.Pos.Y;
                float maxPaddleY = rectangle.Pos.Y + rectangle.Size.Y;
                float minBallY = ball.GetPositionYRange().X;
                float maxBallY = ball.GetPositionYRange().Y;

                if ((minPaddleY <= minBallY && maxPaddleY >= minBallY) ||
                    (minPaddleY <= maxBallY && maxPaddleY >= maxBallY))
                {
                    ball.SetSpeed(0.1f, 0f);
                    ball.ReverseBall();
                }
            }
        }
    }
}
