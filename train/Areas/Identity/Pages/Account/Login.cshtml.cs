using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using train.Areas.Identity.Data;

namespace train.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<appusercontext> _signInManager;
        private readonly UserManager<appusercontext> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            SignInManager<appusercontext> signInManager,
            UserManager<appusercontext> userManager,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager   = userManager;
            _logger        = logger;
        }

        // ----------------- bindable input -----------------
        public class InputModel
        {
            // Not using [EmailAddress] so we can accept username OR email
            [System.ComponentModel.DataAnnotations.Required]
            [System.ComponentModel.DataAnnotations.Display(Name = "Email or username")]
            public string EmailOrUsername { get; set; }

            [System.ComponentModel.DataAnnotations.Required]
            [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
            public string Password { get; set; }

            [System.ComponentModel.DataAnnotations.Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public string ReturnUrl { get; set; }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return Page();

            // Find by email OR username
            var user = await _userManager.FindByEmailAsync(Input.EmailOrUsername)
                       ?? await _userManager.FindByNameAsync(Input.EmailOrUsername);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            // Respect RequireConfirmedAccount
            if (_userManager.Options.SignIn.RequireConfirmedAccount &&
                !await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(string.Empty, "Please confirm your email before logging in.");
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, Input.Password, Input.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");

                // 🔹 Admins go to Admin dashboard
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    return RedirectToAction("Index", "Admin");

                // 🔹 Everyone else goes to Catalog page
                return RedirectToAction("Index", "Catalog");
            }

            if (result.RequiresTwoFactor)
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

    }
}
