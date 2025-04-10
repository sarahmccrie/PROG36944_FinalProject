using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FinalProject_ERMS.ViewModels;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // GET: Admin
    public IActionResult Index()
    {
        return View();
    }


    // GET: Admin/ListUsers
    public IActionResult ListUsers()
    {
        var users = _userManager.Users.ToList();
        var roles = _roleManager.Roles.Select(r => r.Name).ToList();

        var viewModel = users.Select(user => new UserWithRoleViewModel
        {
            UserId = user.Id,
            Email = user.Email,
            Roles = _userManager.GetRolesAsync(user).Result.ToList()
        }).ToList();

        ViewBag.AllRoles = roles;

        return View(viewModel);
    }

    // GET: Admin/EditUserRole/{userId}
    public async Task<IActionResult> EditUserRole(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var roles = _roleManager.Roles.Select(r => r.Name).ToList();
        var userRoles = await _userManager.GetRolesAsync(user);

        var viewModel = new EditUserRoleViewModel
        {
            UserId = user.Id,
            Email = user.Email,
            SelectedRole = userRoles.FirstOrDefault(),
            AvailableRoles = roles
        };

        return View(viewModel);
    }

    // POST: Admin/EditUserRole
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUserRole(EditUserRoleViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null) return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);

        // Remove old roles
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        // Add selected role
        if (!string.IsNullOrEmpty(model.SelectedRole))
        {
            await _userManager.AddToRoleAsync(user, model.SelectedRole);
        }

        return RedirectToAction(nameof(ListUsers));
    }

    // GET: Admin/CreateUser
    public IActionResult CreateUser()
    {
        ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
        return View();
    }

    // POST: Admin/CreateUser
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new IdentityUser { UserName = model.Email, Email = model.Email, EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.Role))
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                }

                return RedirectToAction(nameof(ListUsers));
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
        return View(model);
    }

    // GET: Admin/DeleteUser/{userId}
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var viewModel = new UserWithRoleViewModel
        {
            UserId = user.Id,
            Email = user.Email,
            Roles = (await _userManager.GetRolesAsync(user)).ToList()
        };

        return View(viewModel);
    }

    // POST: Admin/DeleteUserConfirmed/{userId}
    [HttpPost, ActionName("DeleteUser")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUserConfirmed(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Error deleting user.");
            return RedirectToAction(nameof(ListUsers)); 
        }

        return RedirectToAction(nameof(ListUsers));
    }

}
