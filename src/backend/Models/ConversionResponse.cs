namespace OuterloopLabApi.Models;

public sealed class ConversionResponse
{
  public decimal ConvertedAmount { get; set; }
  public decimal Rate { get; set; }
  public string AuditId { get; set; } = string.Empty;
  public string ProviderDate { get; set; } = string.Empty;
  public DateTime BackendExecutionTimestampUtc { get; set; }
  public IReadOnlyDictionary<string, string> ProviderMarkers { get; set; } = new Dictionary<string, string>();

  public static ConversionResponse FromRecord(ConversionAuditRecord record)
  {
    return new ConversionResponse
    {
      AuditId = record.Id,
      ConvertedAmount = record.ConvertedAmount,
      Rate = record.Rate,
      ProviderDate = record.ProviderDate,
      BackendExecutionTimestampUtc = record.BackendExecutionTimestampUtc,
      ProviderMarkers = record.ProviderMarkers
    };
  }
}
