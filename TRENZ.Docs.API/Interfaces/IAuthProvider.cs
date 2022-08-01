using Microsoft.AspNetCore.Mvc;
using TRENZ.Docs.API.Models.Auth;

namespace TRENZ.Docs.API.Interfaces;

public interface IAuthProvider
{
    Task<IActionResult> RedirectToLoginPage(AuthenticateRequest request);
    Task<AuthenticateResult?> Process(HttpRequest request);
}