using OuterloopLabApi.Models;

namespace OuterloopLabApi.Services;

public interface IAuditRepository
{
  Task CreateAsync(ConversionAuditRecord record, CancellationToken ct);
  Task<ConversionAuditRecord?> GetAsync(string auditId, CancellationToken ct);
  Task<IReadOnlyList<ConversionAuditRecord>> ListRecentAsync(int limit, CancellationToken ct);
}
