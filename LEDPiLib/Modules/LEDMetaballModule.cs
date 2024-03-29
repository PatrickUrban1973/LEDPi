﻿using System;
using System.Collections.Generic;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model;
using LEDPiLib.Modules.Model.Metaball;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Metaball)]
    public class LEDMetaballModule : ModuleBase
    {
        private List<Blob> blobs = new List<Blob>();
        private readonly float maxSum;
        public LEDMetaballModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 1f, 30)
        {
            for (int i = 0; i < 5; i++)
            {
                blobs.Add(new Blob(MathHelper.GlobalRandom().Next(renderWidth), MathHelper.GlobalRandom().Next(renderHeight)));
                maxSum = 7f * blobs.Count;
            }
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = GetNewImage();

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < renderHeight; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < renderWidth; x++)
                    {
                        float sum = 0;
                        foreach (Blob b in blobs)
                        {
                            float xdif = x - b.pos.X;
                            float ydif = y - b.pos.Y;
                            double d = Math.Sqrt((xdif * xdif) + (ydif * ydif));

                            //   float d = Vector2.Distance(new Vector2(x, y), b.pos.vector);

                            if (d > 0)
                                sum += (float)(10 * b.r / d);
                            else
                                sum = maxSum;
                        }

                        if (sum > maxSum)
                            sum = maxSum;

                        float colorSum = MathHelper.Map(sum, 0, maxSum, 0f, 254f);
                        //          Debug.Print(sum.ToString());

                        row[x] = new Color(new Rgba32(Convert.ToByte(colorSum % 50),
                            Convert.ToByte(colorSum), Convert.ToByte(colorSum)));

                    }
                }
            });

//            blobs.ForEach(c => c.Draw(image));

            blobs.ForEach(c => c.Update(renderWidth, renderHeight));
            return image;
        }
    }
}
