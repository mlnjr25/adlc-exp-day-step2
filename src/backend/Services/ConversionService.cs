using OuterloopLabApi.Models;

namespace OuterloopLabApi.Services;

public sealed class ConversionService : IConversionService
{
  private readonly IExchangeRateProvider _exchangeRateProvider;
  private readonly IAuditRepository _auditRepository;

  public ConversionService(
    IExchangeRateProvider exchangeRateProvider,
    IAuditRepository auditRepository)
  {
    _exchangeRateProvider = exchangeRateProvider;
    _auditRepository = auditRepository;
  }

  public async Task<ConversionResponse> ConvertAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken ct)
  {
    var quote = await _exchangeRateProvider.GetQuoteAsync(fromCurrency, toCurrency, ct);

    var convertedAmount = amount * quote.Rate;
    var auditId = Guid.NewGuid().ToString("N");
    var nowUtc = DateTime.UtcNow;

    var record = new ConversionAuditRecord
    {
      id = auditId,
      requestedAmount = amount,
      fromCurrency = fromCurrency,
      toCurrency = toCurrency,
      rate = quote.Rate,
      convertedAmount = convertedAmount,
      providerDate = quote.ProviderDate,
      providerMarkers = new Dictionary<string, string>(quote.ProviderMarkers),
      backendExecutionTimestampUtc = nowUtc,
      backendExecutionTimestampUtcIso = nowUtc.ToString("O")
    };

    await _auditRepository.CreateAsync(record, ct);

    return ConversionResponse.FromRecord(record);
  }
}
