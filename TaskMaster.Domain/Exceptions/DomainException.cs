namespace TaskMaster.Domain.Exceptions;

public abstract class DomainException(string message, string errorCode, int httpStatuCode = 500)
    : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
    public int HttpStatusCode { get; } = httpStatuCode;
}
