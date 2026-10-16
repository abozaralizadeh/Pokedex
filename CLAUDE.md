# CLAUDE.md

Guidance for Claude Code (and humans) working in this repository. [README.md](README.md) is the
user-facing page.

## What this is

Pokedex is a small ASP.NET Core Web API (`net5.0`) with two endpoints. `GET /pokemon/{name}` looks
a species up on PokeAPI and returns its name, first English description, habitat and legendary
flag. `GET /pokemon/translated/{name}` returns the same, with the description sent through the
FunTranslations API: Yoda for legendary or cave-habitat Pokemon, Shakespeare for the rest.
Outbound calls use named `HttpClient`s with Polly retry, circuit-breaker and timeout policies.
There is no database and no cache. `Pokedex.Test` is an xUnit project that tests the controller
against recorded API responses.

## Commands

```bash
dotnet build Pokedex/Pokedex.sln       # the solution lives in Pokedex/ and includes ../Pokedex.Test
dotnet test Pokedex/Pokedex.sln        # xUnit + Moq, no network (every upstream call is mocked)
dotnet run --project Pokedex           # http://localhost:5000, Swagger UI at /swagger (Development profile)
docker build -t pokedex Pokedex        # build context is the project folder, not the repo root
docker run -d -p 8080:80 pokedex       # runs as Production: no Swagger
```

## Layout

```
Pokedex/                          the web API project (and Pokedex.sln)
  Program.cs, Startup.cs          host; DI: controllers, Swagger (Development only), Polly registry, HttpClients
  Constants.cs                    HttpClient names, config section names, policy registry keys
  Controllers/PokemonController.cs  both endpoints; Yoda vs Shakespeare is ShouldGetYoda()
  Extensions/ServiceCollectionExtensions.cs  AddPolicyRegistryServices, AddHttpClients, AddGlobalPolicyHandlers
  models/                         settings classes, upstream DTOs (Newtonsoft attributes), the API's Response
  appsettings.json                upstream URLs and Polly settings
  Dockerfile                      multi-stage Release image (sdk:5.0 -> aspnet:5.0)
  Dockerfile.develop, azds.yaml, charts/pokedex/   Azure Dev Spaces / Helm scaffolding from Visual Studio
Pokedex.Test/
  PokemonControllerTest.cs        controller + mocked IHttpClientFactory; all mock setups are in the constructor
  ResponseSamples/*.json          recorded PokeAPI and FunTranslations responses (mewtwo, ditto, druddigon)
  appsettings.Test.json           configuration used by the tests
```

## Configuration

Settings come from `appsettings*.json` plus the standard ASP.NET Core sources (environment
variables use `__` between section and key, e.g. `PollySettings__GlobalTimeOut`).

- `PokeApiClient`: `BaseUrl`, `SpeciesUri` (format string, `{0}` is the lower-cased name).
- `FunTranslationsClient`: `BaseUrl`, `YodaUri`, `ShakespeareUri`.
- `PollySettings`: `GlobalTimeOut` (seconds), `Retry` (back-off delays, seconds),
  `DurationOfBreak` (seconds), `HandledEventsAllowedBeforeBreaking`.
- `ASPNETCORE_ENVIRONMENT`: Swagger and the developer exception page exist only in `Development`,
  which `Properties/launchSettings.json` sets and the Docker image does not.

`BaseUrl` and `PollySettings` are read once at startup (`ServiceCollectionExtensions`); the `*Uri`
values are read from `IConfiguration` on every request.

## Conventions / gotchas

- Responses are serialised by System.Text.Json (camelCase: `name`, `description`, `habitat`,
  `isLegendary`). Newtonsoft.Json is only used to read upstream responses into `models/`.
- The name is lower-cased before calling PokeAPI. If PokeAPI answers with an error, the endpoint
  returns that status code with an empty body (404 for an unknown name).
- Translation is best effort: any FunTranslations failure (the public API is rate-limited) falls
  back to the untranslated description, and the status is only logged at Trace level.
- The description is PokeAPI's first English flavor text as is; it can contain `\n` and form feeds.
- `PollySettings` is effectively required: without it no policies are registered, yet
  `AddGlobalPolicyHandlers()` still looks them up by key. Handlers are added retry, circuit breaker,
  timeout, so the timeout applies per attempt. The retry lambda's variable is named `ms`, but the
  values are seconds.
- Tests: the mocked factory returns one `HttpClient` for both client names, and setups match on
  URL substrings plus the form body's `Content-Length`. A new test Pokemon needs a species JSON and
  both translation JSONs in `ResponseSamples/`, a `CopyToOutputDirectory` entry in
  `Pokedex.Test.csproj`, and setups in the test constructor.
- Tests open fixtures with Windows-style relative paths (`"ResponseSamples\\..."`). .NET on
  Linux/macOS treats `\` as part of the file name, so those reads fail there; run the tests on
  Windows or change the paths.
- `net5.0` is out of support. Use the .NET 5 SDK or the `sdk:5.0` image; newer SDKs can build it,
  but running it needs the 5.0 runtime.
- The Helm chart's ingress template emits `networking.k8s.io/v1beta1`, which current Kubernetes no
  longer serves (ingress is disabled by default in `values.yaml`).
- `.dockerignore` excludes `charts/`, `azds.yaml` and `Dockerfile*`; the test project is outside
  the Docker build context.
