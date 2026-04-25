namespace TaskMaster.Domain.Exceptions;

public class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException()
        : base("Invalid credentials", "INVALID_CREDENTIALS", 401) { }

    public InvalidCredentialsException(string message)
        : base(message, "INVALID_CREDENTIALS", 401) { }

    public static InvalidCredentialsException AccountLocked(TimeSpan timeRemaining)
    {
        var minutes = (int)timeRemaining.TotalMinutes;
        return new InvalidCredentialsException(
            $"Account locked due to too many failed attempts. Please try again in {minutes} minutes."
        );
    }
}
