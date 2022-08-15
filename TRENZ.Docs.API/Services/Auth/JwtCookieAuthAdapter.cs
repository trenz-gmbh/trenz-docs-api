using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using TRENZ.Docs.API.AuthLib;
using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models.Auth;

namespace TRENZ.Docs.API.Services.Auth;

// ReSharper disable once UnusedType.Global
public class JwtCookieAuthAdapter : IAuthAdapter
{
    private const string CookieName = nameof(JwtCookieAuthAdapter) + "Token";
    private const string ClaimsType = "groups";

    private readonly IConfiguration _configuration;

    private string Endpoint => _configuration["Auth:Endpoint"];
    private string ClientId => _configuration["Auth:ClientId"];
    private string ClientSecret => _configuration["Auth:ClientSecret"];

    public JwtCookieAuthAdapter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <inheritdoc />
    public Task<IActionResult> RedirectToSignInPageAsync(AuthenticateRequest request, CancellationToken cancellationToken = default)
    {
        var endpoint = Signature.SignEndpoint(
            Endpoint,
            new Dictionary<string, string?>
            {
                { "returnUrl", request.ReturnUrl },
                { "callbackUrl", request.CallbackUrl },
                { "brandingColor", request.BrandingColor },
                { "brandingImageUrl", request.BrandingImageUrl },
                { "clientId", ClientId },
            },
            ClientSecret
        );

        return Task.FromResult<IActionResult>(new RedirectResult(endpoint, false, false));
    }

    /// <inheritdoc />
    public Task<bool> HandleCallbackAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        var query = context.Request.QueryString.Value;
        if (query == null)
            return Task.FromResult(false);

        if (!Signature.ValidateSignature(query, ClientSecret))
            return Task.FromResult(false);

        var success = bool.Parse(context.Request.Query["success"].ToString());
        if (!success)
            return Task.FromResult(false);

        var token = context.Request.Query["token"].ToString();

        var options = ConfigureCookie().Build(context);
        context.Response.Cookies.Append(CookieName, token, options);

        return Task.FromResult(true);
    }

    private CookieBuilder ConfigureCookie()
    {
        // MAYBE: get some cookie options from configuration
        return new()
        {
            HttpOnly = true,
            SecurePolicy = CookieSecurePolicy.Always,
            SameSite = SameSiteMode.None,
            Path = "/api",
            Expiration = TimeSpan.FromDays(30),
        };
    }

    /// <inheritdoc />
    public Task SignOutAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        if (context.Request.Cookies[CookieName] == null)
            return Task.CompletedTask;

        var options = ConfigureCookie().Build(context);
        options.Expires = DateTimeOffset.MinValue;

        context.Response.Cookies.Delete(CookieName, options);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IEnumerable<string>?> GetClaimsAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        if (!context.Request.Cookies.TryGetValue(CookieName, out var jwt))
            return Task.FromResult<IEnumerable<string>?>(null);

        var jwtHandler = new JwtSecurityTokenHandler();
        if (jwtHandler.ReadToken(jwt) is not JwtSecurityToken token)
            return Task.FromResult<IEnumerable<string>?>(null);

        var claims = token.Claims
            .Where(c => c.Type == ClaimsType)
            .Select(c => c.Value);

        return Task.FromResult<IEnumerable<string>?>(claims);
    }
}