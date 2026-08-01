using System.Text.Json;

namespace OuterloopLabApi.Services;

public sealed class FrankfurterFxProvider : IExchangeRateProvider
{
  private readonly FxProviderClient _client;

  public FrankfurterFxProvider(FxProviderClient client)
  {
    _client = client;
  }

  public async Task<FxQuote> GetQuoteAsync(string fromCurrency, string toCurrency, CancellationToken ct)
  {
    try
    {
      using var document = await _client.GetLatestAsync(fromCurrency, toCurrency, ct);
      return FxProviderResponseAdapter.Normalize(document, fromCurrency, toCurrency);
    }
    catch (ProviderUnavailableException)
    {
      throw;
    }
    catch (OperationCanceledException)
    {
      throw new ProviderUnavailableException("FX provider request timed out.");
    }
    catch (Exception)
    {
      // Domain-safe failure. Avoid leaking raw exception details.
      throw new ProviderUnavailableException("FX provider request failed.");
    }
  }
}
