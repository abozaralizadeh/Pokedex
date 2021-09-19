using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pokedex.models
{
    public class PokemonApiResponse
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public int Id { get; set; }

        [JsonProperty("flavor_text_entries", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<FlavorTextEntry> FlavorTextEntries { get; set; }

        [JsonProperty("is_legendary", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsLegendary { get; set; }

        [JsonProperty("habitat", NullValueHandling = NullValueHandling.Ignore)]
        public NamedUrl Habitat { get; set; }
    }

    public class FlavorTextEntry
    {
        [JsonProperty("flavor_text", NullValueHandling = NullValueHandling.Ignore)]
        public string FlavorText { get; set; }

        [JsonProperty("language", NullValueHandling = NullValueHandling.Ignore)]
        public NamedUrl Language { get; set; }
    }

    public class NamedUrl
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }
    }

}
