using OuterloopLabApi.Models;
using OuterloopLabApi.Services;

namespace OuterloopLabApi.Tests;

public sealed class ConversionServiceTests
{
  private sealed class FakeProvider : IExchangeRateProvider
  {
    private readonly FxQuote _quote;

    public FakeProvider(FxQuote quote) => _quote = quote;

    public Task<FxQuote> GetQuoteAsync(string fromCurrency, string toCurrency, CancellationToken ct)
      => Task.FromResult(_quote);
  }

  private sealed class CapturingAuditRepository : IAuditRepository
  {
    public ConversionAuditRecord? LastRecord { get; private set; }

    public Task CreateAsync(ConversionAuditRecord record, CancellationToken ct)
    {
      LastRecord = record;
      return Task.CompletedTask;
    }

    public Task<ConversionAuditRecord?> GetAsync(string auditId, CancellationToken ct)
      => Task.FromResult<ConversionAuditRecord?>(null);

    public Task<IReadOnlyList<ConversionAuditRecord>> ListRecentAsync(int limit, CancellationToken ct)
      => Task.FromResult<IReadOnlyList<ConversionAuditRecord>>(Array.Empty<ConversionAuditRecord>());
  }

  [Fact]
  public async Task ConvertAsync_PersistsAuditRecord_AndReturnsResponse()
  {
    var providerDate = "2026-08-01";
    var providerMarkers = new Dictionary<string, string> { { "date", providerDate } };
    var quote = new FxQuote
    {
      Rate = 0.9200m,
      ProviderDate = providerDate,
      ProviderMarkers = providerMarkers
    };

    var repo = new CapturingAuditRepository();
    var service = new ConversionService(new FakeProvider(quote), repo);

    var response = await service.ConvertAsync(100.00m, "USD", "EUR", CancellationToken.None);

    Assert.NotNull(repo.LastRecord);
    var record = repo.LastRecord!;

    Assert.False(string.IsNullOrWhiteSpace(record.id));
    Assert.Equal(record.id, response.AuditId);

    Assert.Equal(100.00m, record.requestedAmount);
    Assert.Equal("USD", record.fromCurrency);
    Assert.Equal("EUR", record.toCurrency);

    Assert.Equal(0.9200m, record.rate);
    Assert.Equal(92.0000m, record.convertedAmount);

    Assert.Equal(providerDate, record.providerDate);
    Assert.Equal(providerMarkers, record.providerMarkers);

    Assert.Equal(record.backendExecutionTimestampUtc, response.BackendExecutionTimestampUtc);
    Assert.False(string.IsNullOrWhiteSpace(record.backendExecutionTimestampUtcIso));
  }
}
