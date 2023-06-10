using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyCleanTemplate.API.Models;
using MyCleanTemplate.Application.Contract.Test.Commands;
using MyCleanTemplate.Application.Contract.Test.Query;

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
        return Ok(await _mediator.Send(new CreateTestCommand(request.Name, DateTime.Now)));
    }

    [HttpPost]
    public async Task<IActionResult> GetTest()
    {
        return Ok(await _mediator.Send(new TestQuery()));
    }
}