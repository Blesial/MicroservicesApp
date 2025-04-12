using System.Security.Claims;
using Duende.IdentityModel;
using IdentityServer.Models;
using IdentityServer.Pages.Account.Register;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Register
{
    // was added to ensure the page is served with proper security headers.
    [SecurityHeaders]
    // was required to allow unauthenticated users to access the registration page.
    [AllowAnonymous]
    public class Index : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public Index(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public RegisterViewModel Input { get; set; }
    
        [BindProperty]
        public bool RegisterSuccess { get; set; }


        // This method is called when the page is requested.
        // It is used to initialize the page and set up any necessary data.
        public IActionResult OnGet(string returnUrl = null)
        {
            Input = new RegisterViewModel();
            Input.ReturnUrl = returnUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (Input.Button != "register")
            {
                // If the button clicked was not "register" (Cancel), redirect to the home page.
                return RedirectToPage("../Login/Index");
            }
            
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = Input.UserName,
                    Email = Input.Email,
                    EmailConfirmed = true,
                };

                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    // This claim is stored in the database and can be included in the user's JWT (JSON Web Token) or other authentication tokens when they log in.
                    await _userManager.AddClaimsAsync(user, new Claim[]
                    {
                        new Claim(JwtClaimTypes.Name, Input.FullName),
                    });

                    RegisterSuccess = true;
                }
            }

            return Page();
        }
    }
}

// Token-Based Authentication:

//If your application uses JWTs for authentication, the FullName claim can be included in the token. This allows other services or APIs to access the user's full name without querying the database.