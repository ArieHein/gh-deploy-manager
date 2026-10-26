namespace gh_deploy_manager.Models;

public sealed record ReleaseInfo(string Version, string BuildId, string Tag);

public sealed record EnvironmentDeployment(
    string Environment,
    string Version,
    string BuildId,
    DateTimeOffset DeployedAtUtc,
    string Health,
    int DurationSeconds,
    string LastAttemptStatus,
    DateTimeOffset LastAttemptedAtUtc);

public sealed record DeploymentProject(
    string Id,
    string Name,
    string Lifecycle,
    IReadOnlyList<ReleaseInfo> Releases,
    IReadOnlyDictionary<string, EnvironmentDeployment> EnvironmentStates);

public sealed record DeploymentEvent(
    string ProjectName,
    string Environment,
    string Version,
    string BuildId,
    string TriggeredBy,
    string Status,
    DateTimeOffset OccurredAtUtc);

public sealed record DeploymentDashboard(
    IReadOnlyList<string> Environments,
    IReadOnlyList<DeploymentProject> Projects,
    IReadOnlyList<DeploymentEvent> RecentEvents);

public sealed record DeploymentAttemptResult(
    bool IsValid,
    string ErrorMessage,
    string? AttemptStatus);

public static class DeploymentAttemptStatuses
{
    public const string Queued = "Queued";
    public const string Succeeded = "Succeeded";
    public const string Failed = "Failed";
}
