using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScrappingManagement.Web.Dto;
using ScrappingManagement.Web.Models;

namespace ScrappingManagement.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var vModel = _roleManager.Roles
                    .Select(role => new RoleListDto
                    {
                        Id = role.Id,
                        Name = role.Name
                    }).ToList();

            return View(vModel);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Role role)
        {
            if (await _roleManager.RoleExistsAsync(role.RoleName))
            {
                TempData["Message"] = "Role already exists.";
                TempData["MessageState"] = "alert-danger";
                return RedirectToAction("Index");
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(role.RoleName));

            if (result.Succeeded)
            {
                TempData["Message"] = "Role created successfully.";
                TempData["MessageState"] = "alert-success";

                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Edit(string id)
        {
            var role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Id == id.ToString());

            if (role == null) return NotFound();

            Role vModel = new Role
            {
                Id = role.Id,
                RoleName = role.Name
            };

            return View("Edit", vModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Role role)
        {
            var existingRole = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Id == role.Id);
            if (existingRole == null) return NotFound();

            existingRole.Name = role.RoleName;

            var result = await _roleManager.UpdateAsync(existingRole);

            if (result.Succeeded)
            {
                TempData["Message"] = "Role updated successfully.";
                TempData["MessageState"] = "alert-success";

                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
    }
}
