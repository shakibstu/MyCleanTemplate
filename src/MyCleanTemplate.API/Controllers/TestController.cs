using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyCleanTemplate.API.Models;
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
    [HttpPost]
    public async Task<IActionResult> GetCurrencies([FromBody] CreateTestRequest request)
    {
        await _mediator.Send(new CreateTestCommand());
        return Ok();
    }
}