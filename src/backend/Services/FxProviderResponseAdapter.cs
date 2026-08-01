using System.Text.Json;

namespace OuterloopLabApi.Services;

public static class FxProviderResponseAdapter
{
  public static FxQuote Normalize(JsonDocument providerPayload, string fromCurrency, string toCurrency)
  {
    var root = providerPayload.RootElement;
    var markers = new Dictionary<string, string>();

    var providerDate = TryGetPropertyStringCaseInsensitive(root, new[] { "date", "day" }) ?? string.Empty;
    if (!string.IsNullOrWhiteSpace(providerDate))
    {
      markers["date"] = providerDate;
    }

    // Collect any simple date/sequence-ish markers to help audit reconstruction.
    // (We avoid hardcoding full provider schema; only extract a small set of known marker names.)
    foreach (var key in new[] { "sequence", "timestamp", "time" })
    {
      var val = TryGetPropertyStringCaseInsensitive(root, new[] { key });
      if (!string.IsNullOrWhiteSpace(val))
      {
        markers[key] = val;
      }
    }

    JsonElement? ratesObject = null;
    foreach (var propertyName in new[] { "rates", "conversion_rates", "conversionRates" })
    {
      if (TryGetProperty(root, propertyName, out var element) && element.ValueKind == JsonValueKind.Object)
      {
        ratesObject = element;
        break;
      }
    }

    if (ratesObject is null)
    {
      throw new ProviderUnavailableException("FX provider response did not include a rates object.");
    }

    var toRate = GetRateFromRatesObject(ratesObject.Value, toCurrency);
    if (toRate is null)
    {
      throw new ProviderUnavailableException("FX provider response did not include a matching target currency rate.");
    }

    return new FxQuote
    {
      Rate = toRate.Value,
      ProviderDate = providerDate,
      ProviderMarkers = markers
    };
  }

  private static decimal? GetRateFromRatesObject(JsonElement ratesObject, string toCurrency)
  {
    // rates object keys are usually currency codes (e.g., "EUR").
    var target = toCurrency.Trim().ToUpperInvariant();
    foreach (var rateProperty in ratesObject.EnumerateObject())
    {
      if (string.Equals(rateProperty.Name, target, StringComparison.OrdinalIgnoreCase))
      {
        if (rateProperty.Value.ValueKind == JsonValueKind.Number)
        {
          return rateProperty.Value.GetDecimal();
        }
        if (rateProperty.Value.ValueKind == JsonValueKind.String && decimal.TryParse(rateProperty.Value.GetString(), out var parsed))
        {
          return parsed;
        }
      }
    }
    return null;
  }

  private static bool TryGetProperty(JsonElement root, string propertyName, out JsonElement element)
  {
    foreach (var prop in root.EnumerateObject())
    {
      if (string.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase))
      {
        element = prop.Value;
        return true;
      }
    }

    element = default;
    return false;
  }

  private static string? TryGetPropertyStringCaseInsensitive(JsonElement root, string[] possibleNames)
  {
    foreach (var possible in possibleNames)
    {
      if (TryGetProperty(root, possible, out var element))
      {
        if (element.ValueKind == JsonValueKind.String)
        {
          return element.GetString();
        }
        if (element.ValueKind == JsonValueKind.Number)
        {
          return element.GetDecimal().ToString();
        }
      }
    }
    return null;
  }
}
