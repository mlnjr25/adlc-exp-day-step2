using Microsoft.AspNetCore.Mvc;
using OuterloopLabApi.Controllers;
using OuterloopLabApi.Models;
using OuterloopLabApi.Services;

namespace OuterloopLabApi.Tests;

public sealed class ConversionsControllerTests
{
  private sealed class FakeAuditRepository : IAuditRepository
  {
    public Task CreateAsync(ConversionAuditRecord record, CancellationToken ct) => Task.CompletedTask;
    public Task<ConversionAuditRecord?> GetAsync(string auditId, CancellationToken ct) => Task.FromResult<ConversionAuditRecord?>(null);
    public Task<IReadOnlyList<ConversionAuditRecord>> ListRecentAsync(int limit, CancellationToken ct)
      => Task.FromResult<IReadOnlyList<ConversionAuditRecord>>(Array.Empty<ConversionAuditRecord>());
  }

  private sealed class FakeFailureService : IConversionService
  {
    public Task<ConversionResponse> ConvertAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken ct)
      => throw new ProviderUnavailableException("boom");
  }

  [Fact]
  public async Task Convert_WhenRequestInvalid_Returns400ProblemDetails()
  {
    var controller = new ConversionsController(
      new FakeFailureService(),
      new FakeAuditRepository());

    var result = await controller.Convert(
      new ConversionRequest { Amount = 0, FromCurrency = "usd", ToCurrency = "EUR" },
      CancellationToken.None);

    var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
    var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
    Assert.Equal(400, problem.Status);
  }

  [Fact]
  public async Task Convert_WhenProviderFails_Returns503ProblemDetailsWithoutRawExceptionText()
  {
    var controller = new ConversionsController(
      new FakeFailureService(),
      new FakeAuditRepository());

    var result = await controller.Convert(
      new ConversionRequest { Amount = 100m, FromCurrency = "USD", ToCurrency = "EUR" },
      CancellationToken.None);

    var objectResult = Assert.IsType<ObjectResult>(result.Result);
    Assert.Equal(503, objectResult.StatusCode);

    var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
    Assert.Equal(503, problem.Status);
    Assert.NotNull(problem.Detail);
    Assert.DoesNotContain("boom", problem.Detail!, StringComparison.OrdinalIgnoreCase);
  }
}
