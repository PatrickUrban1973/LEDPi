using System;
using System.Collections.Generic;
using LEDPiLib.Modules.Helper;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LEDPiLib.Modules.Model.MatrixRain
{
    public class Stream
    {
        private static Font font;

        private readonly float fadeInterval;
        private readonly int symbolSize;        
        private int frameCount;
        
        private readonly List<Symbol> symbols = new List<Symbol>();
        private readonly int totalSymbols = MathHelper.GlobalRandom().Next(5, 10);
        private readonly int speed = MathHelper.GlobalRandom().Next(1, 5);

        public Stream(float fadeInterval, int symbolSize)
        {
            this.frameCount = 0;
            this.fadeInterval = fadeInterval;
            this.symbolSize = symbolSize;

            if (font == null)
                init();
        }

        private void init()
        {
            FontFamily fo;
            if (!SystemFonts.TryGet("Kochi Mincho", out fo))
                SystemFonts.TryGet("Arial", out fo);
            
            font = new Font(fo, symbolSize, FontStyle.Regular);
        }

        public void GenerateSymbols(int x, int y)
        {
            float opacity = 255f;
            bool first = new Random().Next(0, 4) == 1;

            for (int i = 0; i < totalSymbols; i++)
            {
                Symbol symbol = new Symbol(x, y, speed, first, opacity);
                
                symbol.SetToRandomSymbol(frameCount);
                symbols.Add(symbol);
                opacity -= (255f / totalSymbols / fadeInterval);
                y -= symbolSize;
                first = false;
            }
        }

        public void Render(ref Image<Rgba32> img)
        {
            foreach (Symbol symbol in symbols)
            {
                Rgba32 color;

                if (symbol.First)
                {
                    color = new Rgba32(50, (byte)symbol.Opacity, 50);
                }
                else
                {
                    color = new Rgba32(0, (byte)MathHelper.Map(symbol.Opacity, 0, 255, 0, 128), 0);
                }

                img.Mutate(c =>
                        c.DrawText(
                                symbol.Value,
                                font, color, new PointF(symbol.X, symbol.Y)));

                symbol.Rain(img.Height);
                symbol.SetToRandomSymbol(frameCount);
                frameCount++;
            }
        }
    }
}