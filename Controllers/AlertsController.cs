using FidsSystem.Helpers;
using Microsoft.AspNetCore.Mvc;
using FidsSystem.Services;

namespace FidsSystem.Controllers
{
    public class AlertsController : Controller
    {
        private readonly IAlertService _alertService;

        public AlertsController(IAlertService alertService) =>
            _alertService = alertService;

        private IActionResult? RequireLogin() => RbacHelper.RequireAdminOrStaff(HttpContext, this);

        public async Task<IActionResult> Index()
        {
            var r = RequireLogin(); if (r != null) return r;
            var alerts = await _alertService.GetAllAlertsAsync(100);
            ViewBag.UnreadCount = await _alertService.GetUnreadCountAsync();
            return View(alerts);
        }

        [HttpPost]
        public async Task<IActionResult> Acknowledge(int id)
        {
            var r = RequireLogin(); if (r != null) return r;
            await _alertService.AcknowledgeAsync(id);
            TempData["Success"] = "Alert acknowledged.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AcknowledgeAll()
        {
            var r = RequireLogin(); if (r != null) return r;
            await _alertService.AcknowledgeAllAsync();
            TempData["Success"] = "All alerts acknowledged.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Count()
        {
            var count = await _alertService.GetUnreadCountAsync();
            return Json(new { count });
        }
    }
}
