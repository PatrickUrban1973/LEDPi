using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using LEDPiLib.DataItems;
using static LEDPiLib.LEDPIProcessorBase;
using LEDPiLib.Modules.Model;
using LEDPiLib.Modules.Model.BrownianTreeSnowflake;
using SixLabors.ImageSharp.Processing;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.BrownianTreeSnowflake)]
    public class BrownianTreeSnowflakeModule : ModuleBase
    {
        private BrownianTreeSnowflake current;
        private readonly List<BrownianTreeSnowflake> snowflake = new List<BrownianTreeSnowflake>();

        public BrownianTreeSnowflakeModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 6f, 25)
        {
            current = new BrownianTreeSnowflake(renderWidth / 2, 0);
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = GetNewImage();

            int count = 0;
            while (!current.Finished() && !current.Intersects(snowflake))
            {
                current.Update();
                count++;
            }

            // If a particle doesn't move at all we're done
            // This is an exit condition not implemented in the video
            if (count != 0)
            {
                current.Render(ref image, renderWidth / 2, renderHeight / 2);
                snowflake.Add(current);
                current = new BrownianTreeSnowflake(renderWidth / 2, 0);

                //for (int i = 0; i < 6; i++)
                //{
//                }
            }

            foreach (BrownianTreeSnowflake p in snowflake)
            {
                p.Render(ref image, renderWidth / 2, renderHeight / 2);
            }

            Image<Rgba32> cropedImage = image.Clone();
            cropedImage.Mutate(c => c.Crop(new Rectangle(renderWidth / 2, 0, renderWidth / 2, renderHeight / 2)));

            cropedImage.Mutate(c => c.RotateFlip(RotateMode.None, FlipMode.Vertical));
            image.Mutate(c => c.DrawImage(cropedImage,
                new SixLabors.ImageSharp.Point((int)(renderWidth / 2), (int)(renderHeight / 2)), 1));

            cropedImage.Mutate(c => c.RotateFlip(RotateMode.None, FlipMode.Horizontal));
            image.Mutate(c => c.DrawImage(cropedImage,
                new SixLabors.ImageSharp.Point(0, (int)(renderHeight / 2)), 1));

            cropedImage.Mutate(c => c.RotateFlip(RotateMode.None, FlipMode.Vertical));
            image.Mutate(c => c.DrawImage(cropedImage,
                new SixLabors.ImageSharp.Point(0, 0), 1));

            return image;
        }
    }
}
