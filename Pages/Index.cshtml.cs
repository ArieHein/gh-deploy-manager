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

    [TempData]
    public string? LastDeploymentProjectId { get; set; }

    [TempData]
    public string? LastDeploymentEnvironment { get; set; }

    [TempData]
    public string? LastDeploymentStatus { get; set; }

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
        var deployed = _deploymentBoardService.Deploy(projectId, environment, releaseVersion, triggeredBy, out var error);

        if (!deployed)
        {
            ErrorMessage = error;
            return RedirectToPage();
        }

        LastDeploymentProjectId = projectId;
        LastDeploymentEnvironment = environment;
        LastDeploymentStatus = "success";
        return RedirectToPage();
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
