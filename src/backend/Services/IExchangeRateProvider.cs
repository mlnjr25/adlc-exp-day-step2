namespace OuterloopLabApi.Services;

public interface IExchangeRateProvider
{
  Task<FxQuote> GetQuoteAsync(string fromCurrency, string toCurrency, CancellationToken ct);
}
