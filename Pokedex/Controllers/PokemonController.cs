using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Pokedex.models;

namespace Pokedex.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PokemonController : ControllerBase
    {
        private readonly ILogger<PokemonController> _logger;
        private readonly HttpClient _pokeHttpClient;
        private readonly HttpClient _funTranslationsHttpClient;
        private readonly IConfiguration _configuration;

        public PokemonController(ILogger<PokemonController> logger, IHttpClientFactory clientFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _pokeHttpClient = clientFactory.CreateClient(Constants.PokeApiClientName);
            _funTranslationsHttpClient = clientFactory.CreateClient(Constants.FunTranslationsClientName);
            _configuration = configuration;
        }


        /// <summary>
        /// Get the basic info of a Pokemon by passing it's name
        /// </summary>
        /// <param name="name">case insensitive pokemon name</param>
        /// <returns>basic info of a Pokemon</returns>
        [HttpGet("{name}")]
        public async Task<ActionResult<Response>> PokemonBasicInfo(string name)
        {
            name = name.ToLower();
            var (statusCode, apiResp) = await GetPokemonSpecies(name);
            return apiResp != null
                ? Ok(new Response()
                {
                    Name = name,
                    Description = apiResp.FlavorTextEntries
                        ?.FirstOrDefault(x => x.Language.Name.Equals(Constants.En, StringComparison.OrdinalIgnoreCase))
                        ?.FlavorText,
                    Habitat = apiResp.Habitat?.Name,
                    IsLegendary = apiResp.IsLegendary
                })
                : StatusCode(statusCode);
        }

        /// <summary>
        /// Get the translated info of a Pokemon by passing it's name
        /// Two translation API is used based on the Pokemon type that is defined based on their habitat or being legendary
        /// A cave habitat pokemon or a legendary one uses the Yoda translation for description and the others use Shakespeare translation  
        /// if any problem occurs in the translation api, the original description will be used
        /// </summary>
        /// <param name="name">case insensitive pokemon name</param>
        /// <returns>translated info of a Pokemon</returns>
        [HttpGet("translated/{name}")]
        public async Task<ActionResult<Response>> PokemonTranslatedInfo(string name)
        {
            name = name.ToLower();
            var (statusCode, apiResp) = await GetPokemonSpecies(name);
            if (apiResp == null)
                return StatusCode(statusCode);
            var description = apiResp.FlavorTextEntries
                ?.FirstOrDefault(x => x.Language.Name.Equals(Constants.En, StringComparison.OrdinalIgnoreCase))
                ?.FlavorText;
            FunTranslationsResponse funTranslationsResponse;
            if (ShouldGetYoda(apiResp))
                (statusCode, funTranslationsResponse) = await GetYodaFunTranslate(description);
            else
                (statusCode, funTranslationsResponse) = await GetShakespeareFunTranslate(description);

            _logger.LogTrace($"Fun translation StatusCode {statusCode} on Pokemon: {name}");

            if (!string.IsNullOrEmpty(funTranslationsResponse?.Contents?.Translated))
                description = Uri.UnescapeDataString(funTranslationsResponse.Contents.Translated);

            return Ok(new Response()
            {
                Name = name,
                Description = description,
                Habitat = apiResp.Habitat?.Name,
                IsLegendary = apiResp.IsLegendary
            });
        }

        /// <summary>
        /// Find out if should get translated by Yoda or not based on cave & legendary params 
        /// </summary>
        /// <param name="pokemon">Pokemon info by PokemonApiResponse type</param>
        /// <returns>true if should be translated by Yoda</returns>
        private static bool ShouldGetYoda(PokemonApiResponse pokemon)
        {
            var habitat = pokemon.Habitat?.Name;
            var isLegendary = pokemon.IsLegendary;
            return habitat == "cave" || isLegendary;
        }


        /// <summary>
        /// Call Pokemon species API
        /// </summary>
        /// <param name="name">case sensitive pokemon name</param>
        /// <returns>a tuple of status code and pokemon info, if the status code is not successful then null</returns>
        private async Task<(int, PokemonApiResponse)> GetPokemonSpecies(string name)
        {
            var pokeApiClientSettings = _configuration.GetSection(Constants.PokeApiClientName)
                .Get<PokeApiClientSettings>();

            using var response = await _pokeHttpClient.GetAsync(string.Format(pokeApiClientSettings.SpeciesUri, name));
            var statusCode = (int) response.StatusCode;
            if (!response.IsSuccessStatusCode) return (statusCode, null);
            var responseString = await response.Content.ReadAsStringAsync();
            return (statusCode, JsonConvert.DeserializeObject<PokemonApiResponse>(responseString));
        }

        /// <summary>
        /// Gets the Yoda translation of input text
        /// </summary>
        /// <param name="text">input text to be translated</param>
        /// <returns>a tuple of status code and translation object, if the status code is not successful then null</returns>
        private async Task<(int, FunTranslationsResponse)> GetYodaFunTranslate(string text)
        {
            var funTranslationApiClientSettings = _configuration.GetSection(Constants.FunTranslationsClientName)
                .Get<FunTranslationsClientSettings>();
            return await GetFunTranslate(text, funTranslationApiClientSettings.YodaUri);
        }

        /// <summary>
        /// Gets the Shakespeare translation of input text
        /// </summary>
        /// <param name="text">input text to be translated</param>
        /// <returns>a tuple of status code and translation object, if the status code is not successful then null</returns>
        private async Task<(int, FunTranslationsResponse)> GetShakespeareFunTranslate(string text)
        {
            var funTranslationApiClientSettings = _configuration.GetSection(Constants.FunTranslationsClientName)
                .Get<FunTranslationsClientSettings>();
            return await GetFunTranslate(text, funTranslationApiClientSettings.ShakespeareUri);
        }

        /// <summary>
        /// Gets the translation of input text
        /// </summary>
        /// <param name="text">input text to be translated</param>
        /// <param name="uri">sub-url of the translation</param>
        /// <returns>a tuple of status code and translation object, if the status code is not successful then null</returns>
        private async Task<(int, FunTranslationsResponse)> GetFunTranslate(string text, string uri)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(uri)) return (400, null);
            var queryParam = $"?text={Uri.EscapeDataString(Uri.EscapeDataString(text))}";
            using var response = await _funTranslationsHttpClient.GetAsync(uri + queryParam);
            var statusCode = (int) response.StatusCode;
            if (!response.IsSuccessStatusCode) return (statusCode, null);
            var responseString = await response.Content.ReadAsStringAsync();
            return (statusCode, JsonConvert.DeserializeObject<FunTranslationsResponse>(responseString));
        }
    }
}
