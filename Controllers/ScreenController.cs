using Dapper;
using Microsoft.AspNetCore.Mvc;
using FidsSystem.Services;

namespace FidsSystem.Controllers
{
    public class ScreenController : Controller
    {
        private readonly IDisplayDeviceService _displayService;
        private readonly ITemplateService _templateService;
        private readonly IConfiguration _config;

        public ScreenController(
            IDisplayDeviceService displayService,
            ITemplateService templateService,
            IConfiguration config)
        {
            _displayService = displayService;
            _templateService = templateService;
            _config = config;
        }

        // GET /Screen/{id}  — full-screen display for physical device
        public async Task<IActionResult> Show(int id)
        {
            var device = await _displayService.GetByIdAsync(id);
            if (device == null) return NotFound();
            if (device.TemplateId.HasValue)
                ViewBag.Template = await _templateService.GetByIdAsync(device.TemplateId.Value);
            return View(device);
        }

        // POST /Screen/Heartbeat  — called by display devices every 30 sec
        [HttpPost]
        public async Task<IActionResult> Heartbeat([FromBody] HeartbeatRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.ScreenKey))
                return BadRequest();

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = Request.Headers.UserAgent.ToString();

            var connStr = _config.GetConnectionString("DefaultConnection")!;
            using var conn = new Microsoft.Data.SqlClient.SqlConnection(connStr);
            await conn.ExecuteAsync("sp_Heartbeat_Upsert",
                new { ScreenKey = req.ScreenKey, IpAddress = ip, UserAgent = ua },
                commandType: System.Data.CommandType.StoredProcedure);

            return Ok(new { ok = true, serverTime = DateTime.Now.ToString("HH:mm:ss") });
        }

        // GET /Screen/HeartbeatStatus — admin can poll this
        public async Task<IActionResult> HeartbeatStatus()
        {
            if (HttpContext.Session.GetString("UserId") == null)
                return Unauthorized();

            var connStr = _config.GetConnectionString("DefaultConnection")!;
            using var conn = new Microsoft.Data.SqlClient.SqlConnection(connStr);
            var rows = await conn.QueryAsync("sp_Heartbeat_GetAll",
                commandType: System.Data.CommandType.StoredProcedure);

            return Json(rows);
        }
    }

    public class HeartbeatRequest
    {
        public string ScreenKey { get; set; } = string.Empty;
    }
}
