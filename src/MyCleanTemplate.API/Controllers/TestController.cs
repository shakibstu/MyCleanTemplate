using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyCleanTemplate.API.Models;
using MyCleanTemplate.Application.Contract.Test.Commands;
using MyCleanTemplate.Application.Contract.Test.Query;

namespace MyCleanTemplate.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class TestController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> GetCurrencies([FromBody] CreateTestRequest request)
    {
        await mediator.Send(new CreateTestCommand(request.Name, DateTime.Now));
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> GetTest()
    {
        return Ok(await mediator.Send(new TestQuery()));
    }
}