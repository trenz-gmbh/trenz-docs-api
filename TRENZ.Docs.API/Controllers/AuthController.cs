using Microsoft.AspNetCore.Mvc;
using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models.Auth;

namespace TRENZ.Docs.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class AuthController : ControllerBase
{
    private readonly IAuthProvider _authProvider;

    public AuthController(IAuthProvider authProvider)
    {
        _authProvider = authProvider;
    }

    [HttpGet]
    public async Task<IActionResult> Redirect()
    {
        var returnUrl = $"{Request.Scheme}://{Request.Host}{Url.Action("Callback", "Auth")!}";

        // TODO: get branding information from configuration
        var request = new AuthenticateRequest(returnUrl, "#3a6");

        return await _authProvider.RedirectToLoginPage(request);
    }

    [HttpGet]
    public async Task<IActionResult> Callback()
    {
        var result = await _authProvider.Process(HttpContext.Request);
        if (result?.Success ?? false)
        {
            // TODO: set cookie
            // MAYBE: set success message?
        }

        // TODO: redirect to front end

        return Ok();
    }
}