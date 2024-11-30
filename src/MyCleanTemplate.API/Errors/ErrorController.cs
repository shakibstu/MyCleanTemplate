using Microsoft.AspNetCore.Mvc;

namespace MyCleanTemplate.API.Errors;

[Route("api/error")]
[ApiController]
public class ErrorController : ControllerBase
{
    [AcceptVerbs("GET", "POST", "PUT", "PATCH", "DELETE")]
    public IActionResult Index()
    {
        return Problem();
    }

}