using FidsSystem.Helpers;
using FidsSystem.Models;
using FidsSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace FidsSystem.Controllers
{
    public class AdminManagementController : Controller
    {
        private readonly IAdminUserService _adminService;
        private readonly ISystemService _systemService;

        public AdminManagementController(IAdminUserService adminService, ISystemService systemService)
        {
            _adminService = adminService;
            _systemService = systemService;
        }

        private IActionResult? RequireLogin() => RbacHelper.RequireAdmin(HttpContext, this);

        public async Task<IActionResult> Index()
        {
            var r = RequireLogin(); if (r != null) return r;
            var users = await _adminService.GetAllAsync();
            var airlines = await _systemService.GetAirlineLogosAsync();
            ViewBag.Airlines = airlines;
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AdminUser model, string password)
        {
            var r = RequireLogin(); if (r != null) return r;
            if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Username and password are required.";
                return RedirectToAction(nameof(Index));
            }
            if (model.Role == Roles.Airline && string.IsNullOrWhiteSpace(model.AirlineCode))
            {
                TempData["Error"] = "Airline code is required for AIRLINE role.";
                return RedirectToAction(nameof(Index));
            }
            var result = await _adminService.CreateAsync(model, password);
            TempData[result == -1 ? "Error" : "Success"] = result == -1
                ? $"Username '{model.Username}' already exists."
                : $"User '{model.Username}' created as {model.Role}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AdminUser model, string? newPassword)
        {
            var r = RequireLogin(); if (r != null) return r;
            if (model.Id == 1 && HttpContext.Session.GetString("UserId") != "1")
            {
                TempData["Error"] = "Cannot edit the primary admin account.";
                return RedirectToAction(nameof(Index));
            }
            await _adminService.UpdateAsync(model, newPassword);
            TempData["Success"] = $"User updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var r = RequireLogin(); if (r != null) return r;
            await _adminService.DeleteAsync(id);
            TempData["Success"] = "User deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
