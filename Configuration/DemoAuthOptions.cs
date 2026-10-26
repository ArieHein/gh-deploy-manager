namespace gh_deploy_manager.Configuration;

public sealed class DemoAuthOptions
{
    public const string SectionName = "DemoAuth";

    public List<DemoUserCredential> Users { get; init; } = [];
}

public sealed class DemoUserCredential
{
    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
