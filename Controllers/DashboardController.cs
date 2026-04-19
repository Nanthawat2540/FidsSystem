using FidsSystem.Helpers;
using FidsSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace FidsSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IFlightService _flightService;
        private readonly IAlertService  _alertService;

        public DashboardController(IFlightService flightService, IAlertService alertService)
        {
            _flightService = flightService;
            _alertService  = alertService;
        }

        public async Task<IActionResult> Index()
        {
            var r = RbacHelper.RequireAnyRole(HttpContext, this);
            if (r != null) return r;

            var all   = await _flightService.GetAllFlightsAsync();
            var today = all.Where(f => f.ScheduledTime.Date == DateTime.Today).ToList();

            var deps  = today.Where(f => f.FlightType == "Departure").ToList();
            var arrs  = today.Where(f => f.FlightType == "Arrival").ToList();
            var now   = DateTime.Now;

            ViewBag.TotalDep    = deps.Count;
            ViewBag.TotalArr    = arrs.Count;
            ViewBag.OnTime      = today.Count(f => f.Status == "On Time");
            ViewBag.Boarding    = today.Count(f => f.Status is "Boarding" or "Final Call");
            ViewBag.Delayed     = today.Count(f => f.Status == "Delayed");
            ViewBag.Cancelled   = today.Count(f => f.Status == "Cancelled");
            ViewBag.Departed    = today.Count(f => f.Status == "Departed");
            ViewBag.Landed      = today.Count(f => f.Status == "Landed");

            // Upcoming departures (next 3 hours, not cancelled/departed)
            ViewBag.UpcomingDep = deps
                .Where(f => f.ScheduledTime >= now && f.ScheduledTime <= now.AddHours(3)
                         && f.Status != "Cancelled" && f.Status != "Departed")
                .OrderBy(f => f.ScheduledTime)
                .Take(8).ToList();

            // Upcoming arrivals (next 3 hours, not cancelled/landed)
            ViewBag.UpcomingArr = arrs
                .Where(f => f.ScheduledTime >= now && f.ScheduledTime <= now.AddHours(3)
                         && f.Status != "Cancelled" && f.Status != "Landed")
                .OrderBy(f => f.ScheduledTime)
                .Take(8).ToList();

            // Active alerts
            var alerts = await _alertService.GetActiveAlertsAsync();
            ViewBag.Alerts = alerts.Take(5).ToList();

            return View();
        }
    }
}
