using FidsSystem.Helpers;
using Microsoft.AspNetCore.Mvc;
using FidsSystem.Models;
using FidsSystem.Services;

namespace FidsSystem.Controllers
{
    public class AdsController : Controller
    {
        private readonly IAdsService _adsService;
        private readonly IZoneService _zoneService;

        public AdsController(IAdsService adsService, IZoneService zoneService)
        {
            _adsService = adsService;
            _zoneService = zoneService;
        }

        private IActionResult? RequireLogin() => RbacHelper.RequireAdmin(HttpContext, this);

        public async Task<IActionResult> Index(string? search, int? month, int? year)
        {
            var r = RequireLogin(); if (r != null) return r;
            var now = DateTime.Now;
            month ??= now.Month;
            year  ??= now.Year;
            var ads = await _adsService.GetAllAdsAsync(search, month, year);
            ViewBag.Search = search;
            ViewBag.Month = month;
            ViewBag.Year = year;
            return View(ads);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAd(Advertisement model, IFormFile? file)
        {
            var r = RequireLogin(); if (r != null) return r;
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                TempData["Error"] = "Title is required.";
                return RedirectToAction(nameof(Index));
            }

            if (file != null && file.Length > 0)
            {
                var allowed = new[] { ".mp4", ".webm", ".mov", ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowed.Contains(ext))
                {
                    TempData["Error"] = "File type not supported. Allowed: mp4, webm, mov, jpg, png, gif, webp";
                    return RedirectToAction(nameof(Index));
                }

                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "ads");
                Directory.CreateDirectory(uploadDir);

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadDir, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                model.FilePath = $"/uploads/ads/{fileName}";
                model.FileSizeMB = Math.Round((decimal)file.Length / 1_048_576, 2);
                model.FileType = ext is ".mp4" or ".webm" or ".mov" ? "Video" : "Image";
            }

            var now = DateTime.Now;
            model.Month = now.Month;
            model.Year  = now.Year;
            await _adsService.CreateAdAsync(model);
            TempData["Success"] = "Advertisement added.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAd(int id)
        {
            var r = RequireLogin(); if (r != null) return r;
            await _adsService.DeleteAdAsync(id);
            TempData["Success"] = "Advertisement deleted.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Playlists(string? search)
        {
            var r = RequireLogin(); if (r != null) return r;
            var playlists = await _adsService.GetAllPlaylistsAsync(search);
            ViewBag.Search = search;
            return View(playlists);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePlaylist(string name)
        {
            var r = RequireLogin(); if (r != null) return r;
            if (!string.IsNullOrWhiteSpace(name))
            {
                await _adsService.CreatePlaylistAsync(new AdPlaylist { Name = name });
                TempData["Success"] = "Playlist created.";
            }
            return RedirectToAction(nameof(Playlists));
        }

        [HttpPost]
        public async Task<IActionResult> DeletePlaylist(int id)
        {
            var r = RequireLogin(); if (r != null) return r;
            await _adsService.DeletePlaylistAsync(id);
            TempData["Success"] = "Playlist deleted.";
            return RedirectToAction(nameof(Playlists));
        }

        public async Task<IActionResult> MatchDisplay(string? search, int? zone, string? status)
        {
            var r = RequireLogin(); if (r != null) return r;
            var matches = await _adsService.GetAllMatchesAsync(search, zone, status);
            // Load zones for filter dropdown
            var zones = await _zoneService.GetAllAsync();
            var playlists = await _adsService.GetAllPlaylistsAsync();
            ViewBag.Search = search;
            ViewBag.ZoneFilter = zone;
            ViewBag.StatusFilter = status;
            ViewBag.Zones = zones;
            ViewBag.Playlists = playlists;
            return View(matches);
        }

        [HttpPost]
        public async Task<IActionResult> SetMatchByDisplay(int displayId, int playlistId)
        {
            var r = RequireLogin(); if (r != null) return r;
            await _adsService.SetMatchByDisplayAsync(displayId, playlistId);
            TempData["Success"] = "Playlist matched successfully.";
            return RedirectToAction(nameof(MatchDisplay));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveMatchByDisplay(int displayId)
        {
            var r = RequireLogin(); if (r != null) return r;
            await _adsService.RemoveMatchByDisplayAsync(displayId);
            TempData["Success"] = "Match removed.";
            return RedirectToAction(nameof(MatchDisplay));
        }
    }
}
