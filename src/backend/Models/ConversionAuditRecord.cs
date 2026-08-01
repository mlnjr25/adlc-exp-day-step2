namespace OuterloopLabApi.Models;

public sealed class ConversionAuditRecord
{
  // Cosmos requires an `id` property.
  public string id { get; set; } = string.Empty;

  public decimal requestedAmount { get; set; }
  public string fromCurrency { get; set; } = string.Empty;
  public string toCurrency { get; set; } = string.Empty;

  public decimal rate { get; set; }
  public decimal convertedAmount { get; set; }

  // Provider markers (at minimum provider date; additional extracted markers are stored as strings)
  public string providerDate { get; set; } = string.Empty;
  public Dictionary<string, string> providerMarkers { get; set; } = new();

  public DateTime backendExecutionTimestampUtc { get; set; }
  public string backendExecutionTimestampUtcIso { get; set; } = string.Empty;

  // Convenience (non-serialized mapping)
  public string Id => id;
  public decimal RequestedAmount => requestedAmount;
  public string FromCurrency => fromCurrency;
  public string ToCurrency => toCurrency;
  public decimal Rate => rate;
  public decimal ConvertedAmount => convertedAmount;
  public string ProviderDate => providerDate;
  public IReadOnlyDictionary<string, string> ProviderMarkers => providerMarkers;
  public DateTime BackendExecutionTimestampUtc => backendExecutionTimestampUtc;
}
