using FidsSystem.Helpers;
using Microsoft.AspNetCore.Mvc;
using FidsSystem.Models;
using FidsSystem.Services;

namespace FidsSystem.Controllers
{
    public class SystemManagementController : Controller
    {
        private readonly ISystemService _systemService;

        public SystemManagementController(ISystemService systemService)
        {
            _systemService = systemService;
        }

        private IActionResult? RequireLogin() => RbacHelper.RequireAdmin(HttpContext, this);

        public async Task<IActionResult> Logo(string? search)
        {
            var r = RequireLogin(); if (r != null) return r;
            var logos = await _systemService.GetAllLogosAsync(search);
            ViewBag.Search = search;
            return View(logos.ToList());
        }

        [HttpPost]
        public async Task<IActionResult> CreateLogo(AirlineLogo model)
        {
            var r = RequireLogin(); if (r != null) return r;
            if (string.IsNullOrWhiteSpace(model.AirlineName) || string.IsNullOrWhiteSpace(model.IATACode))
            {
                TempData["Error"] = "Airline Name and IATA Code are required.";
                return RedirectToAction(nameof(Logo));
            }
            await _systemService.CreateLogoAsync(model);
            TempData["Success"] = "Logo added.";
            return RedirectToAction(nameof(Logo));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLogo(int id)
        {
            var r = RequireLogin(); if (r != null) return r;
            await _systemService.DeleteLogoAsync(id);
            TempData["Success"] = "Logo deleted.";
            return RedirectToAction(nameof(Logo));
        }

        public async Task<IActionResult> DisplaySettings()
        {
            var r = RequireLogin(); if (r != null) return r;
            var settings = await _systemService.GetAllSettingsAsync();
            ViewBag.Settings = settings;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DisplaySettings(IFormCollection form)
        {
            var r = RequireLogin(); if (r != null) return r;
            var keys = new[] { "BagFirstBag","BagLastBag","BagNoFirstBag","ArrivalWindow",
                               "ArrFirstBag","ArrLastBag","ArrNoFirstBag","DelayThreshold",
                               "DepWindow","DepRemove" };
            var settings = keys
                .Where(k => form.ContainsKey(k))
                .ToDictionary(k => k, k => form[k].ToString());
            await _systemService.UpsertSettingsAsync(settings);
            TempData["Success"] = "Settings saved successfully.";
            return RedirectToAction(nameof(DisplaySettings));
        }

        public async Task<IActionResult> Language()
        {
            var r = RequireLogin(); if (r != null) return r;
            ViewBag.CurrentLanguage = await _systemService.GetSettingAsync("Language") ?? "Lao";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Language(string language)
        {
            var r = RequireLogin(); if (r != null) return r;
            await _systemService.UpsertSettingAsync("Language", language);
            TempData["Success"] = "Language updated to " + language + ".";
            return RedirectToAction(nameof(Language));
        }
    }
}
