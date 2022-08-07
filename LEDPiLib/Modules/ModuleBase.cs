using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Diagnostics;
using System.Threading;
using LEDPiLib.DataItems;
using System.Collections.Generic;
using System.Linq;
using SixLabors.ImageSharp.Processing;

namespace LEDPiLib.Modules
{
    public abstract class ModuleBase
    {
        private struct ModuleTask
        {
            public ModuleTask(int order, ModuleBase moduleBase)
            {
                Order = order;
                ModuleBase = moduleBase;
            }

            public readonly int Order;
            public readonly ModuleBase ModuleBase;
        }

        private static readonly Image<Rgba32>[] images = new Image<Rgba32>[7];

        private static readonly List<ModuleBase> _breakingNews = new List<ModuleBase>();

        private readonly Stopwatch _stopwatchDuration = new Stopwatch();
        private static readonly Stopwatch _stopwatchBreakingNews = new Stopwatch();
        private readonly bool _oneTime;
        private TimeSpan _duration;
        private bool isStarted;
        private readonly List<ModuleBase> _layerModules = new List<ModuleBase>();
        private static readonly Rgba32 transparentPixel = LEDPIProcessorBase.TransparentBackgroundColor.ToPixel<Rgba32>();

        protected readonly float renderOffset;
        private readonly int handBrake;
        
        protected readonly int renderWidth;
        protected readonly int renderHeight;

        private readonly Stopwatch stopwatchHandBrake = new Stopwatch();
        private Image<Rgba32> lastImage;
        private static int taskCounter;
        private static readonly object lockObject = new object();

        protected ModuleBase(ModuleConfiguration moduleConfiguration, float renderOffset = 1f, int handBrake = 0)
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

            lastImage = new Image<Rgba32>(renderWidth, renderHeight);
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

        protected void UseBlend(Image<Rgba32> image, float blendFactor)
        {
            image.Mutate(c => c.DrawImage(lastImage, new GraphicsOptions() { BlendPercentage = blendFactor, ColorBlendingMode = PixelColorBlendingMode.Screen, AlphaCompositionMode = PixelAlphaCompositionMode.DestOver}));
        }

        protected void SetLastPictureBlend(Image<Rgba32> image)
        {
            lastImage = image.Clone();
            ThreadPool.QueueUserWorkItem(RemoveDirtyPixelsWaitCallback);
        }

        private void RemoveDirtyPixelsWaitCallback(Object stateInfo)
        {
            lastImage.ProcessPixelRows(accessor =>
            {
                for(int i = 0; i < lastImage.Height; i++)
                {
                    var row = accessor.GetRowSpan(i);

                    for(int y = 0; y < lastImage.Width; y++)
                    {
                        var pixel = row[y];

                        int checkValue = pixel.R + pixel.G + pixel.B;

                        if (checkValue > 0 && checkValue <= 9)
                            row[y] = LEDPIProcessorBase.BlackBackgroundColor;
                    }

                }
            });
        }

        public bool IsRunning { get; private set; } = true;

        public bool Pausing { get; set; }

        protected bool IsLayer { get; set; }

        protected Image<Rgba32> GetNewImage()
        {
            return new Image<Rgba32>(renderWidth, renderHeight, GetBackground());
        }

        protected Color GetBackground()
        {
            return IsLayer ? LEDPIProcessorBase.TransparentBackgroundColor : LEDPIProcessorBase.BlackBackgroundColor;
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
                    Thread.Sleep(1);
                }

                //await all task before start again (new start of a module) 
                while (taskCounter != 0)
                    Thread.Sleep(1);

                int counter = 0;

                counter = addToThreadPool(counter, this);

                foreach(ModuleBase layerModule in _layerModules)
                {
                    counter = addToThreadPool(counter, layerModule);
                }

                lock (_breakingNews)
                {
                    foreach (ModuleBase breakingNews in _breakingNews)
                    {
                        counter = addToThreadPool(counter, breakingNews);
                    }
                }

                while (taskCounter != 0)
                    Thread.Sleep(1);

                Image<Rgba32> image = images[0];
                for(int i = 1; i < counter; i++)
                {
                    Image<Rgba32> nextLayer = images[i];
                    nextLayer.ProcessPixelRows(image, (sourceAccessor, targetAccessor) =>
                    {
                        for (int y = 0; y < LEDPIProcessorBase.LEDHeight; y++)
                        {
                            for (int x = 0; x < LEDPIProcessorBase.LEDWidth; x++)
                            {
                                Rgba32 nextPixel = sourceAccessor.GetRowSpan(y)[x];

                                if (!nextPixel.Equals(transparentPixel))
                                {
                                    ref Rgba32 canvasPixel = ref targetAccessor.GetRowSpan(y)[x];

                                    canvasPixel.R = nextPixel.R;
                                    canvasPixel.G = nextPixel.G;
                                    canvasPixel.B = nextPixel.B;    
                                    canvasPixel.A = nextPixel.A;
                                }
                            }
                        }
                    });
                }

                if ((isStarted && _stopwatchDuration.Elapsed > _duration) || completedRun())
                    IsRunning = false;

                lock (_breakingNews)
                {
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

        private int addToThreadPool(int counter, ModuleBase moduleBase)
        {
            lock (lockObject)
            {
                taskCounter++;
                ThreadPool.QueueUserWorkItem(ThreadProc, new ModuleTask(counter++, moduleBase));
                return counter;
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

        protected abstract Image<Rgba32> RunInternal();

        private static void ThreadProc(Object stateInfo)
        {
            lock (lockObject)
            {
                ModuleTask task = (ModuleTask)stateInfo;
                images[task.Order] = task.ModuleBase.Run();
                taskCounter--;
            }
        }

        public void Stop()
        {
            IsRunning = false;
        }
    }
}
