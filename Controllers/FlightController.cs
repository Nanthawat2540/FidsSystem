using Microsoft.AspNetCore.Mvc;
using FidsSystem.Helpers;
using FidsSystem.Models;
using FidsSystem.Services;

namespace FidsSystem.Controllers
{
    public class FlightController : Controller
    {
        private readonly IFlightService     _flightService;
        private readonly IPermissionService _permSvc;

        public FlightController(IFlightService flightService, IPermissionService permSvc)
        {
            _flightService = flightService;
            _permSvc = permSvc;
        }

        private IActionResult? RequireLogin()   => RbacHelper.RequireAnyRole(HttpContext, this);
        private IActionResult? RequireOperator() => RbacHelper.RequireAdminOrStaff(HttpContext, this);

        private string CurrentUser =>
            HttpContext.Session.GetString("Username") ?? "system";

        private string? MyAirlineCode =>
            RbacHelper.IsAirline(HttpContext) ? RbacHelper.GetAirlineCode(HttpContext) : null;

        // ── Admin Views ───────────────────────────────────────────

        public async Task<IActionResult> Index(string? search, string? flightType, string? status)
        {
            var r = RequireLogin(); if (r != null) return r;
            var flights = await _flightService.GetAllFlightsAsync();

            // AIRLINE role: only sees own airline
            if (MyAirlineCode is { } code && !string.IsNullOrEmpty(code))
                flights = flights.Where(f =>
                    string.Equals(f.Airline, code, StringComparison.OrdinalIgnoreCase) ||
                    (f.FlightNumber ?? "").StartsWith(code, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(search))
                flights = flights.Where(f =>
                    f.FlightNumber.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (f.Airline ?? "").Contains(search, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(flightType))
                flights = flights.Where(f => f.FlightType == flightType);
            if (!string.IsNullOrWhiteSpace(status))
                flights = flights.Where(f => f.Status == status);

            var uid  = int.TryParse(HttpContext.Session.GetString("UserId"), out var _p) ? _p : 0;
            var role = HttpContext.Session.GetString("Role") ?? "";
            var perms = await _permSvc.GetUserPermissionsAsync(uid, role);

            ViewBag.Search         = search;
            ViewBag.FlightType     = flightType;
            ViewBag.Status         = status;
            ViewBag.CanEdit        = RbacHelper.CanManageFlights(HttpContext);
            ViewBag.CanCreate      = perms.Contains(PermKeys.FeatFlightCreate);
            ViewBag.CanEditFlight  = perms.Contains(PermKeys.FeatFlightEdit);
            ViewBag.CanDelete      = perms.Contains(PermKeys.FeatFlightDelete);
            ViewBag.CanBulk        = perms.Contains(PermKeys.FeatFlightBulk);
            return View(flights.ToList());
        }

        public IActionResult Create()
        {
            var r = RequireOperator(); if (r != null) return r;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Flight flight)
        {
            var r = RequireOperator(); if (r != null) return r;
            if (!ModelState.IsValid) return View(flight);
            await _flightService.CreateFlightAsync(flight, CurrentUser);
            TempData["Success"] = "Flight added successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var r = RequireOperator(); if (r != null) return r;
            var flight = await _flightService.GetFlightByIdAsync(id);
            if (flight == null) return NotFound();
            return View(flight);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Flight flight)
        {
            var r = RequireOperator(); if (r != null) return r;
            if (id != flight.Id) return NotFound();
            if (!ModelState.IsValid) return View(flight);
            await _flightService.UpdateFlightAsync(flight, CurrentUser);
            TempData["Success"] = "Flight updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var r = RequireOperator(); if (r != null) return r;
            await _flightService.DeleteFlightAsync(id);
            TempData["Success"] = "Flight deleted.";
            return RedirectToAction(nameof(Index));
        }

        // ── Quick Actions (AJAX) ──────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (!RbacHelper.CanManageFlights(HttpContext)) return Forbid();
            await _flightService.UpdateStatusAsync(id, status, CurrentUser);
            return Ok(new { ok = true });
        }

        [HttpPost]
        public async Task<IActionResult> BulkUpdateStatus([FromBody] BulkStatusRequest req)
        {
            if (!RbacHelper.CanManageFlights(HttpContext)) return Forbid();
            if (req.Ids == null || !req.Ids.Any())
                return BadRequest(new { error = "No flights selected." });
            await _flightService.BulkUpdateStatusAsync(req.Ids, req.Status, CurrentUser);
            return Ok(new { ok = true, count = req.Ids.Count() });
        }

        [HttpGet]
        public async Task<IActionResult> History(int id)
        {
            var r = RequireLogin(); if (r != null) return r;
            var flight = await _flightService.GetFlightByIdAsync(id);
            if (flight == null) return NotFound();
            var history = await _flightService.GetStatusHistoryAsync(id);
            ViewBag.Flight = flight;
            return View(history);
        }

        // ── JSON API for display screens ──────────────────────────

        [HttpGet]
        public async Task<IActionResult> Api(string? type, string? gate, string? belt)
        {
            var flights = await _flightService.GetAllFlightsAsync();
            var query = flights.Where(f =>
                f.ScheduledTime.Date == DateTime.Today ||
                (f.EstimatedTime.HasValue && f.EstimatedTime.Value.Date == DateTime.Today));

            if (!string.IsNullOrEmpty(type))
                query = query.Where(f => f.FlightType == type);
            if (!string.IsNullOrEmpty(gate))
                query = query.Where(f => f.Gate == gate);
            if (!string.IsNullOrEmpty(belt))
                query = query.Where(f => f.Belt == belt);

            return Json(query.OrderBy(f => f.ScheduledTime)
                .Select(f => new {
                    f.Id, f.FlightNumber, f.Airline, f.Origin, f.Destination,
                    f.Gate, f.Belt, f.FlightType, f.Status, f.Remark,
                    Scheduled = f.ScheduledTime.ToString("HH:mm"),
                    Estimated = f.EstimatedTime?.ToString("HH:mm")
                }));
        }

        // ── Display Views (no login) ──────────────────────────────

        public async Task<IActionResult> Display(string type = "Departure", string o = "h")
        {
            var flights = await _flightService.GetAllFlightsAsync();
            var filtered = flights.Where(f => f.FlightType == type)
                                  .OrderBy(f => f.ScheduledTime).ToList();
            ViewBag.Type = type;
            ViewBag.Orient = o;
            return View(filtered);
        }

        public async Task<IActionResult> DisplayCheckin(int? flightId, string o = "h")
        {
            Flight? flight = null;
            if (flightId.HasValue)
                flight = await _flightService.GetFlightByIdAsync(flightId.Value);
            ViewBag.Orient = o;
            return View(flight);
        }

        public async Task<IActionResult> DisplayGate(string? gate, string o = "v")
        {
            var flights = await _flightService.GetAllFlightsAsync();
            IEnumerable<Flight> filtered = flights
                .Where(f => f.FlightType == "Departure")
                .OrderBy(f => f.ScheduledTime);
            if (!string.IsNullOrWhiteSpace(gate))
                filtered = filtered.Where(f => f.Gate == gate);
            ViewBag.Gate = gate ?? "-";
            ViewBag.Orient = o;
            return View(filtered.Take(15).ToList());
        }

        public async Task<IActionResult> DisplayBelt(string? belt, string o = "h")
        {
            var flights = await _flightService.GetAllFlightsAsync();
            IEnumerable<Flight> filtered = flights
                .Where(f => f.FlightType == "Arrival")
                .OrderBy(f => f.ScheduledTime);
            if (!string.IsNullOrWhiteSpace(belt))
                filtered = filtered.Where(f => f.Belt == belt);
            ViewBag.Belt = belt ?? "1";
            ViewBag.Orient = o;
            return View(filtered.FirstOrDefault());
        }
    }

    public class BulkStatusRequest
    {
        public IEnumerable<int> Ids { get; set; } = [];
        public string Status { get; set; } = string.Empty;
    }
}
