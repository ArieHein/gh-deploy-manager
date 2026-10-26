using gh_deploy_manager.Models;

namespace gh_deploy_manager.Services;

public interface IDeploymentBoardService
{
    DeploymentDashboard GetDashboard();

    bool Deploy(string projectId, string environment, string releaseVersion, string triggeredBy, out string error);
}
