namespace QuantFlow.Api.Rest.Controllers;

public class BaseController : ControllerBase
{
    private readonly ILogger<BaseController> _logger;

    public BaseController(ILogger<BaseController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}