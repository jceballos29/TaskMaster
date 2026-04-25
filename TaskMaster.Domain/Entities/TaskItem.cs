using TaskMaster.Domain.Enum;

namespace TaskMaster.Domain.Entities;

public class TaskItem : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TaskState State { get; private set; } = TaskState.ToDo;
    public Priority Priority { get; private set; } = Priority.Medium;
    public int OriginalEstimate { get; private set; }
    public int RemainingWork { get; private set; }
    public int CompletedWork { get; private set; }
    public Activity? Activity { get; private set; }

    private TaskItem() { }

    public static TaskItem Create(Guid id, string title, string? description, int originalEstimante)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id is required", nameof(id));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));

        if (originalEstimante < 0)
            throw new ArgumentException(
                "Original estimante must be greater than 0",
                nameof(originalEstimante)
            );

        return new TaskItem
        {
            Id = id,
            Title = title,
            Description = description ?? string.Empty,
            State = TaskState.ToDo,
            Priority = Priority.Medium,
            OriginalEstimate = originalEstimante,
            RemainingWork = originalEstimante,
            CompletedWork = 0,
        };
    }
}
