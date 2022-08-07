using System;
using System.Collections.Generic;
using System.IO;
using LEDPiLib.DataItems;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Birthday)]
    public class LEDBirthdayModule : ModuleBase
    {
        private readonly List<Image<Rgba32>> _flames = new List<Image<Rgba32>>();
        private readonly Image<Rgba32> _cake;
        private readonly List<Image<Rgba32>> _numerals = new List<Image<Rgba32>>();

        private int _internalCounter;
        private const float candleLength = 5f;
        private readonly int _age;

        public LEDBirthdayModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 1f, 30)
        {
            if (!string.IsNullOrEmpty(moduleConfiguration.Parameter))
            {
                _age = int.Parse(moduleConfiguration.Parameter);
            }


            string _path = Path.Combine(BasePath, "Modules","Objects","Birthday");

            _cake = Image.Load(Path.Combine(_path, "cake.png")).CloneAs<Rgba32>();
            _cake.Mutate(c => c.Resize(renderWidth, renderHeight));

            for (int i = 1; i < 5; i++)
            {
                _flames.Add(Image.Load(Path.Combine(_path, "flame" + i + "-export.png")).CloneAs<Rgba32>());
            }

            for (int i = 0; i < 10; i++)
            {
                _numerals.Add(Image.Load(Path.Combine(_path, "numeral" + i + "-export.png")).CloneAs<Rgba32>());
            }

            _flames.ForEach(c => c.Mutate(b => b.Resize(10, 10)));
            _numerals.ForEach(c => c.Mutate(b => b.Resize(10, 10)));

        }

        protected override Image<Rgba32> RunInternal()
        {
            
            Image<Rgba32> image = _cake.Clone();

            float yOffset1 = 2.3f;
            float yOffset2 = 1.85f;
            float yOffset3 = (yOffset1 + yOffset2) / 2;

            if (_age < 10)
            {
                if (_age >= 4)
                    drawCandle(image, -4f, yOffset2);

                if (_age >= 4)
                    drawCandle(image, -4f, yOffset1);

                if (_age >= 4)
                    drawCandle(image, 4f, yOffset2);

                if (_age >= 4)
                    drawCandle(image, 4f, yOffset1);

                if (_age == 1 || _age == 3 || _age == 5 || _age == 7 || _age == 9)
                    drawCandle(image, 0f, yOffset3);

                if (_age == 2 || _age == 3 || _age >= 8)
                    drawCandle(image, 8f, yOffset3);

                if (_age == 2 || _age == 3 || _age == 8 || _age == 9)
                    drawCandle(image, -8f, yOffset3);

                if (_age >= 6)
                    drawCandle(image, 15f, yOffset3);

                if (_age >= 6)
                    drawCandle(image, -15f, yOffset3);
            }
            else
            {
                bool first = true;
                float xOffset = -5.5f;

                foreach(int i in NumbersIn(_age))
                {
                    image.Mutate(c => c.DrawImage(_numerals[(i)], new Point(Convert.ToInt32(renderWidth / 2 + xOffset - 5), Convert.ToInt32(renderHeight - (renderHeight / yOffset3) - 8f)), 1f));
                    image.Mutate(c => c.DrawImage(_flames[(_internalCounter) % 4], new Point(Convert.ToInt32(renderWidth / 2 + xOffset - 5), Convert.ToInt32(renderHeight - (renderHeight / yOffset3) - _numerals[i].Height - 5)), 1f));
                    _internalCounter++;
                    if (first)
                    {
                        xOffset *= -1;
                        first = false;
                    }
                }

            }

            if (_internalCounter == int.MaxValue)
                _internalCounter = 0;

            return image;
        }

        private void drawCandle(Image<Rgba32> image, float xOffset, float yOffset)
        {
            image.Mutate(c => c.DrawLines(Color.Red, 2f, new[] { new PointF(renderWidth / 2 + xOffset, renderHeight - (renderHeight / yOffset)), new PointF(renderWidth / 2 + xOffset, renderHeight - (renderHeight / yOffset) - candleLength) }));
            image.Mutate(c => c.DrawImage(_flames[(_internalCounter) % 4], new Point(Convert.ToInt32(renderWidth / 2 + xOffset - 5), Convert.ToInt32(renderHeight - (renderHeight / yOffset) - candleLength - 5)), 1f));
            _internalCounter++;
        }

        private int[] NumbersIn(int value)
        {
            var numbers = new Stack<int>();

            for (; value > 0; value /= 10)
                numbers.Push(value % 10);

            return numbers.ToArray();
        }
    }
}
