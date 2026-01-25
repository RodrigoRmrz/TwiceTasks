using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TwiceTasks.Data;
using TwiceTasks.Models;

var builder = WebApplication.CreateBuilder(args);

// =============================
// DATABASE
// =============================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// =============================
// IDENTITY
// =============================

// ✅ Usamos ApplicationUser (IMPORTANTE)
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// =============================
// MVC + RAZOR
// =============================

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// =============================
// BUILD APP
// =============================
var app = builder.Build();

// =============================
// MIDDLEWARE
// =============================

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


// 🔐 AUTH
app.UseAuthentication();
app.UseAuthorization();

// =============================
// ROUTING
// =============================
// ✅ Alias duro para /Notes
// A veces los enlaces apuntan a /Notes aunque el controlador real sea Pages.
// Esto evita 404 incluso si cambian rutas convencionales.
app.MapGet("/Notes", () => Results.Redirect("/Pages"));
app.MapGet("/Notes/{**rest}", () => Results.Redirect("/Pages"));

// Alias convencional adicional: /Notes/... -> PagesController
app.MapControllerRoute(
    name: "notes",
    pattern: "Notes/{action=Index}/{id?}",
    defaults: new { controller = "Pages" });


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// =============================
// RUN
// =============================
app.Run();
