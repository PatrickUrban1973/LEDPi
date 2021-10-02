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

        public Paddle(bool left, ref Ball ball, Vector2D maxBounds) : base ((left ? new Vector2D(1, (maxBounds.vector.Y / 2) - (paddleLength / 2)) : new Vector2D(maxBounds.vector.X - paddleWidth - 2, (maxBounds.vector.Y / 2) - (paddleLength / 2))), new Vector2D(paddleWidth, paddleLength), maxBounds)
        {
            this.ball = ball;
            this.left = left;

            startPos = rectangle.Pos.vector.Y;
        }

        public void Reset()
        {
            rectangle.Pos.vector.Y = startPos;
        }

        public override void Move()
        {
            if (left == !ball.BallLeft)
            {
                rectangle.Pos.vector.Y += 1;
            }
            else
            {
                rectangle.Pos.vector.Y -= speed * (rectangle.Pos.vector.Y + (rectangle.Size.vector.Y / 2) > ball.GetPositionY() ? 1 : -1);
            }

            if (rectangle.Pos.vector.Y <= 0)
            {
                rectangle.Pos.vector.Y = 0;
            }
            else if (rectangle.Pos.vector.Y + rectangle.Size.vector.Y >= maxBounds.vector.Y)
            {
                rectangle.Pos.vector.Y = maxBounds.vector.Y - rectangle.Size.vector.Y - 1;
            }

            hitBall();
        }

        private void hitBall()
        {
            float selfX = rectangle.Pos.vector.X + (left ? rectangle.Size.vector.X + 1 : -1);

            if (left == ball.BallLeft && selfX - 1 <= ball.GetPositionX() && selfX + 1 >= ball.GetPositionX())
            {
                float minPaddleY = rectangle.Pos.vector.Y;
                float maxPaddleY = rectangle.Pos.vector.Y + rectangle.Size.vector.Y;
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
