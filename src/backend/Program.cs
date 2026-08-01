using Azure.Identity;
using Microsoft.Azure.Cosmos;
using OuterloopLabApi.Models;
using OuterloopLabApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

// ---- Configuration (env-var only) ----
var cosmosConfig = CosmosConfig.FromEnvironment();

// Required: Managed Identity token-only auth (no connection strings / account keys)
var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
{
  ManagedIdentityClientId = cosmosConfig.ManagedIdentityClientId
});

// Best-effort ARM provisioning (control plane). Data-plane provisioning is mandatory.
await CosmosProvisioner.ProvisionBestEffortAsync(cosmosConfig, credential);

// ---- Data-plane provisioning (token authenticated) ----
var cosmosClient = new CosmosClient(cosmosConfig.CosmosDbUri, credential);

// If data-plane create-if-not-exists fails, startup must fail.
var databaseResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(cosmosConfig.CosmosDbDatabase);
await databaseResponse.Database.CreateContainerIfNotExistsAsync(
  cosmosConfig.CosmosDbContainer,
  "/id"
);

var container = cosmosClient.GetContainer(cosmosConfig.CosmosDbDatabase, cosmosConfig.CosmosDbContainer);

builder.Services.AddSingleton(container);
builder.Services.AddSingleton<IAuditRepository, CosmosAuditRepository>();

// ---- FX provider ----
builder.Services.AddHttpClient<FxProviderClient>(client =>
{
  var baseUrl = Environment.GetEnvironmentVariable("CURRENCY_API_BASE_URL")
               ?.Trim();
  if (string.IsNullOrWhiteSpace(baseUrl))
  {
    baseUrl = "https://frankfurter.dev";
  }

  client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
  client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddSingleton<IExchangeRateProvider, FrankfurterFxProvider>();
builder.Services.AddSingleton<IConversionService, ConversionService>();

builder.Services.AddSingleton<JsonSerializerOptions>(_ => new JsonSerializerOptions
{
  PropertyNameCaseInsensitive = true
});

var app = builder.Build();

app.MapControllers();

app.Run();
