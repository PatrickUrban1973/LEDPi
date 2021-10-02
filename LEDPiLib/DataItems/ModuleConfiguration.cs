using System;
using System.Collections.Generic;

namespace LEDPiLib.DataItems
{
    public class ModuleConfiguration
    {
        private ModuleConfiguration nextConfiguration = null;

        public LEDPIProcessorBase.LEDModules Module { get; set; }

        public string Parameter { get; set; }   

        public bool OneTime { get; set; }

        public int Duration { get; set; }

        public string CronTime { get; set; }

        public List<ModuleConfiguration> SubConfigurations { get; set; }


        private List<ModuleConfiguration> shuffleList;

        public ModuleConfiguration NextConfiguration()
        {
            if (nextConfiguration == null)
            {
                shuffleList = new List<ModuleConfiguration>(SubConfigurations);
                FisherYatesShuffle(shuffleList);
            }

            nextConfiguration = shuffleList[0];
            shuffleList.RemoveAt(0);
            shuffleList.Add(nextConfiguration);

            return nextConfiguration ;
        }

        private static void FisherYatesShuffle<T>(IList<T> list)
        {
            int count = list.Count;
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                //Randomly set n to a value >=i but<count
                int n = random.Next(i, count);
                //swap the contents of list[n] with those of list[i]
                T temp = list[n];
                list[n] = list[i];
                list[i] = temp;
            }
        }

    }
}
