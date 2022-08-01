using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models.Auth;

namespace TRENZ.Docs.API.Services.Auth;

public class SimpleJwtAuthProvider : IAuthProvider
{
    private readonly IConfiguration _configuration;

    private string Endpoint => _configuration["AuthProvider:Endpoint"];
    private string Secret => _configuration["AuthProvider:Secret"];

    public SimpleJwtAuthProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <inheritdoc />
    public Task<IActionResult> RedirectToLoginPage(AuthenticateRequest request)
    {
        var b = new UriBuilder(Endpoint);
        var query = HttpUtility.ParseQueryString(b.Query);
        query["returnUrl"] = request.ReturnUrl;
        query["brandingColor"] = request.BrandingColor;
        query["brandingImageUrl"] = request.BrandingImageUrl;

        var payload = query.ToString() ?? "";
        var signature = GenerateSignature(payload);
        query["signature"] = signature;

        b.Query = query.ToString();

        return Task.FromResult<IActionResult>(new RedirectResult(b.ToString()));
    }

    /// <inheritdoc />
    public async Task<AuthenticateResult?> Process(HttpRequest request)
    {
        return await request.ReadFromJsonAsync<AuthenticateResult>();
    }

    private string GenerateSignature(string payload)
    {
        var key = Encoding.UTF8.GetBytes(Secret);
        using var hash = new HMACSHA256(key);
        var signature = hash.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(signature);
    }
}
