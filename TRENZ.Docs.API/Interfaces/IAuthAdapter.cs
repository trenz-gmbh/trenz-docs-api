﻿using Microsoft.AspNetCore.Mvc;
using TRENZ.Docs.API.Models.Auth;

namespace TRENZ.Docs.API.Interfaces;

public interface IAuthAdapter
{
    Task<IActionResult> RedirectToSignInPageAsync(AuthenticateRequest request, CancellationToken cancellationToken = default);
    Task<bool> HandleCallbackAsync(HttpContext context, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>?> GetClaimsAsync(HttpContext context, CancellationToken cancellationToken = default);
    Task SignOutAsync(HttpContext context, CancellationToken cancellationToken = default);
}
