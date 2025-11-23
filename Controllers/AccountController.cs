using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OnlineTicket.Models; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineTicket.Controllers
{
    public class AccountController : Controller
    {
        // Use the default IdentityUser class
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager,
                                 RoleManager<IdentityRole> roleManager,
                                 SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        // ✅ List all users with their roles
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var model = new List<UserViewModel>(); // Using your UserViewModel

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    IsAdmin = roles.Contains("Admin"),
                    IsOrganizer = roles.Contains("Organizer"),
                    IsCustomer = roles.Contains("Customer")
                });
            }
            // This view should be in Views/Account/Index.cshtml
            return View(model);
        }


        // ✅ REGISTER function
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Create a default IdentityUser
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // === Ensure all 3 roles exist ===
                    // (This check can be moved to your Program.cs or Startup.cs for efficiency)
                    if (!await _roleManager.RoleExistsAsync("Customer"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Customer"));
                    }
                    if (!await _roleManager.RoleExistsAsync("Organizer"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Organizer"));
                    }
                    if (!await _roleManager.RoleExistsAsync("Admin"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    }

                    // === Add new user to "Customer" role by default ===
                    await _userManager.AddToRoleAsync(user, "Customer");

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Redirect new customers to the homepage
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }


        // ✅ LOGIN function
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Use PasswordSignInAsync with email as username
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var roles = await _userManager.GetRolesAsync(user);

                // --- Role-Based Redirect ---
                if (roles.Contains("Admin"))
                {
                    return RedirectToAction("Contact", "Home");
                }
                if (roles.Contains("Organizer"))
                {
                    return RedirectToAction("Dashboard", "Organizer");
                }

                // --- Standard Customer Redirect ---
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Dashboard", "Customer");
                }
            }

            ModelState.AddModelError("", "Invalid login attempt");
            return View(model);
        }


        // ✅ Promote Customer to Organizer (Admin Only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PromoteToOrganizer(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (await _userManager.IsInRoleAsync(user, "Customer"))
            {
                // Atomically remove from Customer and add to Organizer
                await _userManager.RemoveFromRoleAsync(user, "Customer");
                await _userManager.AddToRoleAsync(user, "Organizer");

                TempData["Message"] = $"{user.Email} promoted to Organizer.";
            }
            else
            {
                TempData["Error"] = $"{user.Email} is not a Customer and cannot be promoted.";
            }

            return RedirectToAction("Index");
        }


        // ✅ Demote Organizer to Customer (Admin Only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DemoteToCustomer(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (await _userManager.IsInRoleAsync(user, "Organizer"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Organizer");
                await _userManager.AddToRoleAsync(user, "Customer");
                TempData["Message"] = $"{user.Email} demoted to Customer.";
            }
            else
            {
                TempData["Error"] = $"{user.Email} is not an Organizer.";
            }
            return RedirectToAction("Index");
        }


        // ✅ LOGOUT function
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}