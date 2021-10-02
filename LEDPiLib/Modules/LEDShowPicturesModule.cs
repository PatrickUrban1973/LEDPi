using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using LEDPiLib.DataItems;
using LEDPiLib.Properties;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Pictures)]
    public class LEDShowPicturesModule : ModuleBase
    {
        private enum Direction
        {
            Left,
            Right,
            Up,
            Down
        }

        private List<Image<Rgba32>> _pictures;
        private Image<Rgba32> _wholeTextImage;
        private int _offset = 0;
        private TimeSpan _speed = new TimeSpan(0, 0, 0, 0, 15);
        private string _path;
        private Direction currentDirection;
        private int currentPictureIndex = 0;
        private Image<Rgba32> _display;
        private Stopwatch stopwatch = new Stopwatch();
        private bool completedFirstRun = false;
        private bool completedDisplayRun = false;

        public LEDShowPicturesModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration)
        {
            _path = moduleConfiguration.Parameter;
            currentDirection = Direction.Left;
        }

        protected override bool completedRun()
        {
            return base.completedRun() && completedDisplayRun;
        }

        protected override Image<Rgba32> RunInternal()
        {
            if (_pictures == null)
            {
                _pictures = new List<Image<Rgba32>>();

                _pictures.Add(Image.Load(Resources.Black));

                foreach (string fileName in Directory.GetFiles(_path))
                {
                    _pictures.Add(Image.Load(fileName).CloneAs<Rgba32>());
                }

                _pictures.ForEach(c => c.Mutate(b => b.Resize(LEDPIProcessorBase.LEDWidth, LEDPIProcessorBase.LEDHeight, KnownResamplers.NearestNeighbor)));
            }

            if (!stopwatch.IsRunning)
            {
                if (_offset == 0)
                {
                    Image<Rgba32> currentPicture =  _pictures[currentPictureIndex];
                    Image<Rgba32> nextPicture = _pictures[currentPictureIndex + 1];
                    
                    currentPictureIndex++;

                    if (currentPictureIndex >= _pictures.Count - 1)
                    {
                        completedFirstRun = true;
                        currentPictureIndex = 0;
                    }

                    _wholeTextImage = generatePicture(currentDirection, currentPicture, nextPicture);
                }
                else
                {
                    Thread.Sleep(_speed);
                }

                _display = shiftPicture(_offset++, currentDirection, _wholeTextImage);

                if (checkReset())
                {
                    stopwatch.Start();
                }
            }
            else
            {
                if (stopwatch.ElapsedMilliseconds > 20000)
                {
                    if (completedFirstRun)
                        completedDisplayRun = true;

                    stopwatch.Reset();
                }
            }

            return _display;
        }

        private bool checkReset()
        {
            if (((currentDirection == Direction.Up || currentDirection == Direction.Down) && _offset > LEDPIProcessorBase.LEDHeight)
                || ((currentDirection == Direction.Left || currentDirection == Direction.Right) && _offset > LEDPIProcessorBase.LEDWidth))
            {
                _offset = 0;

                currentDirection = (Direction) new Random().Next(3);
                return true;
            }

            return false;
        }

        private Image<Rgba32> generatePicture(Direction direction, Image<Rgba32> currentImage, Image<Rgba32> nextImage)
        {
            int height = LEDPIProcessorBase.LEDHeight;
            int width = LEDPIProcessorBase.LEDWidth;
            SixLabors.ImageSharp.Point pointCurrentImage;
            SixLabors.ImageSharp.Point pointNextImage;

            if (direction == Direction.Down || direction == Direction.Up)
            {
                if (direction == Direction.Down)
                {
                    pointCurrentImage = new SixLabors.ImageSharp.Point(0, 0);
                    pointNextImage = new SixLabors.ImageSharp.Point(0, LEDPIProcessorBase.LEDHeight);
                }
                else
                {
                    pointNextImage = new SixLabors.ImageSharp.Point(0, 0);
                    pointCurrentImage = new SixLabors.ImageSharp.Point(0, LEDPIProcessorBase.LEDHeight);
                }

                height *= 2;
            }
            else
            {
                if (direction == Direction.Left)
                {
                    pointCurrentImage = new SixLabors.ImageSharp.Point(0, 0);
                    pointNextImage = new SixLabors.ImageSharp.Point(LEDPIProcessorBase.LEDWidth, 0);
                }
                else
                {
                    pointNextImage = new SixLabors.ImageSharp.Point(0, 0);
                    pointCurrentImage = new SixLabors.ImageSharp.Point(LEDPIProcessorBase.LEDWidth, 0);
                }

                width *= 2;
            }

            Image<Rgba32> wholeImage = new Image<Rgba32>(width, height);

            wholeImage.Mutate(c => c.DrawImage(currentImage, pointCurrentImage, 1).DrawImage(nextImage, pointNextImage, 1));

            return wholeImage;
        }

        private Image<Rgba32> shiftPicture(int currentOffset, Direction direction, Image<Rgba32> wholeImage)
        {
            int x = 0;
            int y = 0;

            if (direction == Direction.Left)
            {
                x += currentOffset;
            }
            else if (direction == Direction.Right)
            {
                x = LEDPIProcessorBase.LEDWidth + 1 - _offset;
            }
            else if (direction == Direction.Down)
            {
                y += currentOffset;
            }
            else if (direction == Direction.Up)
            {
                y = LEDPIProcessorBase.LEDHeight + 1 - _offset;
            }

            Image<Rgba32> cropedImage = wholeImage.Clone();

            cropedImage.Mutate(c => c.Crop(new Rectangle(x,y, LEDPIProcessorBase.LEDWidth, LEDPIProcessorBase.LEDHeight)));

            return cropedImage;
        }
    }
}
