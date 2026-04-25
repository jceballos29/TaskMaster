namespace TaskMaster.Domain.Exceptions;

public class TokenException : DomainException
{
    protected TokenException(string message, string errorCode, int httpStatusCode = 401)
        : base(message, errorCode, httpStatusCode) { }

    public static TokenException Expired() => new("Token has expired", "TOKEN_EXPIRED", 401);

    public static TokenException Revoked() => new("Token has been revoked", "TOKEN_REVOKED", 401);

    public static TokenException InvalidSignature() =>
        new("Invalid token signature", "TOKEN_INVALID_SIGNATURE");

    public static TokenException MalFormed() => new("Token is malformed", "TOKEN_MALFORMED");
}
