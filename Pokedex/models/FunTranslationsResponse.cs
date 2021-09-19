using Newtonsoft.Json;

namespace Pokedex.models
{
    public class FunTranslationsResponse
    {
        [JsonProperty("success")]
        public Success Success { get; set; }

        [JsonProperty("contents")]
        public Contents Contents { get; set; }
    }

    public class Success
    {
        [JsonProperty("total")]
        public int Total { get; set; }
    }

    public class Contents
    {
        [JsonProperty("translated")]
        public string Translated { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("translation")]
        public string Translation { get; set; }
    }
}
