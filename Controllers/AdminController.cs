using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using talentacquisition_jobplacement_mvc.Data;
using talentacquisition_jobplacement_mvc.Models;

namespace talentacquisition_jobplacement_mvc.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: DeleteUserConfirmed - redirect to Users to avoid 404 when accessed directly
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IActionResult DeleteUserConfirmed()
        {
            // The POST action performs the deletion. Navigating to this URL by GET should not attempt deletion.
            // Redirect back to the users list to avoid a 404 and to provide a safer UX.
            return RedirectToAction(nameof(Users));
        }

        // GET: All Users
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add(new UserRoleViewModel
                {
                    User = user,
                    Roles = roles.ToList()
                });
            }

            return View(userRoles);
        }

        // GET: Edit User Roles
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> EditRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            var model = new UserRoleEditViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                CurrentRoles = userRoles.ToList(),
                AllRoles = allRoles
            };

            return View(model);
        }

        // POST: Edit User Roles
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> EditRoles(UserRoleEditViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);

            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (model.SelectedRoles != null && model.SelectedRoles.Any())
            {
                await _userManager.AddToRolesAsync(user, model.SelectedRoles);
            }

            return RedirectToAction(nameof(Users));
        }

        // GET: Delete Confirmation
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var model = new UserDeleteViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName
            };

            return View(model);
        }

        // POST: Delete User
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteUserConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (user.Id == User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Users));
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                TempData["Success"] = $"User '{user.Email}' has been deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to delete user.";
            }

            return RedirectToAction(nameof(Users));
        }

        // POST: Block User
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> BlockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (user.Id == User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                TempData["Error"] = "You cannot block your own account.";
                return RedirectToAction(nameof(Users));
            }

            if (user.IsBlocked)
            {
                TempData["Warning"] = $"User '{user.Email}' is already blocked.";
                return RedirectToAction(nameof(Users));
            }

            user.IsBlocked = true;
            user.BlockedAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["Success"] = $"User '{user.Email}' has been blocked.";
            }
            else
            {
                TempData["Error"] = "Failed to block user.";
            }

            return RedirectToAction(nameof(Users));
        }

        // POST: Unblock User
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UnblockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (!user.IsBlocked)
            {
                TempData["Warning"] = $"User '{user.Email}' is not blocked.";
                return RedirectToAction(nameof(Users));
            }

            user.IsBlocked = false;
            user.UnblockedAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["Success"] = $"User '{user.Email}' has been unblocked.";
            }
            else
            {
                TempData["Error"] = "Failed to unblock user.";
            }

            return RedirectToAction(nameof(Users));
        }

        // GET: View User Profile
        [Authorize(Roles = "Recruiter,Administrator")]
        public async Task<IActionResult> ViewProfile(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }
    }

    public class UserRoleViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public List<string> Roles { get; set; } = new();
    }

    public class UserRoleEditViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public List<string> CurrentRoles { get; set; } = new();
        public List<string> AllRoles { get; set; } = new();
        public List<string> SelectedRoles { get; set; } = new();
    }

    public class UserDeleteViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? FullName { get; set; }
    }
}


