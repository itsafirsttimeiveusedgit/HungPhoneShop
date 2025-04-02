using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HungPhoneShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: /Users
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRolesViewModel.Add(new UserRolesViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }

            return View(userRolesViewModel);
        }

        // GET: /Users/ManageRoles/5
        public async Task<IActionResult> ManageRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new ManageUserRolesViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                Roles = roles.Select(r => new RoleViewModel
                {
                    RoleName = r.Name,
                    IsSelected = userRoles.Contains(r.Name)
                }).ToList()
            };

            return View(model);
        }

        // POST: /Users/ManageRoles/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRoles(ManageUserRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var selectedRoles = model.Roles.Where(r => r.IsSelected).Select(r => r.RoleName).ToList();

            // Xóa các vai trò hiện tại của người dùng
            await _userManager.RemoveFromRolesAsync(user, userRoles);

            // Thêm các vai trò mới được chọn
            await _userManager.AddToRolesAsync(user, selectedRoles);

            return RedirectToAction(nameof(Index));
        }
    }

    public class UserRolesViewModel
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public List<string>? Roles { get; set; }
    }

    public class ManageUserRolesViewModel
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public List<RoleViewModel>? Roles { get; set; }
    }

    public class RoleViewModel
    {
        public string? RoleName { get; set; }
        public bool IsSelected { get; set; }
    }
}