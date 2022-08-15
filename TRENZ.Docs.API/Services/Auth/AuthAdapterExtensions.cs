using TRENZ.Docs.API.Interfaces;

namespace TRENZ.Docs.API.Services.Auth;

public static class AuthAdapterExtensions
{
    public static IServiceCollection AddAuthAdapter(this IServiceCollection services, string section = "Auth")
    {
        return services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var authSec = config.GetSection(section);
            if (authSec == null)
            {
                throw new($"'{section}' section not found in configuration!");
            }

            var authAdapterClass = authSec["Type"];
            if (string.IsNullOrEmpty(authAdapterClass))
            {
                throw new($"'Type' is missing in '{section}' section!");
            }

            var authAdapterType = Type.GetType(authAdapterClass);
            if (authAdapterType == null)
            {
                throw new($"Adapter class '{authAdapterClass}' does not exist! It must be a fully qualified assembly name of the class implementing IAuthAdapter.");
            }

            var instance = ActivatorUtilities.CreateInstance(provider, authAdapterType);
            if (instance is not IAuthAdapter adapter)
            {
                throw new($"Adapter class '{authAdapterClass}' does not implement IAuthAdapter!");
            }

            return adapter;
        });
    }
}
