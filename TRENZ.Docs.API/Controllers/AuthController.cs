using System.Web;
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

    private Task<IActionResult> AppendErrorCode(string returnUrl, string code)
    {
        var uri = new UriBuilder(returnUrl);
        var query = HttpUtility.ParseQueryString(uri.Query);
        query["error"] = code;
        uri.Query = query.ToString();
        return Task.FromResult<IActionResult>(Redirect(uri.ToString()));
    }

    [HttpGet]
    public async Task<IActionResult> Transfer([FromQuery] string? returnUrl, CancellationToken cancellationToken = default)
    {
        returnUrl ??= Request.Headers.Referer.ToString();
        if (_authAdapter == null)
            return await AppendErrorCode(returnUrl, "login_not_available");

        var callbackUrlBuilder = new UriBuilder(Request.Scheme, Request.Host.Host);
        if (Request.Host.Port.HasValue)
            callbackUrlBuilder.Port = Request.Host.Port.Value;

        callbackUrlBuilder.Path = Url.Action("Callback", "Auth");

        var branding = _configuration.GetSection("Branding")?.Get<BrandingInformation?>();
        var request = new AuthenticateRequest(
            returnUrl,
            callbackUrlBuilder.ToString(),
            branding
        );

        return await _authAdapter.RedirectToSignInPageAsync(request, cancellationToken);
    }

    [HttpGet]
    public async Task<IActionResult> Callback([FromQuery] string returnUrl, CancellationToken cancellationToken = default)
    {
        if (_authAdapter == null)
            return await AppendErrorCode(returnUrl, "login_not_available");

        var success = await _authAdapter.HandleCallbackAsync(HttpContext, cancellationToken);
        if (!success)
            return await AppendErrorCode(returnUrl, "invalid_callback");

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

#if DEBUG
    [HttpGet]
    public async Task<IEnumerable<string>?> Claims(CancellationToken cancellationToken = default)
    {
        return _authAdapter is null ? null : await _authAdapter.GetClaimsAsync(HttpContext, cancellationToken);
    }
#endif
}