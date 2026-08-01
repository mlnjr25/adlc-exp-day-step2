using OuterloopLabApi.Models;

namespace OuterloopLabApi.Services;

public interface IConversionService
{
  Task<ConversionResponse> ConvertAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken ct);
}
