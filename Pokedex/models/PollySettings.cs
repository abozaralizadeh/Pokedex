using System.Collections.Generic;

namespace Pokedex.models
{
    public class PollySettings
    {
        public int GlobalTimeOut { get; set; }

        public List<double> Retry { get; set; }

        public int DurationOfBreak { get; set; }

        public int HandledEventsAllowedBeforeBreaking { get; set; }
    }
}
