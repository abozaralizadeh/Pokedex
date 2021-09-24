using System;
using Moq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq.Protected;
using Newtonsoft.Json;
using Pokedex.Controllers;
using Pokedex.models;
using Xunit;

namespace Pokedex.Test
{
    public class PokemonControllerTest
    {
        public PokemonController PokemonController;

        private readonly string _yodaTranslationMewTwo;
        private readonly string _yodaTranslationDruddigon;
        private readonly string _shakespeareTranslationDitto;

        public PokemonControllerTest()
        {
            // Arrange
            var configuration = CreateConfiguration();
            var mockedLogger = new Mock<ILogger<PokemonController>>();
            var logger = mockedLogger.Object;

            var pokeApiClientSettings = configuration.GetSection(Constants.PokeApiClientName)
                .Get<PokeApiClientSettings>();
            var funTranslationApiClientSettings = configuration.GetSection(Constants.FunTranslationsClientName)
                .Get<FunTranslationsClientSettings>();

            var httpClientFactory = new Mock<IHttpClientFactory>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            #region MewTwo Setup

            var pokemonMewTwoApiResp = File.ReadAllText("ResponseSamples\\PokemonMewTwoApiResp.json");
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(
                        r => r.RequestUri.ToString().Contains(UnitTestConstants.PokemonMewTwo)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK, Content = new StringContent(pokemonMewTwoApiResp)
                });

            var funTranslationMewTwoYodaApiResponse =
                File.ReadAllText("ResponseSamples\\FunTranslationMewTwoYodaApiResponse.json");
            var funTranslate1 =
                JsonConvert.DeserializeObject<FunTranslationsResponse>(funTranslationMewTwoYodaApiResponse);
            _yodaTranslationMewTwo = funTranslate1?.Contents.Translated;
            var mewtwoText = funTranslate1?.Contents.Text;
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.RequestUri.ToString().Contains(funTranslationApiClientSettings.YodaUri) && r.Content
                            .Headers.ContentLength
                            .Equals(new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                                {new("text", mewtwoText)}).Headers.ContentLength)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK, Content = new StringContent(funTranslationMewTwoYodaApiResponse)
                });

            var funTranslationMewTwoShakespeareApiResponse =
                File.ReadAllText("ResponseSamples\\FunTranslationMewTwoShakespeareApiResponse.json");
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.RequestUri.ToString().Contains(funTranslationApiClientSettings.ShakespeareUri) && r.Content
                            .Headers.ContentLength
                            .Equals(new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                                {new("text", mewtwoText)}).Headers.ContentLength)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(funTranslationMewTwoShakespeareApiResponse)
                });

            #endregion

            #region Druddigon Setup

            var pokemonDruddigonApiResp = File.ReadAllText("ResponseSamples\\PokemonDruddigonApiResp.json");
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.RequestUri.ToString().Contains(UnitTestConstants.PokemonDruddigon)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK, Content = new StringContent(pokemonDruddigonApiResp)
                });

            var funTranslationDruddigonYodaApiResponse =
                File.ReadAllText("ResponseSamples\\FunTranslationDruddigonYodaApiResponse.json");
            var funTranslate2 =
                JsonConvert.DeserializeObject<FunTranslationsResponse>(funTranslationDruddigonYodaApiResponse);
            _yodaTranslationDruddigon = funTranslate2?.Contents.Translated;
            var druddigonText = funTranslate2?.Contents.Text;
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.RequestUri.ToString().Contains(funTranslationApiClientSettings.YodaUri) && r.Content
                            .Headers.ContentLength
                            .Equals(new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                                {new("text", druddigonText)}).Headers.ContentLength)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(funTranslationDruddigonYodaApiResponse)
                });

            var funTranslationDruddigonShakespeareApiResponse =
                File.ReadAllText("ResponseSamples\\FunTranslationDruddigonShakespeareApiResponse.json");
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.RequestUri.ToString().Contains(funTranslationApiClientSettings.ShakespeareUri) && r.Content
                            .Headers.ContentLength
                            .Equals(new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                                {new("text", druddigonText)}).Headers.ContentLength)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(funTranslationDruddigonShakespeareApiResponse)
                });

            #endregion

            #region Ditto Setup

            var pokemonDittoApiResp = File.ReadAllText("ResponseSamples\\PokemonDittoApiResp.json");
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.RequestUri.ToString().Contains(UnitTestConstants.PokemonDitto)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK, Content = new StringContent(pokemonDittoApiResp)
                });

            var funTranslationDittoYodaApiResponse =
                File.ReadAllText("ResponseSamples\\FunTranslationDittoYodaApiResponse.json");
            var funTranslate4 =
                JsonConvert.DeserializeObject<FunTranslationsResponse>(funTranslationDittoYodaApiResponse);
            var dittoText = funTranslate4?.Contents.Text;
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.RequestUri.ToString().Contains(funTranslationApiClientSettings.YodaUri) && r.Content
                            .Headers.ContentLength
                            .Equals(new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                                {new("text", dittoText)}).Headers.ContentLength)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK, Content = new StringContent(funTranslationDittoYodaApiResponse)
                });

            var funTranslationDittoShakespeareApiResponse =
                File.ReadAllText("ResponseSamples\\FunTranslationDittoShakespeareApiResponse.json");
            var funTranslate3 =
                JsonConvert.DeserializeObject<FunTranslationsResponse>(funTranslationDittoShakespeareApiResponse);
            _shakespeareTranslationDitto = funTranslate3?.Contents.Translated;
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.RequestUri.ToString().Contains(funTranslationApiClientSettings.ShakespeareUri) && r.Content
                            .Headers.ContentLength
                            .Equals(new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                                {new("text", dittoText)}).Headers.ContentLength)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(funTranslationDittoShakespeareApiResponse)
                });

            #endregion

            #region NotFound Setup

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString().Contains(UnitTestConstants.RandomName)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound, Content = new StringContent(string.Empty)
                });

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        !r.RequestUri.ToString().Contains(funTranslationApiClientSettings.ShakespeareUri) &&
                        !r.RequestUri.ToString().Contains(funTranslationApiClientSettings.YodaUri) &&
                        !r.RequestUri.ToString().Contains(pokeApiClientSettings.BaseUrl)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound, Content = new StringContent(string.Empty)
                });

            #endregion

            var client = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri(pokeApiClientSettings.BaseUrl)
            };
            httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

            PokemonController = new PokemonController(logger, httpClientFactory.Object, configuration);
        }

        [Fact]
        public async Task PokemonBasicInfoSuccessTestAsync()
        {
            // Act
            var actionResult = await PokemonController.PokemonBasicInfo(UnitTestConstants.PokemonMewTwo);
            var result = actionResult.Result as ObjectResult;

            // Assert
            Assert.IsType<ActionResult<Response>>(actionResult);
            Assert.NotNull(result);
            Assert.NotEmpty(((Response) result.Value).Name);
            Assert.NotEmpty(((Response) result.Value).Description);
            Assert.NotEmpty(((Response) result.Value).Habitat);
            Assert.True(((Response) result.Value).IsLegendary);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task PokemonBasicInfoFailTestAsync()
        {
            // Act
            var actionResult = await PokemonController.PokemonBasicInfo(UnitTestConstants.RandomName);
            var result = actionResult.Result as StatusCodeResult;

            // Assert
            Assert.IsType<ActionResult<Response>>(actionResult);
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task PokemonTranslatedInfoSuccessTestAsync()
        {
            // Act
            var actionResult = await PokemonController.PokemonTranslatedInfo(UnitTestConstants.PokemonMewTwo);
            var result = actionResult.Result as ObjectResult;

            // Assert
            Assert.IsType<ActionResult<Response>>(actionResult);
            Assert.NotNull(result);
            Assert.NotEmpty(((Response) result.Value).Name);
            Assert.NotEmpty(((Response) result.Value).Description);
            Assert.NotEmpty(((Response) result.Value).Habitat);
            Assert.True(((Response) result.Value).IsLegendary);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task PokemonTranslatedInfoFailTestAsync()
        {
            // Act
            var actionResult = await PokemonController.PokemonTranslatedInfo(UnitTestConstants.RandomName);
            var result = actionResult.Result as StatusCodeResult;

            // Assert
            Assert.IsType<ActionResult<Response>>(actionResult);
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task ShouldGetTheYodaTranslationForLegendaryPokemonTestAsync()
        {
            // Act
            var actionResult = await PokemonController.PokemonTranslatedInfo(UnitTestConstants.PokemonMewTwo);
            var result = actionResult.Result as ObjectResult;

            // Assert
            Assert.True(((Response) result?.Value)?.IsLegendary);
            Assert.Equal(_yodaTranslationMewTwo, ((Response) result?.Value)?.Description);
        }

        [Fact]
        public async Task ShouldGetTheYodaTranslationForCavePokemonTestAsync()
        {
            // Act
            var actionResult = await PokemonController.PokemonTranslatedInfo(UnitTestConstants.PokemonDruddigon);
            var result = actionResult.Result as ObjectResult;

            // Assert
            Assert.Equal(UnitTestConstants.Cave, ((Response) result?.Value)?.Habitat);
            Assert.Equal(_yodaTranslationDruddigon, ((Response) result?.Value)?.Description);
        }

        [Fact]
        public async Task ShouldGetTheShakespreareTranslationForNormalPokemonTestAsync()
        {
            // Act
            var actionResult = await PokemonController.PokemonTranslatedInfo(UnitTestConstants.PokemonDitto);
            var result = actionResult.Result as ObjectResult;

            // Assert
            Assert.False(((Response) result?.Value)?.IsLegendary);
            Assert.NotEqual(UnitTestConstants.Cave, ((Response) result?.Value)?.Habitat);
            Assert.Equal(_shakespeareTranslationDitto, ((Response) result?.Value)?.Description);
        }

        private static IConfiguration CreateConfiguration() =>
            new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string>() {{"InstrumentationKey", null}, {"AllowedHosts", null},})
                .AddJsonFile("appsettings.Test.json")
                .AddEnvironmentVariables()
                .Build();
    }
}