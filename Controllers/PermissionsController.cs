using FidsSystem.Helpers;
using FidsSystem.Models;
using FidsSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace FidsSystem.Controllers
{
    public class PermissionsController : Controller
    {
        private readonly IPermissionService _permSvc;
        private readonly IAdminUserService  _userSvc;

        public PermissionsController(IPermissionService permSvc, IAdminUserService userSvc)
        {
            _permSvc = permSvc;
            _userSvc = userSvc;
        }

        private IActionResult? RequireLogin() => RbacHelper.RequireAdmin(HttpContext, this);

        public async Task<IActionResult> Index()
        {
            var r = RequireLogin(); if (r != null) return r;

            var users     = await _userSvc.GetAllAsync();
            var overrides = await _permSvc.GetAllOverridesAsync();

            ViewBag.Users     = users;
            ViewBag.Overrides = overrides;
            ViewBag.Keys      = PermKeys.All;
            ViewBag.Defs      = PermKeys.Definitions;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int userId, string key, bool enabled)
        {
            var r = RequireLogin(); if (r != null) return r;

            await _permSvc.SetPermissionAsync(userId, key, enabled);
            return Json(new { ok = true });
        }

        [HttpPost]
        public async Task<IActionResult> Reset(int userId)
        {
            var r = RequireLogin(); if (r != null) return r;

            await _permSvc.ResetToDefaultAsync(userId);
            TempData["Success"] = "User permissions reset to role defaults.";
            return RedirectToAction(nameof(Index));
        }
    }
}
