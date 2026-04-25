namespace TaskMaster.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskMaster.Domain.Entities;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("tasks");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Title).IsRequired().HasMaxLength(100);

        builder.Property(t => t.Description).HasMaxLength(500);

        builder.Property(t => t.State).IsRequired().HasConversion<string>();

        builder.Property(t => t.Priority).IsRequired().HasConversion<string>();

        builder.Property(t => t.OriginalEstimate).IsRequired();
        builder.Property(t => t.RemainingWork).IsRequired();
        builder.Property(t => t.CompletedWork).IsRequired();

        builder.Property(t => t.Activity).HasConversion<string>();

        builder.Property(t => t.CreatedAt).IsRequired().HasColumnType("timestamptz");
        builder.Property(t => t.UpdatedAt).HasColumnType("timestamptz");

        builder.HasIndex(t => t.State);
        builder.HasIndex(t => t.Priority);
    }
}
