using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using gh_deploy_manager.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gh_deploy_manager.Pages;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly IDemoAuthService _authService;

    public LoginModel(IDemoAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    [Required]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public bool RememberMe { get; set; }

    [BindProperty]
    public string? UiVariant { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return RedirectToPage("/Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!_authService.ValidateCredentials(Username, Password))
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return Page();
        }

        var selectedUiVariant = string.Equals(UiVariant, "modern", StringComparison.OrdinalIgnoreCase)
            ? "modern"
            : string.Equals(UiVariant, "kanban", StringComparison.OrdinalIgnoreCase)
            ? "kanban"
            : "classic";

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, Username.Trim()),
            new("ui_variant", selectedUiVariant)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(RememberMe ? 24 : 8)
            });

        if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }

        return RedirectToPage("/Index");
    }
}
