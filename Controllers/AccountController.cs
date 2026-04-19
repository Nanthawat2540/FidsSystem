using Microsoft.AspNetCore.Mvc;
using FidsSystem.Models;
using FidsSystem.Services;

namespace FidsSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService) =>
            _authService = authService;

        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserId") != null)
                return RedirectToDefault();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "กรุณากรอกข้อมูลให้ครบ");
                return View();
            }

            var user = await _authService.AuthenticateAsync(username, password);
            if (user == null)
            {
                ModelState.AddModelError("", "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง");
                return View();
            }

            HttpContext.Session.SetString("UserId",      user.Id.ToString());
            HttpContext.Session.SetString("Username",    user.Username);
            HttpContext.Session.SetString("Role",        user.Role);
            HttpContext.Session.SetString("AirlineCode", user.AirlineCode ?? "");

            return RedirectToDefault();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            ViewBag.Role = HttpContext.Session.GetString("Role") ?? "-";
            return View();
        }

        private IActionResult RedirectToDefault()
        {
            var role = HttpContext.Session.GetString("Role");
            return role switch
            {
                Roles.Airline => RedirectToAction("Index", "Flight"),
                _             => RedirectToAction("Index", "Zone")
            };
        }
    }
}
