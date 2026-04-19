using Microsoft.AspNetCore.Mvc;

namespace FidsSystem.Helpers
{
    public static class RbacHelper
    {
        public static string? GetRole(HttpContext ctx) =>
            ctx.Session.GetString("Role");

        public static string? GetUserId(HttpContext ctx) =>
            ctx.Session.GetString("UserId");

        public static string? GetAirlineCode(HttpContext ctx) =>
            ctx.Session.GetString("AirlineCode");

        public static bool IsAdmin(HttpContext ctx) =>
            GetRole(ctx) == "ADMIN";

        public static bool IsStaff(HttpContext ctx) =>
            GetRole(ctx) == "STAFF";

        public static bool IsAirline(HttpContext ctx) =>
            GetRole(ctx) == "AIRLINE";

        public static bool IsAdminOrStaff(HttpContext ctx) =>
            IsAdmin(ctx) || IsStaff(ctx);

        public static bool CanManageFlights(HttpContext ctx) =>
            IsAdmin(ctx) || IsStaff(ctx);

        public static bool CanManageSystem(HttpContext ctx) =>
            IsAdmin(ctx);

        // Returns redirect if not logged in
        public static IActionResult? RequireLogin(HttpContext ctx, Controller ctrl)
        {
            if (GetUserId(ctx) == null)
                return ctrl.RedirectToAction("Login", "Account");
            return null;
        }

        // Returns redirect if not admin
        public static IActionResult? RequireAdmin(HttpContext ctx, Controller ctrl)
        {
            var login = RequireLogin(ctx, ctrl);
            if (login != null) return login;
            if (!IsAdmin(ctx))
                return ctrl.RedirectToAction("AccessDenied", "Account");
            return null;
        }

        // Returns redirect if not admin or staff
        public static IActionResult? RequireAdminOrStaff(HttpContext ctx, Controller ctrl)
        {
            var login = RequireLogin(ctx, ctrl);
            if (login != null) return login;
            if (!IsAdminOrStaff(ctx))
                return ctrl.RedirectToAction("AccessDenied", "Account");
            return null;
        }

        // Returns redirect if not logged in (any role allowed)
        public static IActionResult? RequireAnyRole(HttpContext ctx, Controller ctrl)
        {
            return RequireLogin(ctx, ctrl);
        }
    }
}
