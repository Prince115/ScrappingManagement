using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScrappingManagement.Web.Dto;

namespace ScrappingManagement.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleManagementController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public RoleManagementController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.GroupBy(x => x.Id).Select(x => x.First()).ToList();
            var model = new List<RoleManagementListDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                model.Add(new RoleManagementListDto
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Roles = string.Join(", ", roles.Distinct())
                });
            }

            return View(model);
        }


        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.Select(x => x.Name).ToList();

            var vModel = new UserRoleEditDto
            {
                UserId = user.Id,
                UserName = user.UserName,
                SelectedRoles = userRoles.ToList(),
                AllRoles = allRoles
            };

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserRoleEditDto model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var vExistingRoles = await _userManager.GetRolesAsync(user);

            var vRolesToAdd = model.SelectedRoles.Except(vExistingRoles).ToList();
            var vRolesToRemove = vExistingRoles.Except(model.SelectedRoles).ToList();

            if (vRolesToAdd.Any())
                await _userManager.AddToRolesAsync(user, vRolesToAdd);

            if (vRolesToRemove.Any())
                await _userManager.RemoveFromRolesAsync(user, vRolesToRemove);

            TempData["Message"] = "User roles updated successfully.";
            TempData["MessageState"] = "alert-success";

            return RedirectToAction(nameof(Index));
        }
    }
}
