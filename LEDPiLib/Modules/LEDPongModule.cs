using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using LEDPiLib.DataItems;
using static LEDPiLib.LEDPIProcessorBase;
using LEDPiLib.Modules.Model;
using Rectangle = LEDPiLib.Modules.Model.Rectangle;
using LEDPiLib.Modules.Helper;
using System.Threading;
using LEDPiLib.Modules.Model.Pong;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Pong)]
    public class LEDPongModule : ModuleBase
    {
        private LEDEngine3D engine3D = new LEDEngine3D();

        private Paddle paddlePlayer1;
        private Paddle paddlePlayer2;
        private Ball ball;
        private Digit digitPlayer1;
        private Digit digitPlayer2;

        private int scorePlayer1 = 0;
        private int scorePlayer2 = 0;

        public LEDPongModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 1f, 15)
        {
            ball = new Ball(new Vector2D(renderWidth, renderHeight));

            paddlePlayer1 = new Paddle(true, ref ball, new Vector2D(renderWidth, renderHeight));
            paddlePlayer2 = new Paddle(false, ref ball, new Vector2D(renderWidth, renderHeight));

            digitPlayer1 = new Digit(new Vector2D((renderWidth / 2) - 10, 2));
            digitPlayer2 = new Digit(new Vector2D((renderWidth / 2) + 5, 2));
        }

        protected override bool completedRun()
        {
            return base.completedRun() && Math.Max(scorePlayer1, scorePlayer2) == 10;
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = new Image<Rgba32>(LEDPIProcessorBase.LEDWidth, LEDPIProcessorBase.LEDHeight);
            SetBackgroundColor(image);
            engine3D.Canvas = image;

            ball.Draw(engine3D);
            paddlePlayer1.Draw(engine3D);
            paddlePlayer2.Draw(engine3D);

            engine3D.DrawFilledRectangle(new Rectangle(new Vector2D(renderWidth / 2, 0), new Vector2D(0, renderHeight), Color.White));

            digitPlayer1.Draw(scorePlayer1, engine3D);
            digitPlayer2.Draw(scorePlayer2, engine3D);

            ball.Move();
            paddlePlayer1.Move();
            paddlePlayer2.Move();

            Thread.Sleep(15);

            Ball.OutsideBounds outsideBounds = ball.IsOutsideBounds();

            if (outsideBounds != Ball.OutsideBounds.No)
            {
                ball.ResetBall();
                paddlePlayer1.Reset();
                paddlePlayer2.Reset();

                if (outsideBounds == Ball.OutsideBounds.Left)
                {
                    scorePlayer2++; // add 1 to the CPU score
                }

                if (outsideBounds == Ball.OutsideBounds.Right)
                {
                    scorePlayer1++; // add one to the players score
                }
            }

            return image;
        }

    }
}
