using CircleApp.Data.Helpers.Constants;
using CircleApp.Data.Models;
using CircleApp.ViewModels.Authentication;
using CircleApp.ViewModels.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CircleApp.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthenticationController(UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
                return View(loginVM);

            var user = await _userManager.FindByEmailAsync(loginVM.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(loginVM);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                loginVM.Password,
                false,
                false);

            if (result.Succeeded)
            {
                if (User.IsInRole(AppRoles.Admin))
                    return RedirectToAction("Index", "Admin");
                else
                    return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid login attempt");
            return View(loginVM);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
                return View(registerVM);

            var existingUser = await _userManager.FindByEmailAsync(registerVM.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email already exists");
                return View(registerVM);
            }

            var newUser = new User()
            {
                FullName = $"{registerVM.FirstName} {registerVM.LastName}",
                Email = registerVM.Email,
                UserName = registerVM.Email
            };

            var result = await _userManager.CreateAsync(newUser, registerVM.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, AppRoles.User);
                await _userManager.AddClaimAsync(newUser,
                    new Claim(CustomClaim.FullName, newUser.FullName));

                await _signInManager.SignInAsync(newUser, false);

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(registerVM);
        }

        public IActionResult ExternalLogin(string provider)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Authentication");

            var properties = _signInManager
                .ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return Challenge(properties, provider);
        }

        public async Task<IActionResult> ExternalLoginCallback()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
                return RedirectToAction("Login");

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            if (email == null)
            {
                TempData["Error"] = "Email not received from external provider.";
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new User
                {
                    Email = email,
                    UserName = email,
                    FullName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user);

                if (!result.Succeeded)
                    return RedirectToAction("Login");

                await _userManager.AddToRoleAsync(user, AppRoles.User);

                await _userManager.AddClaimAsync(user,
                    new Claim(CustomClaim.FullName, user.FullName));
            }

            await _signInManager.SignInAsync(user, false);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}