using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using static LEDPiLib.LEDPIProcessorBase;
using System.Collections.Generic;
using LEDPiLib.Modules.Model;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.CollatzConjecture)]
    public class LEDCollatzConjectureModule : ModuleBase
    {
        private List<List<int>> collatzes = new List<List<int>>();
        private int index = 0;

        public LEDCollatzConjectureModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 2f, 0)
        {
            for (int i = 1; i < 100000; i++)
            {
                List<int> sequence = new List<int>();
                collatzes.Add(sequence);
                int n = i;
                do
                {
                    sequence.Add(n);
                    n = collatz(n);
                } while (n != 1);
            }

            collatzes.ForEach(c => c.Reverse());
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = new Image<Rgba32>(renderWidth, renderHeight);
            SetBackgroundColor(image);

            Vector2D startPoint;
            try
            {
                if (index < collatzes.Count)
                {
                    List<int> list = collatzes[index++];

                    startPoint = new Vector2D(renderWidth / 2, renderHeight);

                    foreach (int collatz in list)
                    {
                        Vector2D secondPoint;

                        if (collatz % 2 == 0)
                            secondPoint = new Vector2D(startPoint.vector * new Vector2D(1.3f, 0.9f).vector);
                        else
                            secondPoint = new Vector2D(startPoint.vector * new Vector2D(0.7f, 0.9f).vector);

                        image.Mutate(c => c.DrawLines(Color.Azure, 0.1f, new[] { new PointF(startPoint.vector.X, startPoint.vector.Y), new PointF(secondPoint.vector.X, secondPoint.vector.Y) }));

                        startPoint = secondPoint;
                    }
                }
            }
            catch(Exception e)
            {
                throw e;
            }

            return image;
        }

        private int collatz(int n)
        {
            // even
            if (n % 2 == 0)
            {
                return n / 2;
                // odd
            }
            else
            {
                return (n * 3 + 1) / 2;
            }
        }
    }
}
