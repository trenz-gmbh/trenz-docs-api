using Microsoft.AspNetCore.Mvc;
using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models.Auth;

namespace TRENZ.Docs.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class AuthController : ControllerBase
{
    private readonly IAuthAdapter authAdapter;
    private readonly IConfiguration configuration;

    public AuthController(IAuthAdapter authAdapter, IConfiguration configuration)
    {
        this.authAdapter = authAdapter;
        this.configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> Transfer([FromQuery] string? returnUrl)
    {
        returnUrl ??= Request.Headers.Referer.ToString();
        var callbackUrl = $"{Request.Scheme}://{Request.Host}{Url.Action("Callback", "Auth")!}";

        var request = new AuthenticateRequest(returnUrl, callbackUrl, configuration["Branding:Color"], configuration["Branding:Image"]);

        return await authAdapter.RedirectToLoginPageAsync(request);
    }

    [HttpGet]
    public async Task<IActionResult> Callback([FromQuery] string returnUrl)
    {
        var success = await authAdapter.HandleCallbackAsync(HttpContext);
        if (!success)
        {
            // TODO: set error message
        }

        return Redirect(returnUrl);
    }
}