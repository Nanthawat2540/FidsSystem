using FidsSystem.Helpers;
using Microsoft.AspNetCore.Mvc;
using FidsSystem.Models;
using FidsSystem.Services;

namespace FidsSystem.Controllers
{
    public class DisplayManagementController : Controller
    {
        private readonly IDisplayDeviceService _displayService;
        private readonly IZoneService _zoneService;
        private readonly ITemplateService _templateService;

        public DisplayManagementController(IDisplayDeviceService displayService, IZoneService zoneService, ITemplateService templateService)
        {
            _displayService = displayService;
            _zoneService = zoneService;
            _templateService = templateService;
        }

        private IActionResult? RequireLogin() => RbacHelper.RequireAdmin(HttpContext, this);

        public async Task<IActionResult> Index(string? search)
        {
            var r = RequireLogin(); if (r != null) return r;
            var devices = await _displayService.GetAllAsync(search);
            ViewBag.Search = search;
            return View(devices);
        }

        public async Task<IActionResult> Create()
        {
            var r = RequireLogin(); if (r != null) return r;
            ViewBag.Zones = await _zoneService.GetAllAsync();
            ViewBag.Templates = await _templateService.GetAllAsync();
            return View(new DisplayDevice());
        }

        [HttpPost]
        public async Task<IActionResult> Create(DisplayDevice model)
        {
            var r = RequireLogin(); if (r != null) return r;
            if (!ModelState.IsValid)
            {
                ViewBag.Zones = await _zoneService.GetAllAsync();
                ViewBag.Templates = await _templateService.GetAllAsync();
                return View(model);
            }
            await _displayService.CreateAsync(model);
            TempData["Success"] = "Display device created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var r = RequireLogin(); if (r != null) return r;
            var item = await _displayService.GetByIdAsync(id);
            if (item == null) return NotFound();
            ViewBag.Zones = await _zoneService.GetAllAsync();
            ViewBag.Templates = await _templateService.GetAllAsync();
            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, DisplayDevice model)
        {
            var r = RequireLogin(); if (r != null) return r;
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid)
            {
                ViewBag.Zones = await _zoneService.GetAllAsync();
                ViewBag.Templates = await _templateService.GetAllAsync();
                return View(model);
            }
            await _displayService.UpdateAsync(model);
            TempData["Success"] = "Display device updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleOn(int id)
        {
            var r = RequireLogin(); if (r != null) return r;
            var result = await _displayService.ToggleOnAsync(id);
            return Json(new { isOn = result });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var r = RequireLogin(); if (r != null) return r;
            await _displayService.DeleteAsync(id);
            TempData["Success"] = "Display device deleted.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Preview(int id)
        {
            var r = RequireLogin(); if (r != null) return r;
            var device = await _displayService.GetByIdAsync(id);
            if (device == null) return NotFound();
            if (device.TemplateId.HasValue)
                ViewBag.Template = await _templateService.GetByIdAsync(device.TemplateId.Value);
            return View(device);
        }
    }
}
