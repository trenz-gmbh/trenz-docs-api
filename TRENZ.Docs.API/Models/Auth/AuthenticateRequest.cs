namespace TRENZ.Docs.API.Models.Auth;

public record AuthenticateRequest(string ReturnUrl, string CallbackUrl, string BrandingColor, string BrandingImageUrl = "");