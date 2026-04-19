using FidsSystem.Helpers;
using Microsoft.AspNetCore.Mvc;
using FidsSystem.Models;
using FidsSystem.Services;

namespace FidsSystem.Controllers
{
    public class DeviceController : Controller
    {
        private readonly IDeviceService _deviceService;
        private readonly IZoneService _zoneService;

        public DeviceController(IDeviceService deviceService, IZoneService zoneService)
        {
            _deviceService = deviceService;
            _zoneService = zoneService;
        }

        private IActionResult? RequireLogin() => RbacHelper.RequireAdmin(HttpContext, this);

        public async Task<IActionResult> Index(string? search)
        {
            var r = RequireLogin(); if (r != null) return r;
            var devices = await _deviceService.GetAllAsync(search);
            ViewBag.Search = search;
            ViewBag.Zones = await _zoneService.GetAllAsync();
            ViewBag.DeviceTypes = DeviceTypes.All;
            return View(devices);
        }

        [HttpPost]
        public async Task<IActionResult> Create(DeviceItem model)
        {
            var r = RequireLogin(); if (r != null) return r;
            if (string.IsNullOrWhiteSpace(model.DeviceName))
            {
                TempData["Error"] = "Device Name is required.";
                return RedirectToAction(nameof(Index));
            }
            await _deviceService.CreateAsync(model);
            TempData["Success"] = "Device added.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var r = RequireLogin(); if (r != null) return r;
            await _deviceService.DeleteAsync(id);
            TempData["Success"] = "Device deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
