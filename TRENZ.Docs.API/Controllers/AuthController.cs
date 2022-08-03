using Microsoft.AspNetCore.Mvc;
using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models.Auth;

namespace TRENZ.Docs.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class AuthController : ControllerBase
{
    private readonly IAuthAdapter authAdapter;

    public AuthController(IAuthAdapter authAdapter)
    {
        this.authAdapter = authAdapter;
    }

    [HttpGet]
    public async Task<IActionResult> Transfer([FromQuery] string? returnUrl)
    {
        returnUrl ??= Request.Headers.Referer.ToString();
        var callbackUrl = $"{Request.Scheme}://{Request.Host}{Url.Action("Callback", "Auth")!}";

        // TODO: get branding information from configuration
        var request = new AuthenticateRequest(returnUrl, callbackUrl, "TRENZ Docs", "#3a6");

        return await authAdapter.RedirectToLoginPageAsync(request);
    }

    [HttpGet]
    public async Task<IActionResult> Callback([FromQuery] string returnUrl)
    {
        var result = await authAdapter.HandleCallbackAsync(HttpContext);
        if (result)
        {
            // TODO: set success message
        }
        else
        {
            // TODO: set error message
        }

        // TODO: redirect to front end

        return Redirect(returnUrl);
    }
}