using Hangfire;
using Microsoft.EntityFrameworkCore;
using SalesPulseCRM.Application.ServiceContracts;
using SalesPulseCRM.Application.Services;
using SalesPulseCRM.Infrastructure.DB;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// DB
builder.Services.AddDbContext<CrmDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddHangfire(config =>
    {
        config.UseSqlServerStorage(builder.Configuration.GetConnectionString("Default"));
    });

builder.Services.AddHangfireServer();

// Session (FIXED)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<ILeadService , LeadService>();

// Auth (ADD THIS)
builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";

        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddScoped<EmailServices>();


var app = builder.Build(); 

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ⚠️ Comment for now (no SSL yet)
// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Auth order (IMPORTANT)
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();
app.UseHangfireDashboard();

// Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();