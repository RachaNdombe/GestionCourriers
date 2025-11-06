using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using JconsultGC.Models;
using JconsultGC.Data;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ProjetIdDbContext>(
    options => options.UseSqlServer(connectionString),
    ServiceLifetime.Transient
);

// Configuration Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ProjetIdDbContext>()
.AddDefaultTokenProviders();

// Configuration des cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configuration de l'authentification cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

// Seed default roles and an administrator account at startup (development convenience)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        var roles = new[] { "Stat", "Classement", "Admin", "Indexateur", "SuperUser", "ServiceExp", "Archiviste", "Viseur", "Signataire" };
        foreach (var roleName in roles)
        {
            var exists = roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult();
            if (!exists)
            {
                var r = roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
                if (!r.Succeeded)
                {
                    logger.LogWarning("Unable to create role {Role}: {Errors}", roleName, string.Join(';', r.Errors.Select(e => e.Description)));
                }
            }
        }

        // Admin user (defaults - change in production via configuration)
        var adminEmail = builder.Configuration["AdminUser:Email"] ?? "admin@local";
        var adminPassword = builder.Configuration["AdminUser:Password"] ?? "Admin123!";

        var admin = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
        if (admin == null)
        {
            var user = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                Nom = "Admin",
                Prenom = "System",
                Societe = "Administration",
                Telephone = "0000000000",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var created = userManager.CreateAsync(user, adminPassword).GetAwaiter().GetResult();
            if (created.Succeeded)
            {
                userManager.AddToRoleAsync(user, UserRole.Admin.ToString()).GetAwaiter().GetResult();
                logger.LogInformation("Seeded admin user '{Email}' with role Admin (password: {Pwd}).", adminEmail, adminPassword);
            }
            else
            {
                logger.LogWarning("Failed to create admin user: {Errors}", string.Join(';', created.Errors.Select(e => e.Description)));
            }
        }

        // Seed an Indexateur user for testing
        var indexEmail = builder.Configuration["IndexateurUser:Email"] ?? "indexateur@local";
        var indexPassword = builder.Configuration["IndexateurUser:Password"] ?? "Index123!";

        var indexUser = userManager.FindByEmailAsync(indexEmail).GetAwaiter().GetResult();
        if (indexUser == null)
        {
            var iu = new User
            {
                UserName = indexEmail,
                Email = indexEmail,
                Nom = "Index",
                Prenom = "User",
                Societe = "Administration",
                Telephone = "0000000000",
                Role = UserRole.Indexateur,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createdIndex = userManager.CreateAsync(iu, indexPassword).GetAwaiter().GetResult();
            if (createdIndex.Succeeded)
            {
                userManager.AddToRoleAsync(iu, UserRole.Indexateur.ToString()).GetAwaiter().GetResult();
                logger.LogInformation("Seeded indexateur user '{Email}' with role Indexateur (password: {Pwd}).", indexEmail, indexPassword);
            }
            else
            {
                logger.LogWarning("Failed to create indexateur user: {Errors}", string.Join(';', createdIndex.Errors.Select(e => e.Description)));
            }
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error seeding roles/admin user");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
