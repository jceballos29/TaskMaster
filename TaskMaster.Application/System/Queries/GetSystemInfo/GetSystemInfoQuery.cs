using MediatR;
using TaskMaster.Application.Common.Models;

namespace TaskMaster.Application.System.Queries.GetSystemInfo;

public record GetSystemInfoQuery : IRequest<Result<SystemInfoResult>>;
