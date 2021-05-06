using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LEDPiLib.DataItems
{
    public class ModulePlaylist
    {
        public ObservableCollection<ModuleConfiguration> ModuleConfigurations { get; set; }
        public bool Loop { get; set; }
    }
}
