using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using LEDPiLib.DataItems;
using static LEDPiLib.LEDPIProcessorBase;
using LEDPiLib.Modules.Helper;
using System.Numerics;
using LEDPiLib.Modules.Model.Common;
using LEDPiLib.Modules.Model.Museum;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Museum)]
    public class LEDMuseumModule : ModuleBase
    {
        private readonly LEDEngine3D engine3D = new LEDEngine3D();

        private readonly List<DrawObjectBase> drawObjects = new List<DrawObjectBase>();
        private readonly Player player;
        private const int rows = 5;
        private const int size = 5;
        
        public LEDMuseumModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 1f, 20)
        {
            player = new Player(new Vector2(MathHelper.GlobalRandom().Next(10, renderWidth-10), 0), new Vector2(renderWidth, renderHeight));
            drawObjects.Add(player);
            bool odd = true;
            float yCoordinate = 0;
            
            for (int i = 0; i < rows; i++)
            {
                float startPosition = 0;

                while (startPosition < renderWidth)
                {
                    drawObjects.Add(new Square( new Vector2(startPosition, yCoordinate), size, odd, new Vector2(renderWidth, renderHeight)));
                    odd = !odd;
                    startPosition += size + 1;
                }

                yCoordinate += rows + 1;
            }
        }

        protected override bool completedRun()
        {
            return false;
        }
        
        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = GetNewImage();
            engine3D.Canvas = image;

            drawObjects.ForEach(c => c.Draw(engine3D));
            drawObjects.ForEach(c => c.Move());

            Bullet bullet = player.Action();
            if (bullet != null)
            {
                drawObjects.Add(bullet);
            }

            checkHitSquare();            
            return image;
        }


        private void checkHitSquare()
        {
            List<Bullet> bullets = drawObjects.Where(c => c.GetType() == typeof(Bullet)).Cast<Bullet>().ToList();
            List<Square> squares = drawObjects.Where(c => c.GetType() == typeof(Square)).Cast<Square>().ToList();
            List<DrawObjectBase> deleteList = new List<DrawObjectBase>();

            foreach (Bullet bullet in bullets)
            {
                Tuple<Vector2, Vector2> bulletSize = bullet.GetSize();

                foreach (Square square in squares)
                {
                    if (square.Hit(bulletSize))
                    {
                        deleteList.Add(square);
                        deleteList.Add(bullet);
                        drawObjects.AddRange(square.Explode());
                        break;
                    }
                }
            }

            deleteList.AddRange(drawObjects.Where(c => c.OutOfBounds()));
            deleteList.ForEach(c => drawObjects.Remove(c));
        }
    }
}
