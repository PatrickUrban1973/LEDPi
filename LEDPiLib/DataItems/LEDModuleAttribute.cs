using System;
using static LEDPiLib.LEDPIProcessorBase;

namespace LEDPiLib.DataItems
{
    internal class LEDModuleAttribute : Attribute
    {
        private readonly LEDModules _lEDModule;

        public LEDModuleAttribute(LEDModules lEDModule)
        {
            _lEDModule = lEDModule;
        }

        public LEDModules LEDModule { get { return _lEDModule; } }
    }
}
