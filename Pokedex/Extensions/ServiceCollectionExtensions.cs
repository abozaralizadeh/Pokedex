using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pokedex.models;
using Polly;
using Polly.Extensions.Http;
using Polly.Registry;
using Polly.Timeout;

namespace Pokedex.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IConfiguration GetConfiguration(this IServiceCollection services)
        {
            return services.BuildServiceProvider().GetService<IConfiguration>();
        }

        public static IServiceCollection AddHttpClients(this IServiceCollection services)
        {
            var config = services.GetConfiguration();

            var pokeApiClientSettings = config.GetSection(Constants.PokeApiClientName)
                .Get<PokeApiClientSettings>();
            services.AddHttpClient(Constants.PokeApiClientName,
                    client =>
                    {
                        client.BaseAddress = new Uri(pokeApiClientSettings.BaseUrl);
                        client.DefaultRequestHeaders.Add("Accept", "application/json");
                    })
                .AddGlobalPolicyHandlers();

            var funTranslationsApiClientSettings =
                config.GetSection(Constants.FunTranslationsClientName)
                    .Get<FunTranslationsClientSettings>();
            services.AddHttpClient(Constants.FunTranslationsClientName,
                    client => client.BaseAddress = new Uri(funTranslationsApiClientSettings.BaseUrl))
                .AddGlobalPolicyHandlers();

            return services;
        }

        public static IServiceCollection AddPolicyRegistryServices(this IServiceCollection services)
        {
            var pollySettings = services.GetConfiguration().GetSection(Constants.PollySettings).Get<PollySettings>();

            if (pollySettings == null) return services;
            var retryTimeSpans = new List<TimeSpan>();
            pollySettings.Retry?.ForEach(ms => retryTimeSpans.Add(TimeSpan.FromSeconds(ms)));
            var globalPolicyRegistry = new PolicyRegistry()
            {
                {
                    Constants.GlobalRetryStrategyPolicyRegistry,
                    HttpPolicyExtensions.HandleTransientHttpError()
                        .Or<TimeoutRejectedException>()
                        .WaitAndRetryAsync(retryTimeSpans.ToArray())
                },
                {
                    Constants.GlobalCircuitBreakerPolicyRegistry,
                    HttpPolicyExtensions.HandleTransientHttpError()
                        .CircuitBreakerAsync(
                            handledEventsAllowedBeforeBreaking: pollySettings.HandledEventsAllowedBeforeBreaking,
                            durationOfBreak: TimeSpan.FromSeconds(pollySettings.DurationOfBreak))
                },
                {
                    Constants.GlobalTimeOutPolicyRegistry,
                    Policy.TimeoutAsync<HttpResponseMessage>(pollySettings.GlobalTimeOut)
                }
            };

            services.AddPolicyRegistry(globalPolicyRegistry);

            return services;
        }

        public static IHttpClientBuilder AddGlobalPolicyHandlers(this IHttpClientBuilder builder)
        {
            return builder.AddPolicyHandlerFromRegistry(Constants.GlobalRetryStrategyPolicyRegistry)
                .AddPolicyHandlerFromRegistry(Constants.GlobalCircuitBreakerPolicyRegistry)
                .AddPolicyHandlerFromRegistry(Constants.GlobalTimeOutPolicyRegistry);
        }

    }
}
