namespace TRENZ.Docs.API.Models.Auth;

public record AuthenticateRequest(string ReturnUrl, string CallbackUrl, string BrandingName, string BrandingColor, string BrandingImageUrl = "");