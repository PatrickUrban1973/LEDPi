using LEDPiLib.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LEDPiLib.DataItems;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using NCrontab;
using System.Diagnostics;

namespace LEDPiLib
{

    public abstract class LEDPIProcessorBase
    {
        public static Color TransparentBackgroundColor = Color.Transparent;
        public static Color BlackBackgroundColor = Color.Black;
        public const int LEDHeight = 64;
        public const int LEDWidth = 64;
        private ModuleBase lastModule;
        private Dictionary<string, List<Timer>> cron2Timers = new Dictionary<string, List<Timer>>();
        private Stopwatch _stopwatch = new Stopwatch();
        private TimeSpan wait = new TimeSpan(0, 0, 0, 0, 5);
        private Image<Rgba32> currentImage = new Image<Rgba32>(LEDHeight, LEDWidth);
        private bool hasNewImage = false;

        public static string BasePath = string.Empty;

        public static List<Rgba32> Colors = new List<Rgba32>();

        private Stopwatch frameRateWatch = new Stopwatch();
        private static LEDFrameRateModule frameRateModule;

        protected abstract void doProcess(Image<Rgba32> image);

        public long LastFrameRate { get; set; }

        public enum LEDModules
        {
            BreakingNews = -3,
            Grouped = -2,
            Surprise = -1,
            None,
            Test,
            Clock,
            Rain,
            ScrollingText,
            Pictures,
            MoreCube,
            AmigaBallFast,
            Gif,
            ReactionDiffusion,
            Plasma,
            Fluid,
            Cube,
            Firework,
            Terrain,
            Countdown,
            RayCasting,
            Pong,
            TimesTablesCardioid,
            MandelbrotSet,
            JuliaSet,
            Vertex,
            Cloth,
            Clock2,
            Hypercube,
            Metaball,
            FireEffect,
            WaterRipple,
            Birthday,
            Starfield,
            Phyllotaxis,
            BezierCurves,
            Molecule,
            SpaceColonization,
            Sprites,
            SuperShape,
            Snowflake,
            BrownianTreeSnowflake,
            MatrixRain,
            MutualAttraction,
            Museum,
            Maze,
            WorleyNoise,
        }

        public LEDPIProcessorBase()
        {
            for (var i = 0; i < 255; ++i)
            {
                float r = i;
                float g = 255 - (2 * Math.Abs(i - 127));
                float b = 255 - i;

                Color color = new Rgba32(Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b));

                Colors.Add(color);
            }
        }

        private Task processTask = null;

        public void RunModule(ModulePlaylist playlist, bool awaitTask = false, bool showFrameRate = false)
        {
            try
            {
                if (showFrameRate)
                    frameRateModule = new LEDFrameRateModule(new ModuleConfiguration(), this);

                bool firstRun = true;
                currentImage = new Image<Rgba32>(LEDWidth, LEDHeight);

                lastModule?.Stop();
                _stopwatch.Restart();

                // setup timer after a delay, because we need the correct system time
                Task.Run(() =>
                {
#if !DEBUG
                    Thread.Sleep(60 * 1000);
#endif
                    playlist.ModuleConfigurations.Where(c => !string.IsNullOrEmpty(c.CronTime)).ToList().ForEach(c => SetUpTimer(c));
                });

                Task task = Task.Run(() =>
                {
                    while (firstRun || playlist.Loop)
                    {
                        firstRun = false;

                        foreach (ModuleConfiguration moduleConfiguration in playlist.ModuleConfigurations.Where(c => string.IsNullOrEmpty(c.CronTime)))
                        {
                            List<ModuleConfiguration> configurations = new List<ModuleConfiguration>();

                            if (moduleConfiguration.Module == LEDModules.Grouped)
                            {
                                configurations.AddRange(moduleConfiguration.SubConfigurations);
                            }
                            else
                                configurations.Add(moduleConfiguration);

                            foreach(ModuleConfiguration configuration in configurations)
                            {
                                ModuleBase module = GetModuleBase(configuration);

                                while (module.IsRunning)
                                {
                                    lastModule = module;
                                    var newImage = module.Start();

                                    lock (currentImage)
                                    {
                                        currentImage = newImage;
                                        hasNewImage = true;
                                    }
                                }
                            }
                        }
                    }
                });

                if (processTask == null)
                {
                    processTask = Task.Run(() =>
                    {
                        while (process()) { }
                    });
                }

                if (awaitTask)
                    Task.WaitAll(new Task[] { task });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        public static ModuleBase GetModuleBase(ModuleConfiguration moduleConfiguration)
        {
            if (moduleConfiguration.Module == LEDModules.Surprise || moduleConfiguration.Module == LEDModules.BreakingNews || moduleConfiguration.Module == LEDModules.Grouped)
            {
                moduleConfiguration = moduleConfiguration.NextConfiguration(moduleConfiguration, moduleConfiguration.Module == LEDModules.Surprise);
            }

            Type type = System.Reflection.Assembly.GetCallingAssembly().GetTypes().First(c => 
            {
                LEDModuleAttribute attribValue = (LEDModuleAttribute) c.GetCustomAttributes(typeof(LEDModuleAttribute), true).FirstOrDefault();

                if (attribValue != null)
                {
                    return attribValue.LEDModule == moduleConfiguration.Module;
                }

                return false;
                });

            ModuleBase moduleBase = (ModuleBase) Activator.CreateInstance(type, new Object[] { moduleConfiguration });

            Console.WriteLine(moduleBase);
            Console.WriteLine("Configuration:");
            Console.WriteLine("Duration:" + moduleConfiguration.Duration);
            Console.WriteLine("OneTime" + moduleConfiguration.OneTime);
            Console.WriteLine("Parameter" + moduleConfiguration.Parameter);
            Console.WriteLine("CronTime" + moduleConfiguration.CronTime);

            if (frameRateModule != null)
                moduleBase.AddLayerModule(frameRateModule);

            return moduleBase;
        }

        private void SetUpTimer(ModuleConfiguration moduleConfiguration)
        {
            if (string.IsNullOrEmpty(moduleConfiguration.CronTime))
                return;

            CrontabSchedule s = CrontabSchedule.Parse(moduleConfiguration.CronTime);
            DateTime start = DateTime.Now;
            DateTime end = start.AddDays(1);
            IEnumerable<DateTime> occurrences = s.GetNextOccurrences(start, end);

            if (cron2Timers.ContainsKey(moduleConfiguration.CronTime))
                cron2Timers.Remove(moduleConfiguration.CronTime);

            List<Timer> timers = new List<Timer>();
            cron2Timers.Add(moduleConfiguration.CronTime, timers);

            foreach (DateTime startTime in occurrences)
            {
                DateTime current = DateTime.Now;
                TimeSpan alertTime = new TimeSpan(startTime.Hour, startTime.Minute, startTime.Second);
                TimeSpan timeToGo = alertTime - current.TimeOfDay;

                if (timeToGo < TimeSpan.Zero)
                {
                    timeToGo = timeToGo.Add(new TimeSpan(1, 0, 0, 0));
                }

                Timer moduleTimer = new Timer(x =>
                {
                    try
                    {
                        ModuleBase module = GetModuleBase(moduleConfiguration);

                        if (moduleConfiguration.Module == LEDModules.BreakingNews)
                        {
                            ModuleBase.AddBreakingNews(module);
                        }
                        else
                        {
                            lastModule.Pausing = true;

                            while (module.IsRunning)
                            {
                                var newImage = module.Start();

                                lock (currentImage)
                                {
                                    currentImage = newImage.Clone();
                                    hasNewImage = true;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                    }
                    finally
                    {
                        timers.Remove(timers.First());

                        if (!timers.Any())
                            SetUpTimer(moduleConfiguration);

                        lastModule.Pausing = false;
                    }
                }, null, timeToGo, Timeout.InfiniteTimeSpan);

                timers.Add(moduleTimer);
            }
        }

        protected bool process()
        {
            try
            {
                while (!hasNewImage)
                    Thread.Sleep(wait);

                LastFrameRate = frameRateWatch.ElapsedMilliseconds;

                lock (currentImage)
                {
                    doProcess(currentImage);
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return false;
            }
            finally
            {
                hasNewImage = false;
                _stopwatch.Restart();
                frameRateWatch.Restart();
            }
        }
    }
}
