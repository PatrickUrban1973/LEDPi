using System.Collections.Generic;
using LEDPiLib.Modules.Helper;

namespace LEDPiLib.DataItems
{
    public class ModuleConfiguration
    {
        private static readonly Dictionary<ModuleConfiguration, List<ModuleConfiguration>> nextConfigurationMap =
            new Dictionary<ModuleConfiguration, List<ModuleConfiguration>>();

        public LEDPIProcessorBase.LEDModules Module { get; set; }

        public string Parameter { get; set; }   

        public bool OneTime { get; set; }

        public int Duration { get; set; }

        public string CronTime { get; set; }

        public List<ModuleConfiguration> SubConfigurations { get; set; }

        public ModuleConfiguration NextConfiguration(ModuleConfiguration moduleConfiguration, bool withShuffle)
        {
            if (!nextConfigurationMap.ContainsKey(moduleConfiguration))
            {
                List<ModuleConfiguration> shuffleList = new List<ModuleConfiguration>(SubConfigurations);
                if (withShuffle)
                    FisherYatesShuffle(shuffleList);

                nextConfigurationMap.Add(moduleConfiguration, shuffleList);
            }

            ModuleConfiguration nextConfiguration = nextConfigurationMap[moduleConfiguration][0];
            nextConfigurationMap[moduleConfiguration].RemoveAt(0);
            nextConfigurationMap[moduleConfiguration].Add(nextConfiguration);

            return nextConfiguration ;
        }

        private static void FisherYatesShuffle<T>(IList<T> list)
        {
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                //Randomly set n to a value >=i but<count
                int n = MathHelper.GlobalRandom().Next(i, count);
                //swap the contents of list[n] with those of list[i]
                (list[n], list[i]) = (list[i], list[n]);
            }
        }

    }
}
