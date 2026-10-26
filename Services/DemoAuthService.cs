using gh_deploy_manager.Configuration;
using Microsoft.Extensions.Options;

namespace gh_deploy_manager.Services;

public sealed class DemoAuthService : IDemoAuthService
{
    private readonly IReadOnlyDictionary<string, string> _users;

    public DemoAuthService(IOptions<DemoAuthOptions> authOptions)
    {
        _users = authOptions
            .Value
            .Users
            .Where(user => !string.IsNullOrWhiteSpace(user.Username))
            .ToDictionary(
                user => user.Username.Trim(),
                user => user.Password ?? string.Empty,
                StringComparer.OrdinalIgnoreCase);
    }

    public bool ValidateCredentials(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return false;
        }

        return _users.TryGetValue(username.Trim(), out var storedPassword)
            && string.Equals(storedPassword, password, StringComparison.Ordinal);
    }
}
