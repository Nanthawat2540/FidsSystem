namespace FidsSystem.Models
{
    public static class PermKeys
    {
        // ── Menus ──────────────────────────────────────────────
        public const string MenuTemplate    = "menu_template";
        public const string MenuDisplay     = "menu_display";
        public const string MenuFlight      = "menu_flight";
        public const string MenuReports     = "menu_reports";
        public const string MenuAlerts      = "menu_alerts";
        public const string MenuAds         = "menu_ads";
        public const string MenuZone        = "menu_zone";
        public const string MenuDevice      = "menu_device";
        public const string MenuAdmin       = "menu_admin";
        public const string MenuSystem      = "menu_system";
        public const string MenuPermissions = "menu_permissions";

        // ── Features ───────────────────────────────────────────
        public const string FeatFlightCreate = "feat_flight_create";
        public const string FeatFlightEdit   = "feat_flight_edit";
        public const string FeatFlightDelete = "feat_flight_delete";
        public const string FeatFlightBulk   = "feat_flight_bulk";
        public const string FeatAdsUpload    = "feat_ads_upload";

        public static readonly string[] All =
        [
            MenuTemplate, MenuDisplay, MenuFlight, MenuReports, MenuAlerts,
            MenuAds, MenuZone, MenuDevice, MenuAdmin, MenuSystem, MenuPermissions,
            FeatFlightCreate, FeatFlightEdit, FeatFlightDelete, FeatFlightBulk, FeatAdsUpload
        ];

        // Panel UI metadata — order must match All[]
        public static readonly (string Group, string Label, string Icon, string[] Roles)[] Definitions =
        [
            ("Menu",    "Template",           "bi-grid-1x2",       [Roles.Admin]),
            ("Menu",    "Display Management", "bi-display",        [Roles.Admin]),
            ("Menu",    "Flight Management",  "bi-airplane",       [Roles.Admin, Roles.Staff, Roles.Airline]),
            ("Menu",    "Reports",            "bi-bar-chart-line", [Roles.Admin, Roles.Staff]),
            ("Menu",    "Alerts",             "bi-bell",           [Roles.Admin, Roles.Staff]),
            ("Menu",    "Ads & PA",           "bi-megaphone",      [Roles.Admin, Roles.Staff]),
            ("Menu",    "Zone",               "bi-diagram-3",      [Roles.Admin]),
            ("Menu",    "Device",             "bi-tv",             [Roles.Admin]),
            ("Menu",    "Admin Management",   "bi-people",         [Roles.Admin]),
            ("Menu",    "System Management",  "bi-sliders",        [Roles.Admin]),
            ("Menu",    "Permissions",        "bi-shield-lock",    [Roles.Admin]),
            ("Feature", "Create Flights",     "bi-plus-circle",    [Roles.Admin, Roles.Staff]),
            ("Feature", "Edit Flights",       "bi-pencil",         [Roles.Admin, Roles.Staff]),
            ("Feature", "Delete Flights",     "bi-trash",          [Roles.Admin, Roles.Staff]),
            ("Feature", "Bulk Edit Flights",  "bi-check2-all",     [Roles.Admin, Roles.Staff]),
            ("Feature", "Upload Ads",         "bi-cloud-upload",   [Roles.Admin, Roles.Staff]),
        ];

        // Returns whether a key is enabled by default for a given role
        public static bool RoleDefault(string key, string role)
        {
            var idx = Array.IndexOf(All, key);
            if (idx < 0 || idx >= Definitions.Length) return false;
            return Array.IndexOf(Definitions[idx].Roles, role) >= 0;
        }
    }
}
