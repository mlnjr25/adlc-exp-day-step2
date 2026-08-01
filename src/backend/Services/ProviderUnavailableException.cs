namespace OuterloopLabApi.Services;

public sealed class ProviderUnavailableException : Exception
{
  public ProviderUnavailableException(string message) : base(message) { }
}
