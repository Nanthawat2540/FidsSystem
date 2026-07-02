using FidsSystem.Helpers;
using Microsoft.AspNetCore.Mvc;
using FidsSystem.Models;
using FidsSystem.Services;

namespace FidsSystem.Controllers
{
    public class TemplateController : Controller
    {
        private readonly ITemplateService _templateService;

        public TemplateController(ITemplateService templateService)
        {
            _templateService = templateService;
        }

        private IActionResult? RequireLogin() => RbacHelper.RequireAdmin(HttpContext, this);

        public async Task<IActionResult> Index(string? search, string? deviceType)
        {
            var r = RequireLogin(); if (r != null) return r;
            var templates = await _templateService.GetAllAsync(search, deviceType);
            ViewBag.Search = search;
            ViewBag.DeviceType = deviceType;
            ViewBag.DeviceTypes = DeviceTypes.All;
            return View(templates);
        }

        public IActionResult Create()
        {
            var r = RequireLogin(); if (r != null) return r;
            ViewBag.DeviceTypes = DeviceTypes.All;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(TemplateItem model)
        {
            var r = RequireLogin(); if (r != null) return r;
            if (!ModelState.IsValid) { ViewBag.DeviceTypes = DeviceTypes.All; return View(model); }
            await _templateService.CreateAsync(model);
            TempData["Success"] = "Template created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var r = RequireLogin(); if (r != null) return r;
            var item = await _templateService.GetByIdAsync(id);
            if (item == null) return NotFound();
            ViewBag.DeviceTypes = DeviceTypes.All;
            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, TemplateItem model)
        {
            var r = RequireLogin(); if (r != null) return r;
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) { ViewBag.DeviceTypes = DeviceTypes.All; return View(model); }
            await _templateService.UpdateAsync(model);
            TempData["Success"] = "Template updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var r = RequireLogin(); if (r != null) return r;
            await _templateService.DeleteAsync(id);
            TempData["Success"] = "Template deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
