using System;
using System.Collections.Generic;

namespace LEDPiLib.DataItems
{
    public class ModuleConfiguration
    {
        public LEDPIProcessorBase.LEDModules Module { get; set; }

        public string Parameter { get; set; }   

        public bool OneTime { get; set; }

        public int Duration { get; set; }

        public string CronTime { get; set; }

        public List<ModuleConfiguration> SubConfigurations { get; set; }
    }
}
