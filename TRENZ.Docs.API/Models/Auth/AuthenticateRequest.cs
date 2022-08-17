namespace TRENZ.Docs.API.Models.Auth;

public record AuthenticateRequest(string ReturnUrl, string CallbackUrl, BrandingInformation? Branding = null);

public class BrandingInformation
{
    public string? Color { get; set; }
    public string? Image { get; set; }
}
