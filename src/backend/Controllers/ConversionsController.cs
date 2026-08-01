using Microsoft.AspNetCore.Mvc;
using OuterloopLabApi.Models;
using OuterloopLabApi.Services;

namespace OuterloopLabApi.Controllers;

[ApiController]
[Route("/api/conversions")]
public class ConversionsController : ControllerBase
{
  private readonly IConversionService _conversionService;
  private readonly IAuditRepository _auditRepository;

  public ConversionsController(
    IConversionService conversionService,
    IAuditRepository auditRepository)
  {
    _conversionService = conversionService;
    _auditRepository = auditRepository;
  }

  [HttpPost]
  public async Task<ActionResult<ConversionResponse>> Convert([FromBody] ConversionRequest request, CancellationToken ct)
  {
    var validation = request.Validate();
    if (validation is not null)
    {
      return BadRequest(validation);
    }

    try
    {
      var result = await _conversionService.ConvertAsync(
        request.Amount,
        request.FromCurrency,
        request.ToCurrency,
        ct);

      return Ok(result);
    }
    catch (ProviderUnavailableException)
    {
      return StatusCode(503, new ProblemDetails
      {
        Status = 503,
        Title = "Currency provider unavailable",
        Detail = "Unable to retrieve and normalize a conversion rate from the configured FX provider."
      });
    }
  }

  [HttpGet("{auditId}")]
  public async Task<ActionResult<ConversionResponse>> GetById([FromRoute] string auditId, CancellationToken ct)
  {
    if (string.IsNullOrWhiteSpace(auditId))
    {
      return BadRequest(new ProblemDetails
      {
        Status = 400,
        Title = "Validation error",
        Detail = "auditId is required."
      });
    }

    var record = await _auditRepository.GetAsync(auditId.Trim(), ct);
    if (record is null)
    {
      return NotFound(new ProblemDetails
      {
        Status = 404,
        Title = "Audit record not found",
        Detail = "No conversion audit record exists with the provided identifier."
      });
    }

    return Ok(ConversionResponse.FromRecord(record));
  }

  [HttpGet]
  public async Task<ActionResult<IReadOnlyList<ConversionResponse>>> List([FromQuery] int? limit, CancellationToken ct)
  {
    var effectiveLimit = limit ?? 10;
    if (effectiveLimit <= 0 || effectiveLimit > 50)
    {
      return BadRequest(new ProblemDetails
      {
        Status = 400,
        Title = "Validation error",
        Detail = "limit must be between 1 and 50."
      });
    }

    var records = await _auditRepository.ListRecentAsync(effectiveLimit, ct);
    var response = records.Select(ConversionResponse.FromRecord).ToList();
    return Ok(response);
  }
}
