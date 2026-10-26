using gh_deploy_manager.Models;
using gh_deploy_manager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gh_deploy_manager.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private const string ModernUiVariant = "modern";
    private const string KanbanUiVariant = "kanban";
    private readonly IDeploymentBoardService _deploymentBoardService;

    public IndexModel(IDeploymentBoardService deploymentBoardService)
    {
        _deploymentBoardService = deploymentBoardService;
    }

    public DeploymentDashboard Dashboard { get; private set; } = new([], [], []);

    [TempData]
    public string? ErrorMessage { get; set; }

    public string UiVariant { get; private set; } = "classic";

    public bool IsModernUi => string.Equals(UiVariant, ModernUiVariant, StringComparison.OrdinalIgnoreCase);

    public bool IsKanbanUi => string.Equals(UiVariant, KanbanUiVariant, StringComparison.OrdinalIgnoreCase);

    public void OnGet()
    {
        Dashboard = _deploymentBoardService.GetDashboard();
        UiVariant = ResolveUiVariant();
    }

    public IActionResult OnPostDeploy(string projectId, string environment, string releaseVersion)
    {
        var triggeredBy = User.Identity?.Name ?? "anonymous";
        var deploymentResult = _deploymentBoardService.Deploy(projectId, environment, releaseVersion, triggeredBy);

        if (!deploymentResult.IsValid)
        {
            ErrorMessage = deploymentResult.ErrorMessage;
            return RedirectToPage();
        }

        return RedirectToPage();
    }

    public string GetStatusChipClass(string? status)
    {
        if (string.Equals(status, DeploymentAttemptStatuses.Succeeded, StringComparison.OrdinalIgnoreCase))
            return "status-chip-success";
        if (string.Equals(status, DeploymentAttemptStatuses.Failed, StringComparison.OrdinalIgnoreCase))
            return "status-chip-failed";
        return "status-chip-hidden";
    }

    public string GetStatusChipLabel(string? status, bool compact)
    {
        if (string.Equals(status, DeploymentAttemptStatuses.Succeeded, StringComparison.OrdinalIgnoreCase))
            return compact ? "✓" : "Deployed";
        if (string.Equals(status, DeploymentAttemptStatuses.Failed, StringComparison.OrdinalIgnoreCase))
            return compact ? "✕" : "Failed";
        return string.Empty;
    }

    public string GetEventBadgeClass(string status)
    {
        if (string.Equals(status, DeploymentAttemptStatuses.Succeeded, StringComparison.OrdinalIgnoreCase))
            return "text-bg-success";
        if (string.Equals(status, DeploymentAttemptStatuses.Failed, StringComparison.OrdinalIgnoreCase))
            return "text-bg-danger";
        return "text-bg-secondary";
    }

    private string ResolveUiVariant()
    {
        var userVariant = User.FindFirst("ui_variant")?.Value;
        if (string.Equals(userVariant, ModernUiVariant, StringComparison.OrdinalIgnoreCase))
            return ModernUiVariant;
        if (string.Equals(userVariant, KanbanUiVariant, StringComparison.OrdinalIgnoreCase))
            return KanbanUiVariant;
        return "classic";
    }
}
