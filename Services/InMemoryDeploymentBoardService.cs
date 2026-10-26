using gh_deploy_manager.Models;

namespace gh_deploy_manager.Services;

public sealed class InMemoryDeploymentBoardService : IDeploymentBoardService
{
    private readonly object _syncRoot = new();
    private readonly List<string> _environments = ["Development", "Test", "Staging", "Production"];
    private readonly List<ProjectState> _projects;
    private readonly List<DeploymentEvent> _events;

    public InMemoryDeploymentBoardService()
    {
        _projects =
        [
            CreateProject(
                "payments-api",
                "Payments API",
                "Default Lifecycle",
                [
                    new ReleaseInfo("2.8.1", "build-6412", "stable"),
                    new ReleaseInfo("2.8.2", "build-6448", "hotfix"),
                    new ReleaseInfo("2.9.0", "build-6520", "latest")
                ]),
            CreateProject(
                "checkout-web",
                "Checkout Web",
                "Web Lifecycle",
                [
                    new ReleaseInfo("1.14.3", "build-8012", "stable"),
                    new ReleaseInfo("1.15.0", "build-8090", "latest"),
                    new ReleaseInfo("1.15.1", "build-8104", "candidate")
                ]),
            CreateProject(
                "worker-notifications",
                "Notifications Worker",
                "Worker Lifecycle",
                [
                    new ReleaseInfo("4.2.0", "build-3110", "stable"),
                    new ReleaseInfo("4.3.0", "build-3201", "latest"),
                    new ReleaseInfo("4.3.1", "build-3218", "candidate")
                ])
        ];

        _events =
        [
            new DeploymentEvent("Payments API", "Staging", "2.8.2", "build-6448", "octo-admin", "Succeeded", DateTimeOffset.UtcNow.AddMinutes(-52)),
            new DeploymentEvent("Checkout Web", "Test", "1.15.0", "build-8090", "release-bot", "Succeeded", DateTimeOffset.UtcNow.AddMinutes(-34)),
            new DeploymentEvent("Notifications Worker", "Development", "4.3.0", "build-3201", "release-bot", "Succeeded", DateTimeOffset.UtcNow.AddMinutes(-17))
        ];
    }

    public DeploymentDashboard GetDashboard()
    {
        lock (_syncRoot)
        {
            var projectSnapshots = _projects
                .Select(project => new DeploymentProject(
                    project.Id,
                    project.Name,
                    project.Lifecycle,
                    project.Releases.ToList(),
                    project.EnvironmentStates.ToDictionary(
                        state => state.Key,
                        state => state.Value,
                        StringComparer.OrdinalIgnoreCase)))
                .ToList();

            return new DeploymentDashboard(
                _environments.ToList(),
                projectSnapshots,
                _events.OrderByDescending(evt => evt.OccurredAtUtc).Take(20).ToList());
        }
    }

    public bool Deploy(string projectId, string environment, string releaseVersion, string triggeredBy, out string error)
    {
        lock (_syncRoot)
        {
            var targetProject = _projects.FirstOrDefault(project => string.Equals(project.Id, projectId, StringComparison.OrdinalIgnoreCase));
            if (targetProject is null)
            {
                error = "Project was not found.";
                return false;
            }

            if (!_environments.Contains(environment, StringComparer.OrdinalIgnoreCase))
            {
                error = "Environment is invalid.";
                return false;
            }

            var selectedRelease = targetProject.Releases.FirstOrDefault(release => string.Equals(release.Version, releaseVersion, StringComparison.OrdinalIgnoreCase));
            if (selectedRelease is null)
            {
                error = "Release version is invalid.";
                return false;
            }

            var deploymentTime = DateTimeOffset.UtcNow;
            targetProject.EnvironmentStates[environment] = new EnvironmentDeployment(
                environment,
                selectedRelease.Version,
                selectedRelease.BuildId,
                deploymentTime,
                "Healthy",
                NextDurationSeconds());

            _events.Insert(
                0,
                new DeploymentEvent(
                    targetProject.Name,
                    environment,
                    selectedRelease.Version,
                    selectedRelease.BuildId,
                    string.IsNullOrWhiteSpace(triggeredBy) ? "unknown-user" : triggeredBy,
                    "Succeeded",
                    deploymentTime));

            error = string.Empty;
            return true;
        }
    }

    private static ProjectState CreateProject(
        string id,
        string name,
        string lifecycle,
        IReadOnlyList<ReleaseInfo> releases)
    {
        var states = new Dictionary<string, EnvironmentDeployment>(StringComparer.OrdinalIgnoreCase)
        {
            ["Development"] = new EnvironmentDeployment("Development", releases[2].Version, releases[2].BuildId, DateTimeOffset.UtcNow.AddHours(-2), "Healthy", NextDurationSeconds()),
            ["Test"] = new EnvironmentDeployment("Test", releases[1].Version, releases[1].BuildId, DateTimeOffset.UtcNow.AddHours(-6), "Healthy", NextDurationSeconds()),
            ["Staging"] = new EnvironmentDeployment("Staging", releases[0].Version, releases[0].BuildId, DateTimeOffset.UtcNow.AddDays(-1), "Healthy", NextDurationSeconds()),
            ["Production"] = new EnvironmentDeployment("Production", releases[0].Version, releases[0].BuildId, DateTimeOffset.UtcNow.AddDays(-4), "Healthy", NextDurationSeconds())
        };

        return new ProjectState(id, name, lifecycle, releases.ToList(), states);
    }

    private sealed class ProjectState(
        string id,
        string name,
        string lifecycle,
        List<ReleaseInfo> releases,
        Dictionary<string, EnvironmentDeployment> environmentStates)
    {
        public string Id { get; } = id;

        public string Name { get; } = name;

        public string Lifecycle { get; } = lifecycle;

        public List<ReleaseInfo> Releases { get; } = releases;

        public Dictionary<string, EnvironmentDeployment> EnvironmentStates { get; } = environmentStates;
    }

    private static int NextDurationSeconds() => Random.Shared.Next(12, 96);
}
