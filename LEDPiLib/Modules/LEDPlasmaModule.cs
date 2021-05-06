using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Plasma)]
    public class LEDPlasmaModule : ModuleBase
    {
        private class PlasmaParam 
        {
            public double x { get; set; }
            public double y { get; set; }
            public double vx { get; set; }
            public double vy { get; set; }

            public double p { get; set; }
            public double d { get; set; }

            public Func<double, double> f { get; set; }
            public List<Func<double, double, double, double, double>> m { get; set; }

            public int a { get; set; }
            public int b { get; set; }
            public double n { get; set; }

            public double pt { get; set; }
        }

        private static List<Func<double, double, double, double, double>> functions =
            new List<Func<double, double, double, double, double>>()
            {
                inner,
                d2,
                d1,
                dn,
                d22,
                di,
                prod,
                sined2,
                sineInner
            };

        private static List<double> sineTable = new List<double>();
        private double t = 0;
        private PlasmaParam param;

        public LEDPlasmaModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration)
        {
            Random random = new Random();
            List<Func<double, double, double, double, double>> worklist =
                new List<Func<double, double, double, double, double>>();


            if (!string.IsNullOrEmpty(moduleConfiguration.Parameter))
            {
                List<int> functionIds = moduleConfiguration.Parameter.Split(',').Select(int.Parse).ToList();
                functionIds.ForEach(c => worklist.Add(functions[c]));
            }
            else
            {
                worklist.Add(functions[0]);
            }

            param = new PlasmaParam()
            {
                pt = random.NextDouble() + random.Next(23), //23,// random.Next(23) * 5,
                vx = random.NextDouble() * 8 - 4, //0.5,
                vy = random.NextDouble() * 8 - 4, //0.35,
                d = random.NextDouble() * 4 + 2, //5,
                m = worklist,
                f = sine,
                a = 1,
                b = 1,
                x = 1,
                y = 0,
                p = 3
            };

            Debug.WriteLine("pt: " + param.pt + ", vx: " + param.vx + ", vy: " + param.vy + ", d: " + param.d);

            if (sineTable.Count == 0)
            {
                for (int i = 0; i < 256; i++)
                {
                    sineTable.Add(Math.Round(((Math.Sin(i * 2 * Math.PI / 255) * 255) + 255) / 2));
                }
            }
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> Run()
        {
            Image<Rgba32> image = new Image<Rgba32>(LEDPIProcessorBase.LEDWidth, LEDPIProcessorBase.LEDHeight);
            SetBackgroundColor(image);

            for (int y = 0; y < LEDPIProcessorBase.LEDHeight; y++)
            {
                for(int x = 0; x < LEDPIProcessorBase.LEDWidth; x++)
                {
                    Rgba32 pixel = image.GetPixelRowSpan(y)[x];
                    double z = wave(x, y, param);

                    pixel.R = Convert.ToByte(z);
                    pixel.G = 0;
                    pixel.B = Convert.ToByte(z);

                    image.GetPixelRowSpan(y)[x] = pixel;
                }
            }

            t += .20;

            return image;
        }

        private double wave(int x, int y, PlasmaParam plasmaParam)
        {
            double ret = 0;

            foreach (Func<double, double, double, double, double> func in plasmaParam.m)
            {
                ret += plasmaParam.f.Invoke(Math.Floor(
                    func.Invoke(x + plasmaParam.vx * t, y + plasmaParam.vy * t, plasmaParam.x,
                        plasmaParam.y) * plasmaParam.d + (plasmaParam.p * t))) / (plasmaParam.m.Count);
            }

            return ret;
        }

        private double sawtooth(double x)
        {
            return x % 256;
        }

        private static double sine(double x)
        {
            x = x % 256;
            return (x < 0) ? sineTable[Convert.ToInt32(-x)] : sineTable[Convert.ToInt32(x)];
        }

        private double triangle(double x)
        {
            x = x % 256;
            return (x > 127) ? x - 127 : x;
        }


        private static double inner(double x, double y, double vx, double vy) { return vx * x + vy * y; }


        private static double d1(double x1, double y1, double x2, double y2) { return Math.Abs(x1 - x2) + Math.Abs(y1 - y2); }
        private static double dn(double x1, double y1, double x2, double y2)
        {
            double n = .075;
            double a = 1;
            double b = 1;
            //console.log(a, b);
            return Math.Pow(
                Math.Pow(Math.Abs(x1 - x2) / a, n)
                + Math.Pow(Math.Abs(y1 - y2), n) / b, 1 / n);
        }


        private static double d2(double x1, double y1, double x2, double y2)
        { return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2)); }

        private static double d22(double x1, double y1, double x2, double y2) { return (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2); }
        private static double di(double x1, double y1, double x2, double y2) { return Math.Max(Math.Abs(x1 - x2), Math.Abs(y1 - y2)); }

        private static double prod(double x, double y, double vx, double vy) { return vx * x * y; }
        private static double sined2(double x1, double y1, double x2, double y2)
        { return sine(Math.Floor(Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2)))); }
        private static double sineInner(double x, double y, double vx, double vy) { return sine(Math.Floor(vx * x + vy * y)); }

    }
}
