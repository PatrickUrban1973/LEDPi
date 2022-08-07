using System.Collections.Generic;
using LEDPiLib.DataItems;
using LEDPiLib.Modules.Helper;
using LEDPiLib.Modules.Model.MatrixRain;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.Modules
{
    [LEDModule(LEDModules.MatrixRain)]
    public class LEDMatrixRainModule : ModuleBase
    {
        
        private const int _symbolSize = 15;
        private const float fadeInterval = 1.6f;

        private readonly List<Stream> streams = new List<Stream>();

        public LEDMatrixRainModule(ModuleConfiguration moduleConfiguration) : base(moduleConfiguration, 2f, 10)
        {
            var x = 0;
            for (var i = 0; i <= renderWidth / _symbolSize; i++)
            {
                var stream = new Stream(fadeInterval,_symbolSize);
                stream.GenerateSymbols(x, MathHelper.GlobalRandom().Next(-50, 1));
                streams.Add(stream);
                x += _symbolSize;
            }
        }

        protected override bool completedRun()
        {
            return false;
        }

        protected override Image<Rgba32> RunInternal()
        {
            Image<Rgba32> image = GetNewImage();
            UseBlend(image, .6f);

            streams.ForEach(stream => stream.Render(ref image));

            SetLastPictureBlend(image);
            return image;
        }
    }
}
