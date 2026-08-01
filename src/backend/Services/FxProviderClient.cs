using System.Text.Json;

namespace OuterloopLabApi.Services;

public sealed class FxProviderClient
{
  private readonly HttpClient _httpClient;

  public FxProviderClient(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }

  public async Task<JsonDocument> GetLatestAsync(string fromCurrency, string toCurrency, CancellationToken ct)
  {
    // Frankfurter-compatible query shape.
    var response = await _httpClient.GetAsync($"latest?base={Uri.EscapeDataString(fromCurrency)}&symbols={Uri.EscapeDataString(toCurrency)}", ct);
    if (!response.IsSuccessStatusCode)
    {
      throw new ProviderUnavailableException("FX provider returned a non-success response.");
    }

    try
    {
      var stream = await response.Content.ReadAsStreamAsync(ct);
      var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
      return document;
    }
    catch (JsonException)
    {
      throw new ProviderUnavailableException("FX provider returned invalid JSON.");
    }
  }
}
