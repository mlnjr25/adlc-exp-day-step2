namespace OuterloopLabApi.Services;

public sealed class CosmosConfig
{
  public string CosmosDbUri { get; init; } = string.Empty;
  public string CosmosDbDatabase { get; init; } = string.Empty;
  public string CosmosDbContainer { get; init; } = string.Empty;
  public string CosmosDbAccountName { get; init; } = string.Empty;
  public string CosmosDbResourceGroup { get; init; } = string.Empty;
  public string CosmosDbRegion { get; init; } = string.Empty;
  public string ManagedIdentityClientId { get; init; } = string.Empty;

  public static CosmosConfig FromEnvironment()
  {
    string Require(string key)
    {
      var v = Environment.GetEnvironmentVariable(key);
      if (string.IsNullOrWhiteSpace(v))
      {
        throw new InvalidOperationException($"Missing required environment variable: {key}");
      }
      return v;
    }

    return new CosmosConfig
    {
      CosmosDbUri = Require("COSMOS_DB_URI"),
      CosmosDbDatabase = Require("COSMOS_DB_DATABASE"),
      CosmosDbContainer = Require("COSMOS_DB_CONTAINER"),
      CosmosDbAccountName = Require("COSMOS_DB_ACCOUNT_NAME"),
      CosmosDbResourceGroup = Require("COSMOS_DB_RESOURCE_GROUP"),
      CosmosDbRegion = Require("COSMOS_DB_REGION"),
      ManagedIdentityClientId = Require("AZURE_MANAGED_IDENTITY_CLIENT_ID")
    };
  }
}
