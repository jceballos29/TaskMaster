using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskMaster.Application.System.Queries.GetSystemInfo;

namespace TaskMaster.Api.Controllers;

public class SystemController(ISender mediator) : BaseApiController
{
    private readonly ISender _mediator = mediator;

    [HttpGet("info")]
    public async Task<IActionResult> GetInfo()
    {
        var result = await _mediator.Send(new GetSystemInfoQuery());

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorCode, message = result.ErrorMessage });
        }

        var systemInfo = result.Value!;
        return Ok(new { version = systemInfo.Version });
    }
}
