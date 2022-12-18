using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyCleanTemplate.Application.Contract.Test.Commands;

namespace MyCleanTemplate.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly IMediator _mediator;
    public TestController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [HttpGet]
    public async Task<IActionResult> GetCurrencies()
    {
        await _mediator.Send(new CreateTestCommand());
        return Ok();
    }
}