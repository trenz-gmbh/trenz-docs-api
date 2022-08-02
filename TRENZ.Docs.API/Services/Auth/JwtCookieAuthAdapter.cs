using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models.Auth;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace TRENZ.Docs.API.Services.Auth;

// ReSharper disable once UnusedType.Global
public class JwtCookieAuthAdapter : IAuthAdapter
{
    private const string CookieName = nameof(JwtCookieAuthAdapter) + "Token";
    private const string ClaimsType = JwtRegisteredClaimNames.Sid;

    private readonly IConfiguration _configuration;

    private string Endpoint => _configuration["Auth:Endpoint"];
    private string Secret => _configuration["Auth:Secret"];

    public JwtCookieAuthAdapter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <inheritdoc />
    public Task<IActionResult> RedirectToLoginPageAsync(AuthenticateRequest request, CancellationToken cancellationToken = default)
    {
        var endpoint = GetSignedEndpoint(request);

        return Task.FromResult<IActionResult>(new RedirectResult(endpoint, false, false));
    }

    /// <inheritdoc />
    public Task<bool> HandleCallbackAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        var success = bool.Parse(context.Request.Query["success"].ToString());
        if (!success)
        {
            return Task.FromResult(false);
        }

        var token = context.Request.Query["token"].ToString();

        // MAYBE: get some cookie options from configuration
        var cookieOptsBuilder = new CookieBuilder
        {
            HttpOnly = true,
            SecurePolicy = CookieSecurePolicy.None,
            SameSite = SameSiteMode.Lax,
            Expiration = TimeSpan.FromDays(30),
        };
        var cookieOpts = cookieOptsBuilder.Build(context);
        context.Response.Cookies.Append(CookieName, token, cookieOpts);

        return Task.FromResult(true);
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

    private string GetSignedEndpoint(AuthenticateRequest request)
    {
        var uri = new UriBuilder(Endpoint);

        var query = HttpUtility.ParseQueryString(uri.Query);
        query["returnUrl"] = request.ReturnUrl;
        query["callbackUrl"] = request.CallbackUrl;
        query["brandingColor"] = request.BrandingColor;
        query["brandingImageUrl"] = request.BrandingImageUrl;

        var payload = query.ToString()!;
        var signature = GenerateSignature(payload);
        query["signature"] = signature;

        uri.Query = query.ToString();

        return uri.ToString();
    }

    private string GenerateSignature(string payload)
    {
        var key = Encoding.UTF8.GetBytes(Secret);
        using var hash = new HMACSHA256(key);
        var signature = hash.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(signature);
    }
}