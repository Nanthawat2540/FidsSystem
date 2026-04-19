using Dapper;
using FidsSystem.Helpers;
using Microsoft.AspNetCore.Mvc;
using FidsSystem.Models;
using FidsSystem.Services;

namespace FidsSystem.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IFlightService _flightService;
        private readonly IConfiguration _config;

        public ReportsController(IFlightService flightService, IConfiguration config)
        {
            _flightService = flightService;
            _config = config;
        }

        private IActionResult? RequireLogin() => RbacHelper.RequireAdminOrStaff(HttpContext, this);

        public async Task<IActionResult> Index(string? date)
        {
            var r = RequireLogin(); if (r != null) return r;

            var reportDate = string.IsNullOrEmpty(date)
                ? DateTime.Today
                : DateTime.Parse(date);

            var connStr = _config.GetConnectionString("DefaultConnection")!;
            using var conn = new Microsoft.Data.SqlClient.SqlConnection(connStr);

            var multi = await conn.QueryMultipleAsync("sp_Report_FlightSummary",
                new { Date = DateOnly.FromDateTime(reportDate) },
                commandType: System.Data.CommandType.StoredProcedure);

            var report = new FlightReportData
            {
                ReportDate = reportDate
            };

            var summary = await multi.ReadFirstOrDefaultAsync<dynamic>();
            if (summary != null)
            {
                report.TotalDepartures = (int)(summary.TotalDepartures ?? 0);
                report.TotalArrivals   = (int)(summary.TotalArrivals   ?? 0);
                report.OnTime          = (int)(summary.OnTime          ?? 0);
                report.Delayed         = (int)(summary.Delayed         ?? 0);
                report.Cancelled       = (int)(summary.Cancelled       ?? 0);
                report.Boarded         = (int)(summary.Boarded         ?? 0);
            }

            report.ByAirline  = (await multi.ReadAsync<AirlineStat>()).ToList();
            report.ByStatus   = (await multi.ReadAsync<StatusStat>()).ToList();
            report.RecentDelays = (await multi.ReadAsync<Flight>()).ToList();

            ViewBag.ReportDate = reportDate.ToString("yyyy-MM-dd");
            return View(report);
        }

        [HttpGet]
        public async Task<IActionResult> ExportCsv(string? date)
        {
            var r = RequireLogin(); if (r != null) return r;

            var reportDate = string.IsNullOrEmpty(date)
                ? DateTime.Today
                : DateTime.Parse(date);

            var all = await _flightService.GetAllFlightsAsync();
            var flights = all.Where(f => f.ScheduledTime.Date == reportDate.Date)
                             .OrderBy(f => f.ScheduledTime).ToList();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("FlightNumber,FlightType,Airline,Origin,Destination,ScheduledTime,EstimatedTime,Gate,Belt,Status,Remark");
            foreach (var f in flights)
            {
                sb.AppendLine(string.Join(",",
                    Esc(f.FlightNumber), Esc(f.FlightType), Esc(f.Airline), Esc(f.Origin),
                    Esc(f.Destination), f.ScheduledTime.ToString("yyyy-MM-dd HH:mm"),
                    f.EstimatedTime?.ToString("yyyy-MM-dd HH:mm") ?? "",
                    Esc(f.Gate), Esc(f.Belt), Esc(f.Status), Esc(f.Remark)));
            }

            var bytes = System.Text.Encoding.UTF8.GetPreamble()
                .Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString()))
                .ToArray();

            return File(bytes, "text/csv", $"flights_{reportDate:yyyy-MM-dd}.csv");
        }

        private static string Esc(string? s)
        {
            if (s == null) return "";
            if (s.Contains(',') || s.Contains('"') || s.Contains('\n'))
                return $"\"{s.Replace("\"", "\"\"")}\"";
            return s;
        }
    }
}
