namespace TaskMaster.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }

    protected BaseEntity() { }

    protected void UpdateTimestamp()
    {
        UpdatedAt = DateTime.Now;
    }
}
