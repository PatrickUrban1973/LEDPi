using System.Collections.Generic;
using System.IO;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Objects.Sprites;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.Sprites)]
    public class LEDSpritesModule : ModuleBase
    {
        private readonly List<Sprite> sprites = new List<Sprite>();
   
        public LEDSpritesModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 1f, 30)
        {
            string _path = Path.Combine(BasePath, "Modules","Objects","Sprites", "horse");
            string spriteSheetJSon;
            using (StreamReader r = new StreamReader(Path.Combine(_path, "horse.json")))
            {
                spriteSheetJSon = r.ReadToEnd();
            }
            
            Image<Rgba32> _animationSheet = Image.Load<Rgba32>(Path.Combine(_path, "horse.png"));
            Frames frames = JsonConvert.DeserializeObject<Frames>(spriteSheetJSon);
            List<Image<Rgba32>> animations = new List<Image<Rgba32>>();

            if (frames != null)
            {
                foreach (Frame frame in frames.frames)
                {
                    Image<Rgba32> cropedImage = _animationSheet.Clone();

                    cropedImage.Mutate(c =>
                        c.Crop(new Rectangle(frame.position.x, frame.position.y, frame.position.w, frame.position.h))
                            .Resize(30, 30));
                    animations.Add(cropedImage);
                }
            }

            for (int i = 0; i < 5; i++)
            {
                sprites.Add(new Sprite(animations, 0, i * 11, (MathHelper.GlobalRandom().Next(1, 5)) / 10f));
            }
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = GetNewImage();

            foreach(Sprite sprite in sprites)
            {
                sprite.Show(image);
                sprite.Animate(renderWidth);
            }
 
            return image;
        }
    }
}
