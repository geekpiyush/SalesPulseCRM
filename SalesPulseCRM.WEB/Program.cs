
using Microsoft.EntityFrameworkCore;
using SalesPulseCRM.Infrastructure.DB;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Add MVC (instead of only Razor Pages)
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<CrmDbContext>(option=>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});
var app = builder.Build();

// 🔹 Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// 🔹 MVC Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();