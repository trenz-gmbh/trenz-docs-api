namespace TRENZ.Docs.API.Models.Auth;

public record AuthenticateRequest(string ReturnUrl, string CallbackUrl, string? BrandingColor = null, string? BrandingImageUrl = null);