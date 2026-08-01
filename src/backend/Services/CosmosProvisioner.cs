using Azure.Core;
using Azure.ResourceManager;

namespace OuterloopLabApi.Services;

public static class CosmosProvisioner
{
  // Best-effort ARM provisioning is explicitly allowed to fail; data-plane must still be enforced.
  public static async Task ProvisionBestEffortAsync(CosmosConfig config, Azure.Core.TokenCredential credential)
  {
    try
    {
      var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
      if (string.IsNullOrWhiteSpace(subscriptionId))
      {
        return;
      }

      // Control plane uses Azure.ResourceManager.
      var armClient = new ArmClient(credential, subscriptionId);

      // We intentionally use generic ARM resources to avoid tightly coupling to exact SDK model signatures.
      // Runtime failures here are allowed; data-plane provisioning enforces correctness.
      var dbResourceId = new ResourceIdentifier(
        $"/subscriptions/{subscriptionId}/resourceGroups/{config.CosmosDbResourceGroup}/providers/Microsoft.DocumentDB/databaseAccounts/{config.CosmosDbAccountName}/sqlDatabases/{config.CosmosDbDatabase}");

      var containerResourceId = new ResourceIdentifier(
        $"/subscriptions/{subscriptionId}/resourceGroups/{config.CosmosDbResourceGroup}/providers/Microsoft.DocumentDB/databaseAccounts/{config.CosmosDbAccountName}/sqlDatabases/{config.CosmosDbDatabase}/containers/{config.CosmosDbContainer}");

      var dbResource = armClient.GetGenericResource(dbResourceId);
      var containerResource = armClient.GetGenericResource(containerResourceId);

      // Minimal payload; ARM will validate and/or reject depending on account configuration.
      await dbResource.CreateOrUpdateAsync(
        WaitUntil.Completed,
        new BinaryData(new { id = config.CosmosDbDatabase }));

      await containerResource.CreateOrUpdateAsync(
        WaitUntil.Completed,
        new BinaryData(new
        {
          id = config.CosmosDbContainer,
          partitionKey = new { paths = new[] { "/id" }, kind = "Hash" }
        }));
    }
    catch
    {
      // Best-effort only. Data-plane create-if-not-exists will enforce correctness.
    }
  }
}
