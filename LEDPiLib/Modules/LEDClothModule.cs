using System;
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
        private int cols = 12;
        private int rows = 7;
        private int border = 2;
        private int engineRange = 20;

        private List<ClothPointBase> points = new List<ClothPointBase>();
        private List<Stick> sticks = new List<Stick>();

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


        //    points.Add(new Point(100, 100, 100, 100));
        //    points.Add(new Point(200, 100, 200, 100));
        //    points.Add(new Point(200, 200, 200, 200));
        //    points.Add(new Point(100, 200, 100, 200));

        //    points.Add(new Point(150, 0, 150, 0, true));
        //    //points.Add(new Point(250, 100, 250, 100));
        //    //points.Add(new Point(450, 100, 450, 100, true));


        //    sticks.Add(new Stick(points[0], points[1]));
        //    sticks.Add(new Stick(points[0], points[2], true));
        //    sticks.Add(new Stick(points[1], points[2]));
        //    sticks.Add(new Stick(points[2], points[3]));
        //    sticks.Add(new Stick(points[3], points[0]));
        //    sticks.Add(new Stick(points[0], points[4]));
        //    //sticks.Add(new Stick(points[4], points[5]));
        //    //sticks.Add(new Stick(points[5], points[6]));
        }

        private float distance(Point p0, Point p1)
        {
            float dx = p1.X - p0.X;
            float dy = p1.Y - p0.Y;
            return Convert.ToSingle(Math.Sqrt(dx * dx + dy * dy));
        }


        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = new Image<Rgba32>(renderWidth, renderHeight);
            SetBackgroundColor(image);

            points.ForEach(c => c.Update());

            sticks.ForEach(c => c.UpdateStick());
            points.ForEach(c => c.ConstrainPoint());


            sticks.ForEach(c => c.Display(image));
           // points.ForEach(c => c.Display(image));

            return image;
        }
    }
}
