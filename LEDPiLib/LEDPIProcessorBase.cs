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
        private TimeSpan _frameRate = new TimeSpan(0, 0, 0, 0, 5);
        private ModuleBase lastModule;
        protected  static Func<Image<Rgba32>, bool> processor;
        private Dictionary<string, List<Timer>> cron2Timers = new Dictionary<string, List<Timer>>();
        private static Random random = new Random();

        public enum LEDModules
        {
            Grouped = -2,
            Surprise = -1,
            None,
            Test,
            Clock,
            Rain,
            ScrollingText,
            Pictures,
            AmigaBall,
            AmigaBallFast,
            Gif,
            ReactionDiffusion,
            Plasma,
            Fluid,
            Cube,
            Firework,
            Terrain,
            Countdown,
        }

        protected Image<Rgba32> currentImage;

        public LEDPIProcessorBase()
        {
        }

        public void RunModule(ModulePlaylist playlist, bool awaitTask = false)
        {
            try
            {
               
                bool firstRun = true;
                currentImage = new Image<Rgba32>(LEDWidth, LEDHeight);

                lastModule?.Stop();

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
                                    Thread.Sleep(_frameRate);
                                    module.Start(processor);
                                }
                            }
                        }
                    }
                });

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
            if (moduleConfiguration.Module == LEDModules.Surprise)
            {
                int randomIndex = random.Next() % moduleConfiguration.SubConfigurations.Count;
                moduleConfiguration = moduleConfiguration.SubConfigurations[randomIndex];
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
                        lastModule.Pausing = true;
                        ModuleBase module = GetModuleBase(moduleConfiguration);

                        while (module.IsRunning)
                        {
                            Thread.Sleep(_frameRate);
                            module.Start(processor);
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
    }
}
