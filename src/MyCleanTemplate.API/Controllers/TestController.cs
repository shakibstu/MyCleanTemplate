using MediatRR;
using Microsoft.AspNetCore.Mvc;
using MyCleanTemplate.API.Models;
using MyCleanTemplate.Application.Contract.Test.Commands;
using MyCleanTemplate.Application.Contract.Test.Query;
using MyCleanTemplate.Domain.Abstractions.Messaging;

namespace MyCleanTemplate.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class TestController(IMediator mediator, ILogger<TestController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateTest([FromBody] CreateTestRequest request)
    {
        using (logger.BeginScope("Processing this {name}", request.Name))
        {
            logger.LogError("LOG TEST!!");
            await mediator.Send(new CreateTestCommand{ Name = request.Name});
            return Ok();
        }
    }

    [HttpPost]
    public async Task<IActionResult> GetTest()
    {
        return Ok(await mediator.Send(new TestQuery()));
    }
}