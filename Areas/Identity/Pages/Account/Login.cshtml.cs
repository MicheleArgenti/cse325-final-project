using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RecipeManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace RecipeManagement.Areas.Identity.Pages.Account
{
  public class LoginModel : PageModel
  {
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
      _signInManager = signInManager;
      _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
      [Required]
      [EmailAddress]
      public string Email { get; set; } = string.Empty;

      [Required]
      [DataType(DataType.Password)]
      public string Password { get; set; } = string.Empty;

      [Display(Name = "Remember me?")]
      public bool RememberMe { get; set; }
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
      returnUrl ??= Url.Content("~/");

      if (ModelState.IsValid)
      {
        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user != null)
        {
          var result = await _signInManager.PasswordSignInAsync(user.UserName ?? Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
          if (result.Succeeded)
          {
            return LocalRedirect(returnUrl);
          }
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
      }

      return Page();
    }
  }
}