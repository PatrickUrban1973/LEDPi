using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using static LEDPiLib.LEDPIProcessorBase;
using LEDPiLib.Modules.Model.Snowflake;
using LEDPiLib.Modules.Objects.Sprites;
using System.IO;
using Newtonsoft.Json;
using SixLabors.ImageSharp.Processing;
using System.Numerics;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Snowflake)]
    public class LEDSnowflakeModule : ModuleBase
    {
        private readonly List<Snowflake> snowflakes = new List<Snowflake>();
        private readonly Vector2 gravity;
        private float zOff;
        private readonly Perlin perlin = new Perlin();
        private readonly Sprite santa;

        public LEDSnowflakeModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 2f, 25)
        {
            gravity = new Vector2(0, 1.0f);

            for (int i = 0; i < 100; i++)
            {
                int x = MathHelper.GlobalRandom().Next(renderWidth);
                int y = MathHelper.GlobalRandom().Next(renderHeight);
                snowflakes.Add(new Snowflake(x, y, renderWidth, renderHeight));
            }

            string _path = Path.Combine(BasePath, "Modules", "Objects", "Sprites", "santa");
            string spriteSheetJSon;
            using (StreamReader r = new StreamReader(Path.Combine(_path, "santa.json")))
            {
                spriteSheetJSon = r.ReadToEnd();
            }

            Image<Rgba32> _animationSheet = Image.Load<Rgba32>(Path.Combine(_path, "santa.png"));
            Frames frames = JsonConvert.DeserializeObject<Frames>(spriteSheetJSon);
            List<Image<Rgba32>> animations = new List<Image<Rgba32>>();

            if (frames != null)
            {
                foreach (Frame frame in frames.frames)
                {
                    Image<Rgba32> cropedImage = _animationSheet.Clone();

                    cropedImage.Mutate(c => c.Crop(new SixLabors.ImageSharp.Rectangle(frame.position.x,
                        frame.position.y, frame.position.w, frame.position.h)));
                    animations.Add(cropedImage);
                }
            }

            animations.ForEach(c => c.Mutate(b => b.Resize(Convert.ToInt32(c.Width * renderOffset), Convert.ToInt32(c.Height * renderOffset))));
            santa = new Sprite(animations, 0, renderHeight - 60, 0.1f);
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = GetNewImage();

            zOff += 0.1f;

            santa.Show(image);
            santa.Animate(renderWidth);

            foreach (Snowflake snowflake in snowflakes)
            {
                float xOff = snowflake.Pos.X / renderWidth;
                float yOff = snowflake.Pos.Y / renderHeight;
                float wAngle = (float)(perlin.perlin(xOff, yOff, zOff) * Math.PI * 2);
                Vector2 wind = MathHelper.RadianToVector2D(MathHelper.Map(wAngle, 0, (float) Math.PI *2f, 270, 360));
                wind *= 0.025f;

                snowflake.ApplyForce(gravity);
                snowflake.ApplyForce(wind);
                snowflake.Update();
                snowflake.Render(ref image);
            }

            return image;
        }
    }
}
