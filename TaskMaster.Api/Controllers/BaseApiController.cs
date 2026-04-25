using Microsoft.AspNetCore.Mvc;

namespace TaskMaster.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase { }
