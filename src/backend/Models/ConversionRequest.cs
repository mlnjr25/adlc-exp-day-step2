using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace OuterloopLabApi.Models;

public sealed class ConversionRequest
{
  public decimal Amount { get; set; }
  public string FromCurrency { get; set; } = string.Empty;
  public string ToCurrency { get; set; } = string.Empty;

  public ProblemDetails? Validate()
  {
    if (Amount <= 0)
    {
      return new ProblemDetails
      {
        Status = 400,
        Title = "Validation error",
        Detail = "amount must be greater than 0."
      };
    }

    var from = NormalizeCurrency(FromCurrency);
    var to = NormalizeCurrency(ToCurrency);
    if (from is null || to is null)
    {
      return new ProblemDetails
      {
        Status = 400,
        Title = "Validation error",
        Detail = "fromCurrency and toCurrency must be exactly three alphabetic characters."
      };
    }

    FromCurrency = from;
    ToCurrency = to;
    return null;
  }

  private static string? NormalizeCurrency(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      return null;
    }

    var normalized = value.Trim().ToUpperInvariant();
    if (!Regex.IsMatch(normalized, "^[A-Z]{3}$"))
    {
      return null;
    }

    return normalized;
  }
}
