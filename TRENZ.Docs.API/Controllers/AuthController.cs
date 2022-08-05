using Microsoft.AspNetCore.Mvc;
using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models.Auth;

namespace TRENZ.Docs.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class AuthController : ControllerBase
{
    private readonly IAuthAdapter? _authAdapter;
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration, IAuthAdapter? authAdapter = null)
    {
        _authAdapter = authAdapter;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> Transfer([FromQuery] string? returnUrl, CancellationToken cancellationToken = default)
    {
        returnUrl ??= Request.Headers.Referer.ToString();
        if (_authAdapter == null)
            return Redirect(returnUrl); // MAYBE: append message telling user login is not available?

        var callbackUrl = $"{Request.Scheme}://{Request.Host}{Url.Action("Callback", "Auth")!}";

        var request = new AuthenticateRequest(returnUrl, callbackUrl, _configuration["Branding:Color"], _configuration["Branding:Image"]);

        return await _authAdapter.RedirectToSignInPageAsync(request, cancellationToken);
    }

    [HttpGet]
    public async Task<IActionResult> Callback([FromQuery] string returnUrl, CancellationToken cancellationToken = default)
    {
        if (_authAdapter == null)
            return Redirect(returnUrl); // MAYBE: append message telling user login is not available?

        var success = await _authAdapter.HandleCallbackAsync(HttpContext, cancellationToken);
        if (!success)
        {
            // TODO: set error message
        }

        return Redirect(returnUrl);
    }

    [HttpGet]
    public async Task<IActionResult> SignOut([FromQuery] string returnUrl, CancellationToken cancellationToken = default)
    {
        if (_authAdapter != null)
            await _authAdapter.SignOutAsync(HttpContext, cancellationToken);

        return Redirect(returnUrl);
    }

    [HttpGet]
    public async Task<bool> State(CancellationToken cancellationToken = default)
    {
        if (_authAdapter == null)
            return false;

        var claims = await _authAdapter.GetClaimsAsync(HttpContext, cancellationToken);

        return claims != null;
    }
}