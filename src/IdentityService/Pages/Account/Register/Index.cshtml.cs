using IdentityModel;
using IdentityService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace IdentityService.Pages.Register;

[SecurityHeaders]
[AllowAnonymous]
public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    [BindProperty]
    public bool RegisterSuccess { get; set; }

    public IActionResult OnGet(string returnUrl)
    {
        Input = new InputModel
        {
            ReturnUrl = returnUrl
        };

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (Input.Button is not "register")
        {
            return Redirect("~/");
        }

        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {

                Email = Input.Email,
                EmailConfirmed = true,
                UserName = Input.Username
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                await _userManager.AddClaimsAsync(user,
                [
                    new Claim(JwtClaimTypes.Name,Input.FullName),
                ]);

                RegisterSuccess = true;
            }
        }

        return Page();
    }
}
