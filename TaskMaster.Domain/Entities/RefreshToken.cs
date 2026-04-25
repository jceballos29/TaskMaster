namespace TaskMaster.Domain.Entities;

public class RefreshToken : BaseEntity
{
    // Propiedades con setters privados para proteger el estado
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public bool IsUsed { get; private set; }
    public Guid UserId { get; private set; }

    // Propiedad de navegación
    public virtual ApplicationUser User { get; private set; } = null!;

    // Reglas de negocio encapsuladas (Calculadas)
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsUsed && !IsExpired;

    // 1. Constructor privado para obligar el uso de 'Create'
    private RefreshToken() { }

    // 2. Método Factory para consistencia con TaskItem
    public static RefreshToken Create(string token, DateTime expiresAt, Guid userId)
    {
        // Validaciones de integridad
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("El token es requerido", nameof(token));

        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException(
                "La fecha de expiración debe ser en el futuro",
                nameof(expiresAt)
            );

        if (userId == Guid.Empty)
            throw new ArgumentException("El UserId es requerido", nameof(userId));

        return new RefreshToken
        {
            Id = Guid.NewGuid(), // Generamos el ID aquí para esta entidad
            Token = token,
            ExpiresAt = expiresAt,
            UserId = userId,
            IsRevoked = false,
            IsUsed = false,
            CreatedAt = DateTime.UtcNow, // Inicializamos el campo de BaseEntity
        };
    }

    // 3. Comportamientos de dominio (DDD)
    public void Revoke()
    {
        if (!IsRevoked)
        {
            IsRevoked = true;
            UpdateTimestamp(); // Método de tu BaseEntity
        }
    }

    public void Use()
    {
        if (!IsUsed)
        {
            IsUsed = true;
            UpdateTimestamp();
        }
    }
}
