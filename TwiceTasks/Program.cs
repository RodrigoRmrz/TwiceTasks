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
// Notas viven en /Notes (NotesController)
// y las operaciones CRUD siguen en PagesController.


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// =============================
// RUN
// =============================
app.Run();
