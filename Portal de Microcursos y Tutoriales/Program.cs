using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Portal_de_Microcursos_y_Tutoriales.Data;
using Portal_de_Microcursos_y_Tutoriales.Models;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Cadena de conexión ─────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");

// ── 2. DbContext (debe ir antes que Identity) ─────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ── 3. Identity con roles ─────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit           = true;
    options.Password.RequiredLength         = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase       = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();           // páginas de Login/Register generadas por Identity UI

// ── 4. MVC + Razor Pages ──────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();          // necesario para Identity UI

// ── Pipeline ──────────────────────────────────────────────────────────────────
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();   // debe ir ANTES de UseAuthorization
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();       // rutas de Identity UI (/Identity/Account/Login, etc.)

// ── Seed de roles y usuario admin ─────────────────────────────────────────────
await SeedAsync(app);

app.Run();

// ── Método seed ───────────────────────────────────────────────────────────────
static async Task SeedAsync(WebApplication app)
{
    using var scope       = app.Services.CreateScope();
    var roleManager       = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager       = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Aplicar migraciones pendientes automáticamente
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();


    // Crear roles si no existen
    string[] roles = ["Administrador", "Suscriptor"];
    foreach (var rol in roles)
    {
        if (!await roleManager.RoleExistsAsync(rol))
            await roleManager.CreateAsync(new IdentityRole(rol));
    }

    // Crear usuario administrador por defecto
    const string adminEmail    = "admin@portal.com";
    const string adminPassword = "Admin123!";

    if (await userManager.FindByEmailAsync(adminEmail) is null)
    {
        var admin = new ApplicationUser
        {
            UserName       = adminEmail,
            Email          = adminEmail,
            NombreCompleto = "Administrador del Portal",
            EmailConfirmed = true
        };

        var resultado = await userManager.CreateAsync(admin, adminPassword);
        if (resultado.Succeeded)
            await userManager.AddToRoleAsync(admin, "Administrador");
    }
}
