using Microsoft.AspNetCore.Identity;

namespace TaskMaster.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    public virtual ICollection<RefreshToken> RefreshTokens { get; private set; } = [];

    protected ApplicationUser() { }

    public static ApplicationUser Create(string email, string displayName)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email required");

        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            DisplayName = displayName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
        };
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateTimestamp();
    }

    private void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
