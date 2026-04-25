namespace TaskMaster.Domain.Exceptions;

public class UnauthorizedOperationException(string message)
    : DomainException(message, "FORBIDDEN", 403)
{
    public static UnauthorizedOperationException RequiresRole(string role) =>
        new($"Operation requires role '{role}'.");

    public static UnauthorizedOperationException RequiresScope(string scope) =>
        new($"Operation requires scope '{scope}'.");
}
