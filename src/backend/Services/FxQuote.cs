namespace OuterloopLabApi.Services;

public sealed class FxQuote
{
  public decimal Rate { get; init; }
  public string ProviderDate { get; init; } = string.Empty;
  public IReadOnlyDictionary<string, string> ProviderMarkers { get; init; } = new Dictionary<string, string>();
}
