using FidsSystem.Helpers;
using Microsoft.AspNetCore.Mvc;
using FidsSystem.Models;
using FidsSystem.Services;

namespace FidsSystem.Controllers
{
    public class ZoneController : Controller
    {
        private readonly IZoneService _zoneService;

        public ZoneController(IZoneService zoneService)
        {
            _zoneService = zoneService;
        }

        private IActionResult? RequireLogin() => RbacHelper.RequireAdmin(HttpContext, this);

        public async Task<IActionResult> Index(string? search)
        {
            var redirect = RequireLogin(); if (redirect != null) return redirect;
            ViewBag.Search = search;
            var zones = await _zoneService.GetAllAsync(search);
            return View(zones);
        }

        public IActionResult Create()
        {
            var redirect = RequireLogin(); if (redirect != null) return redirect;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Zone zone)
        {
            var redirect = RequireLogin(); if (redirect != null) return redirect;
            if (!ModelState.IsValid) return View(zone);
            await _zoneService.CreateAsync(zone);
            TempData["Success"] = "Zone created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var redirect = RequireLogin(); if (redirect != null) return redirect;
            var zone = await _zoneService.GetByIdAsync(id);
            if (zone == null) return NotFound();
            return View(zone);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Zone zone)
        {
            var redirect = RequireLogin(); if (redirect != null) return redirect;
            if (id != zone.Id) return NotFound();
            if (!ModelState.IsValid) return View(zone);
            await _zoneService.UpdateAsync(zone);
            TempData["Success"] = "Zone updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var redirect = RequireLogin(); if (redirect != null) return redirect;
            await _zoneService.DeleteAsync(id);
            TempData["Success"] = "Zone deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
