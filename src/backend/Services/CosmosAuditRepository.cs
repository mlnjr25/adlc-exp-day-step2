using Microsoft.Azure.Cosmos;
using OuterloopLabApi.Models;

namespace OuterloopLabApi.Services;

public sealed class CosmosAuditRepository : IAuditRepository
{
  private readonly Container _container;

  public CosmosAuditRepository(Container container)
  {
    _container = container;
  }

  public async Task CreateAsync(ConversionAuditRecord record, CancellationToken ct)
  {
    // Append-only contract: this endpoint creates only.
    await _container.CreateItemAsync(record, new PartitionKey(record.id), cancellationToken: ct);
  }

  public async Task<ConversionAuditRecord?> GetAsync(string auditId, CancellationToken ct)
  {
    try
    {
      var response = await _container.ReadItemAsync<ConversionAuditRecord>(auditId, new PartitionKey(auditId), cancellationToken: ct);
      return response.Resource;
    }
    catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
      return null;
    }
  }

  public async Task<IReadOnlyList<ConversionAuditRecord>> ListRecentAsync(int limit, CancellationToken ct)
  {
    var query = new QueryDefinition(
      "SELECT TOP @limit * FROM c ORDER BY c.backendExecutionTimestampUtcIso DESC")
      .WithParameter("@limit", limit);

    using var iterator = _container.GetItemQueryIterator<ConversionAuditRecord>(query);

    var results = new List<ConversionAuditRecord>(limit);
    while (iterator.HasMoreResults)
    {
      var page = await iterator.ReadNextAsync(ct);
      results.AddRange(page);
    }

    return results;
  }
}
