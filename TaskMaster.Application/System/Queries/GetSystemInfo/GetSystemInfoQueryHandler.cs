using MediatR;
using TaskMaster.Application.Common.Models;
using TaskMaster.Domain.Interfaces;

namespace TaskMaster.Application.System.Queries.GetSystemInfo;

public class GetSystemInfoQueryHandler(ISystemInfoService systemInfo)
    : IRequestHandler<GetSystemInfoQuery, Result<SystemInfoResult>>
{
    private readonly ISystemInfoService _systemInfo = systemInfo;

    public async Task<Result<SystemInfoResult>> Handle(
        GetSystemInfoQuery request,
        CancellationToken cancellationToken
    )
    {
        var version = _systemInfo.GetSystemVersion();
        return Result<SystemInfoResult>.Success(new SystemInfoResult { Version = version });
    }
}
