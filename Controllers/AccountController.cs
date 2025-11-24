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

                    // Redirect new customers to the login page
                    return RedirectToAction("Login", "Account");
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
                    return RedirectToAction("Dashboard", "Admin");
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

        // Edit Account GET
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new EditAccountViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        // Edit Account POST
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAccount(EditAccountViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            bool isUpdated = false;

            // Update profile fields
            if (user.UserName != model.UserName)
            {
                var userNameResult = await _userManager.SetUserNameAsync(user, model.UserName);
                if (userNameResult.Succeeded) isUpdated = true;
                else
                {
                    foreach (var error in userNameResult.Errors)
                        ModelState.AddModelError("", error.Description);
                    TempData["ErrorMessage"] = "Failed to update username.";
                    return View(model);
                }
            }

            if (user.Email != model.Email)
            {
                var emailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (emailResult.Succeeded) isUpdated = true;
                else
                {
                    foreach (var error in emailResult.Errors)
                        ModelState.AddModelError("", error.Description);
                    TempData["ErrorMessage"] = "Failed to update email.";
                    return View(model);
                }
            }

            if (user.PhoneNumber != model.PhoneNumber)
            {
                var phoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (phoneResult.Succeeded) isUpdated = true;
                else
                {
                    foreach (var error in phoneResult.Errors)
                        ModelState.AddModelError("", error.Description);
                    TempData["ErrorMessage"] = "Failed to update phone number.";
                    return View(model);
                }
            }

            // Update password only if provided
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(model.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is required to change password.");
                    TempData["ErrorMessage"] = "Current password is required.";
                    return View(model);
                }

                var passwordCheck = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
                if (!passwordCheck)
                {
                    ModelState.AddModelError("CurrentPassword", "Incorrect current password.");
                    TempData["ErrorMessage"] = "Incorrect current password.";
                    return View(model);
                }

                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                        ModelState.AddModelError("", error.Description);
                    TempData["ErrorMessage"] = "Failed to change password.";
                    return View(model);
                }

                isUpdated = true;
            }

            if (isUpdated)
                TempData["SuccessMessage"] = "Account details updated successfully.";
            else
                TempData["ErrorMessage"] = "No changes were made.";

            return RedirectToAction("EditAccount");
        }


        //Redirection for cancel button
        [Authorize]
        public async Task<IActionResult> CancelEditAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Admin"))
                return RedirectToAction("Dashboard", "Admin"); 
            if (roles.Contains("Organizer"))
                return RedirectToAction("Dashboard", "Organizer");
            if (roles.Contains("Customer"))
                return RedirectToAction("Dashboard", "Customer");

            // Fallback
            return RedirectToAction("Index", "Home");
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