using System;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.DataItems
{
    class LEDModuleAttribute : Attribute
    {
        private LEDModules _lEDModule;

        public LEDModuleAttribute(LEDModules lEDModule)
        {
            _lEDModule = lEDModule;
        }

        public LEDModules LEDModule { get { return _lEDModule; } }
    }
}
