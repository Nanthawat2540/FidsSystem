using FidsSystem.Hubs;
using FidsSystem.Services;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// SignalR
builder.Services.AddSignalR();

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", o =>
    {
        o.PermitLimit = 10;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });
    options.RejectionStatusCode = 429;
});

// HTTP security headers
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

// Register Dapper services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IZoneService, ZoneService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IDisplayDeviceService, DisplayDeviceService>();
builder.Services.AddScoped<IAdsService, AdsService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<ISystemService, SystemService>();
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();

// Background services
builder.Services.AddHostedService<AutoStatusService>();

// File upload size limit (50 MB)
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 52_428_800;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Account/Login");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseSession();
app.UseAuthorization();

// Security headers middleware
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
    ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
}));

// SignalR Hub
app.MapHub<FlightHub>("/hubs/flight");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
