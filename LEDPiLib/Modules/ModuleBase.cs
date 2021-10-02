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
        private static List<ModuleBase> _breakingNews = new List<ModuleBase>();

        private Stopwatch _stopwatchDuration = new Stopwatch();
        private static Stopwatch _stopwatchBreakingNews = new Stopwatch();
        protected bool _oneTime;
        private TimeSpan _duration;
        private bool isStarted;
        private List<ModuleBase> _layerModules = new List<ModuleBase>();
        private static Rgba32 transparentPixel = LEDPIProcessorBase.TransparentBackgroundColor.ToPixel<Rgba32>();

        protected readonly float renderOffset = 1f;
        private readonly int handBrake = 0;
        
        protected int renderWidth;
        protected int renderHeight;

        private Stopwatch stopwatchHandBrake = new Stopwatch();

        public ModuleBase(ModuleConfiguration moduleConfiguration, float renderOffset = 1f, int handBrake = 0)
        {
            this.renderOffset = renderOffset;
            this.handBrake = handBrake;
            renderWidth = Convert.ToInt32(LEDPIProcessorBase.LEDWidth * renderOffset);
            renderHeight = Convert.ToInt32(LEDPIProcessorBase.LEDHeight * renderOffset);

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

            if (handBrake > 0 || !IsLayer)
                stopwatchHandBrake.Start();
        }

        public static void AddBreakingNews(ModuleBase moduleBase)
        {
            lock (_breakingNews)
            {
                moduleBase.IsLayer = true;
                _breakingNews.Add(moduleBase);
                _stopwatchBreakingNews.Restart();
            }
        }

        public void AddLayerModule(ModuleBase moduleBase)
        {
            _layerModules.Add(moduleBase);
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

        public Image<Rgba32> Start()
        {
            try
            {
                while (Pausing)
                {
                    Thread.Sleep(new TimeSpan(0, 0, 0, 1));
                }

                List<Task<Image<Rgba32>>> taskList = new List<Task<Image<Rgba32>>>();
                Image <Rgba32> image;

                taskList.Add(Task<Image<Rgba32>>.Run(Run));

                foreach(ModuleBase layerModule in _layerModules)
                {
                    taskList.Add(Task<Image<Rgba32>>.Run(layerModule.Run));
                }

                foreach (ModuleBase breakingNews in _breakingNews)
                {
                    taskList.Add(Task<Image<Rgba32>>.Run(breakingNews.Run));
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

                if ((isStarted && _stopwatchDuration.Elapsed > _duration) || completedRun())
                    IsRunning = false;

                for(int i = _breakingNews.Count - 1; i >= 0; i--)
                {
                    if (_stopwatchBreakingNews.Elapsed > _breakingNews[i]._duration)
                    {
                        lock(_breakingNews)
                        {
                            _breakingNews.RemoveAt(i);
                        }
                        _stopwatchBreakingNews.Stop();
                    }
                }

                return image;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                if (!isStarted && _duration.TotalMilliseconds > 0)
                    _stopwatchDuration.Restart();

                isStarted = true;
            }
        }

        private Image<Rgba32> Run()
        {
            Image<Rgba32> image = RunInternal();

            if (renderOffset != 1f)
            {
                image.Mutate(c => c.Resize(LEDPIProcessorBase.LEDWidth, LEDPIProcessorBase.LEDHeight));
            }

            if (handBrake > 0 || !IsLayer)
            {
                int stoppedTime = Convert.ToInt32(stopwatchHandBrake.ElapsedMilliseconds);

                if (stoppedTime < handBrake)
                    Thread.Sleep(Convert.ToInt32(handBrake - stoppedTime));

                stopwatchHandBrake.Restart();
            }

            return image;
        }

        abstract protected Image<Rgba32> RunInternal();

        public void Stop()
        {
            IsRunning = false;
        }
    }
}
