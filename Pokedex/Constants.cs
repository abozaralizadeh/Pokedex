namespace Pokedex
{
    public class Constants
    {
        #region Generic
        public const string ApiTitle = "Pokedex";
        public const string ApiVersion = "v1";
        public const string En = "en";
        #endregion

        #region HttpClients
        public const string PokeApiClientName = "PokeApiClient";
        public const string FunTranslationsClientName = "FunTranslationsClient";
        # endregion

        #region Polly
        public const string PollySettings = "PollySettings";
        public const string GlobalRetryStrategyPolicyRegistry = "GlobalRetryStrategyPolicyRegistry";
        public const string GlobalCircuitBreakerPolicyRegistry = "GlobalCircuitBreakerPolicyRegistry";
        public const string GlobalTimeOutPolicyRegistry = "GlobalTimeOutPolicyRegistry";
        # endregion
    }
}
