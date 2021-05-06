using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Diagnostics;
using System.Threading;
using LEDPiLib.DataItems;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using SixLabors.ImageSharp.Processing;

namespace LEDPiLib.Modules
{
    public abstract class ModuleBase
    {
        private Stopwatch _stopwatch = new Stopwatch();
        private Stopwatch _stopwatchDuration = new Stopwatch();
        private TimeSpan _frameRate = new TimeSpan(0, 0, 0, 0, 30);
        protected bool _oneTime;
        private TimeSpan _duration;
        private bool isStarted;
        private List<ModuleBase> _layerModules = new List<ModuleBase>();
        private static Rgba32 transparentPixel = LEDPIProcessorBase.TransparentBackgroundColor.ToPixel<Rgba32>();

        private readonly float renderOffset = 1f;
        protected int renderWidth;
        protected int renderHeight;

        public ModuleBase(ModuleConfiguration moduleConfiguration, float renderOffset = 1f)
        {
            this.renderOffset = renderOffset;
            renderWidth = Convert.ToInt32(LEDPIProcessorBase.LEDWidth * renderOffset);
            renderHeight = Convert.ToInt32(LEDPIProcessorBase.LEDHeight * renderOffset);

            _stopwatch.Restart();
            _duration = new TimeSpan(0, 0, moduleConfiguration.Duration);
            _oneTime = moduleConfiguration.OneTime;

            if (moduleConfiguration.SubConfigurations != null && moduleConfiguration.SubConfigurations.Any())
            {
                foreach(ModuleConfiguration layerConfiguration in moduleConfiguration.SubConfigurations)
                {
                    ModuleBase layerModule = LEDPIProcessorBase.GetModuleBase(layerConfiguration);
                    layerModule.IsLayer = true;
                    _layerModules.Add(layerModule);
                }
            }
        }

        public bool IsRunning { get; set; } = true;

        public bool Pausing { get; set; } = false;

        public bool IsLayer { get; set; } = false;

        protected void SetBackgroundColor(Image<Rgba32> image)
        {
            image.Mutate(c => c.BackgroundColor(IsLayer ? LEDPIProcessorBase.TransparentBackgroundColor : LEDPIProcessorBase.BlackBackgroundColor));
        }

        protected virtual bool completedRun()
        {
            return _oneTime && isStarted;
        }

        public void Start(Func<Image<Rgba32>, bool> processor)
        {
            List<Task<Image<Rgba32>>> taskList = new List<Task<Image<Rgba32>>>();
            Image <Rgba32> image;

            taskList.Add(Task<Image<Rgba32>>.Run(Run));

            foreach(ModuleBase layerModule in _layerModules)
            {
                taskList.Add(Task<Image<Rgba32>>.Run(layerModule.Run));
            }

            Task.WaitAll(taskList.ToArray());

            Task<Image<Rgba32>> task = taskList.First();

            image = task.Result;
            taskList.Remove(task);

            foreach(Task<Image<Rgba32>> nextTask in taskList)
            {
                Image<Rgba32> nextLayer = nextTask.Result;

                for(int y = 0; y < LEDPIProcessorBase.LEDHeight; y++)
                {
                    for(int x = 0; x < LEDPIProcessorBase.LEDWidth; x++)
                    {
                        Rgba32 nextPixel = nextLayer.GetPixelRowSpan(y)[x];

                        if (!nextPixel.Equals(transparentPixel))
                        {
                            nextPixel.A = Byte.MaxValue;
                            image.GetPixelRowSpan(y)[x] = nextPixel;
                        }
                    }
                }
            }

            process(processor, image);
        }


        abstract protected Image<Rgba32> Run();

        protected bool process(Func<Image<Rgba32>, bool> processor, Image<Rgba32> image)
        {
            try
            {
                while (Pausing)
                {
                    Thread.Sleep(new TimeSpan(0, 0, 0, 1));
                }

                if (_stopwatch.Elapsed < _frameRate)
                    Thread.Sleep(_frameRate - _stopwatch.Elapsed);

                if ((isStarted && _stopwatchDuration.Elapsed > _duration) || completedRun())
                    IsRunning = false;

                return processor.Invoke(image);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);

                return false;
            }
            finally
            {
                if (!isStarted && _duration.TotalMilliseconds > 0)
                    _stopwatchDuration.Restart();


                isStarted = true;
                _stopwatch.Restart();
            }
        }

        public void Stop()
        {
            IsRunning = false;
        }
    }
}
