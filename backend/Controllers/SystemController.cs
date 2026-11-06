using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
public class SystemController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IHostEnvironment _environment;

    public SystemController(IOrderService orderService, IHostEnvironment environment)
    {
        _orderService = orderService;
        _environment = environment;
    }

    [HttpGet("/health")]
    public IActionResult GetHealth()
    {
        return Ok("Healthy");
    }

    [HttpGet("/info")]
    public IActionResult GetInfo()
    {
        return Ok(new
        {
            app = _orderService.GetApplicationName(),
            environment = _environment.EnvironmentName,
            version = "ЛР8",
            note = _orderService.GetLabStageNote()
        });
    }
}
