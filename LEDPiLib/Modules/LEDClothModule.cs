using System.Collections.Generic;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Model.Cloth;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Cloth)]
    public class LEDClothModule : ModuleBase
    {
        private const int cols = 12;
        private const int rows = 7;
        private const int border = 2;
        private const int engineRange = 20;

        private readonly List<ClothPointBase> points = new List<ClothPointBase>();
        private readonly List<Stick> sticks = new List<Stick>();

        public LEDClothModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 2f, 20)
        {
            ClothPoint.InitPoints(0.999f, 0.5f, 0.9f, renderWidth, renderHeight);

            bool firstRow = true;
            Dictionary<int, List<ClothPointBase>> tempDict = new Dictionary<int, List<ClothPointBase>>();


            float offset = (renderWidth - (2 * border)) / cols;

            for (int y = 1; y <= rows; y++)
            {
                tempDict.Add(y, new List<ClothPointBase>());
                for(int x = 1; x <= cols; x++)
                {
                    float calcX = border + (x * (x == 1 ? 1 : offset));
                    ClothPointBase newPoint;

                    if (firstRow && (x == 1 || x == cols))
                    {
                        if (x == 1)
                            newPoint = new ClothPointEngine(calcX, 0, 0 + engineRange, 0, engineRange, 0, 0.05f);
                        else
                            newPoint = new ClothPointEngine(calcX, 0, renderWidth - engineRange, 0, engineRange, -1, 0.1f);
                    }
                    else
                    {
                        newPoint = new ClothPoint(calcX, 0, calcX, 0);
                    }

                    points.Add(newPoint);
                    tempDict[y].Add(newPoint);

                    if (x > 1)
                    {
                        sticks.Add(new Stick(newPoint, tempDict[y][x-2], offset));
                    }

                    if (!firstRow)
                    {
                        sticks.Add(new Stick(tempDict[y - 1][x-1], newPoint, offset));
 
                    }
                }

                firstRow = false;
            }
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = GetNewImage();

            points.ForEach(c => c.Update());

            sticks.ForEach(c => c.UpdateStick());
            points.ForEach(c => c.ConstrainPoint());


            sticks.ForEach(c => c.Display(image));
           // points.ForEach(c => c.Display(image));

            return image;
        }
    }
}
