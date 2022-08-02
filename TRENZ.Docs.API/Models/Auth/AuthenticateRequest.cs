namespace TRENZ.Docs.API.Models.Auth;

public record AuthenticateRequest(string ReturnUrl, string BrandingColor, string BrandingImageUrl = "");