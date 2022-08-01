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

    public async Task<IActionResult> Redirect()
    {
        var request = new AuthenticateRequest(Url.Action("Callback", "Auth")!, "#3a6"); // TODO: get branding information from configuration

        return await _authProvider.RedirectToLoginPage(request);
    }

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