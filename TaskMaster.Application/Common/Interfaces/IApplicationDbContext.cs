using Microsoft.EntityFrameworkCore;
using TaskMaster.Domain.Entities;

namespace TaskMaster.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TaskItem> Tasks { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
}
