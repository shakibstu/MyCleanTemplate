using System.Security.Claims;
using MediatRR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCleanTemplate.API.Models.Auth;
using MyCleanTemplate.Application.Contract.Auth;
using MyCleanTemplate.Application.Contract.Auth.Commands;
using MyCleanTemplate.Application.Contract.Auth.Query;
using MyCleanTemplate.Infrastructure.Authentication;
using MyCleanTemplate.Infrastructure.Authentication.Attributes;

namespace MyCleanTemplate.API.Controllers;

/// <summary>
/// Controller for authentication operations.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class AuthController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns the authenticator key for MFA enrollment. Only available while MFA is not yet enabled.
    /// </summary>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetMFAKey()
    {
        var key = await mediator.Send(new GetMfaKeyQuery(CurrentUserId));
        return Ok(new MFAKeyModel { Key = key });
    }

    /// <summary>
    /// Confirms MFA enrollment for the current user with a code from the authenticator app.
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> VerifyMFAKey([FromBody] VerifyMFATokenModel model)
    {
        await mediator.Send(new EnableMfaCommand { UserId = CurrentUserId, MfaCode = model.SecurityCode });
        return Ok();
    }

    /// <summary>
    /// Sign in. Users with MFA enabled must also send the authenticator code in <see cref="SignInModel.Token"/>.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<AuthToken>> SignIn([FromBody] SignInModel model)
    {
        return await mediator.Send(new SignInCommand
        {
            Username = model.Username,
            Password = model.Password,
            MfaCode = model.Token
        });
    }

    /// <summary>
    /// A protected endpoint that requires the Admin role.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public IActionResult GetProtectedResource()
    {
        return Ok(new { Message = "You have access to this protected resource!", User = User.Identity?.Name ?? "Unknown" });
    }

    /// <summary>
    /// A protected endpoint that requires the user to have the "Create Users" permission.
    /// </summary>
    [HttpPost]
    [RequireMfa]
    [MustHavePermission(AppService.Identity, AppFeature.Users, AppAction.Create)]
    public IActionResult CreateProtectedResource([FromBody] object data)
    {
        return Ok(new { Message = "Protected resource created successfully!", Data = data });
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
}

public class MFAKeyModel
{
    public string Key { get; set; }
}

public class VerifyMFATokenModel
{
    public string SecurityCode { get; set; }
}
