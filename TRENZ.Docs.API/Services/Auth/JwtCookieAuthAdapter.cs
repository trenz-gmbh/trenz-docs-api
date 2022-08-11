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
        var endpoint = SignEndpoint(
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

        if (!ValidateSignature(query, ClientSecret))
            return Task.FromResult(false);

        var success = bool.Parse(context.Request.Query["success"].ToString());
        if (!success)
            return Task.FromResult(false);

        var token = context.Request.Query["token"].ToString();

        // MAYBE: get some cookie options from configuration
        var cookieOptsBuilder = new CookieBuilder
        {
            HttpOnly = true,
            SecurePolicy = CookieSecurePolicy.Always,
            SameSite = SameSiteMode.None,
            Path = "/api",
            Expiration = TimeSpan.FromDays(30),
        };
        var cookieOpts = cookieOptsBuilder.Build(context);
        context.Response.Cookies.Append(CookieName, token, cookieOpts);

        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task SignOutAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        if (context.Request.Cookies[CookieName] != null)
            context.Response.Cookies.Delete(CookieName, new()
            {
                Expires = DateTimeOffset.MinValue,
            });

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

    private static string SignEndpoint(
        string endpoint,
        IDictionary<string, string?> queryParams,
        string secret
    )
    {
        var uri = new UriBuilder(endpoint);

        var query = HttpUtility.ParseQueryString(uri.Query);
        foreach (var (key, value) in queryParams)
        {
            query[key] = value;
        }

        var payload = query.ToString()!;
        var signature = GenerateSignature(secret, payload);
        query["signature"] = signature;

        uri.Query = query.ToString();

        return uri.ToString();
    }

    private static bool ValidateSignature(
        string query,
        string secret
    )
    {
        var collection = HttpUtility.ParseQueryString(query);
        if (collection["signature"] == null)
            return false;

        var signature = collection["signature"];
        collection.Remove("signature");

        var payload = collection.ToString()!;
        var expectedSignature = GenerateSignature(secret, payload);

        return signature == expectedSignature;
    }

    private static string GenerateSignature(string secret, string payload)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        using var hash = new HMACSHA256(key);
        var signature = hash.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(signature);
    }
}