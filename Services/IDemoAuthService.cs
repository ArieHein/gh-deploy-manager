namespace gh_deploy_manager.Services;

public interface IDemoAuthService
{
    bool ValidateCredentials(string username, string password);
}
