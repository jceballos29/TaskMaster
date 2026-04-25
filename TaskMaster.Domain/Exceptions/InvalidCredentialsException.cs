namespace TaskMaster.Domain.Exceptions;

public class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException()
        : base("Invalid credentials", "INVALID_CREDENTIALS", 401) { }

    public InvalidCredentialsException(string message)
        : base(message, "INVALID_CREDENTIALS", 401) { }
}
