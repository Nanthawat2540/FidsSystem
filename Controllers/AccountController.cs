using Microsoft.AspNetCore.Mvc;
using FidsSystem.Helpers;
using FidsSystem.Models;
using FidsSystem.Services;

namespace FidsSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService      _authService;
        private readonly IAdminUserService _userService;

        public AccountController(IAuthService authService, IAdminUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

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

        public IActionResult ChangePassword()
        {
            var r = RbacHelper.RequireAnyRole(HttpContext, this);
            if (r != null) return r;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var r = RbacHelper.RequireAnyRole(HttpContext, this);
            if (r != null) return r;

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "New password and confirm password do not match.";
                return View();
            }
            if (newPassword.Length < 6)
            {
                TempData["Error"] = "Password must be at least 6 characters.";
                return View();
            }

            var username = HttpContext.Session.GetString("Username")!;
            var verified = await _authService.AuthenticateAsync(username, currentPassword);
            if (verified == null)
            {
                TempData["Error"] = "Current password is incorrect.";
                return View();
            }

            var userId = int.Parse(HttpContext.Session.GetString("UserId")!);
            var user   = await _userService.GetByIdAsync(userId);
            if (user == null) return NotFound();

            await _userService.UpdateAsync(user, newPassword);
            TempData["Success"] = "Password changed successfully.";
            return RedirectToAction(nameof(ChangePassword));
        }

        private IActionResult RedirectToDefault()
        {
            var role = HttpContext.Session.GetString("Role");
            return role switch
            {
                Roles.Airline => RedirectToAction("Index", "Flight"),
                _             => RedirectToAction("Index", "Dashboard")
            };
        }
    }
}
