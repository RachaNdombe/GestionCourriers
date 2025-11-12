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

        var roles = new[] { "Stat", "Classement", "Admin", "Indexateur", "SuperUser", "ServiceExpéditeur", "Archiviste", "Viseur", "Signataire" };
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
            // Utiliser le contexte existant
            using var adminContext = scope.ServiceProvider.GetRequiredService<ProjetIdDbContext>();
            
            // Créer un service par défaut s'il n'en existe pas
            var existingService = adminContext.Services!.FirstOrDefault();
            if (existingService == null)
            {
                existingService = new Service
                {
                    Nom = "Service Administratif",
                    Code = "SERV-ADMIN",
                    Description = "Service administratif par défaut",
                    Actif = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow
                };
                adminContext.Services!.Add(existingService);
                adminContext.SaveChanges();
            }

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
                CreatedAt = DateTime.UtcNow,
                ServiceId = existingService.Id
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
            // Utiliser le contexte existant
            using var indexContext = scope.ServiceProvider.GetRequiredService<ProjetIdDbContext>();
            
            // Créer un service par défaut s'il n'en existe pas
            var existingService = indexContext.Services!.FirstOrDefault();
            if (existingService == null)
            {
                existingService = new Service
                {
                    Nom = "Service Indexation",
                    Code = "SERV-INDEX",
                    Description = "Service d'indexation par défaut",
                    Actif = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow
                };
                indexContext.Services!.Add(existingService);
                indexContext.SaveChanges();
            }

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
                CreatedAt = DateTime.UtcNow,
                ServiceId = existingService.Id
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

        // Seed a Viseur user for testing
        var viseurEmail = builder.Configuration["ViseurUser:Email"] ?? "viseur@local";
        var viseurPassword = builder.Configuration["ViseurUser:Password"] ?? "Viseur123!";

        var viseurUser = userManager.FindByEmailAsync(viseurEmail).GetAwaiter().GetResult();
        if (viseurUser == null)
        {
            // Utiliser le contexte existant
            using var viseurContext = scope.ServiceProvider.GetRequiredService<ProjetIdDbContext>();
            
            // Créer un service par défaut s'il n'en existe pas
            var existingService = viseurContext.Services!.FirstOrDefault();
            if (existingService == null)
            {
                existingService = new Service
                {
                    Nom = "Service Viseur",
                    Code = "SERV-VISEUR",
                    Description = "Service de visa par défaut",
                    Actif = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow
                };
                viseurContext.Services!.Add(existingService);
                viseurContext.SaveChanges();
            }

            var vu = new User
            {
                UserName = viseurEmail,
                Email = viseurEmail,
                Nom = "Viseur",
                Prenom = "User",
                Societe = "Administration",
                Telephone = "0000000000",
                Role = UserRole.Viseur,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ServiceId = existingService.Id
            };

            var createdViseur = userManager.CreateAsync(vu, viseurPassword).GetAwaiter().GetResult();
            if (createdViseur.Succeeded)
            {
                userManager.AddToRoleAsync(vu, UserRole.Viseur.ToString()).GetAwaiter().GetResult();
                logger.LogInformation("Seeded viseur user '{Email}' with role Viseur (password: {Pwd}).", viseurEmail, viseurPassword);
            }
            else
            {
                logger.LogWarning("Failed to create viseur user: {Errors}", string.Join(';', createdViseur.Errors.Select(e => e.Description)));
            }
        }

        // Seed a ServiceExpéditeur user for testing (Responsable de service)
        var serviceEmail = builder.Configuration["ServiceUser:Email"] ?? "service@jconsult.com";
        var servicePassword = builder.Configuration["ServiceUser:Password"] ?? "Service123!";

        var serviceUser = userManager.FindByEmailAsync(serviceEmail).GetAwaiter().GetResult();
        if (serviceUser == null)
        {
            // Utiliser le contexte existant
            using var serviceContext = scope.ServiceProvider.GetRequiredService<ProjetIdDbContext>();
            
            // Trouver le service Algorithmique existant ou créer un service par défaut
            var existingService = serviceContext.Services!.FirstOrDefault(s => s.Nom!.Contains("Algorithmique"));
            if (existingService == null)
            {
                existingService = serviceContext.Services!.FirstOrDefault();
                if (existingService == null)
                {
                    existingService = new Service
                    {
                        Nom = "Service Algorithmique",
                        Code = "SERV-ALGO",
                        Description = "Service  algorithmique",
                        Actif = true,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    };
                    serviceContext.Services!.Add(existingService);
                    serviceContext.SaveChanges();
                }
            }

            var su = new User
            {
                UserName = serviceEmail,
                Email = serviceEmail,
                Nom = "Responsable",
                Prenom = "Service",
                Societe = "Administration",
                Telephone = "0000000000",
                Role = UserRole.ServiceExpéditeur,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ServiceId = existingService.Id
            };

            var createdService = userManager.CreateAsync(su, servicePassword).GetAwaiter().GetResult();
            if (createdService.Succeeded)
            {
                userManager.AddToRoleAsync(su, "ServiceExpéditeur").GetAwaiter().GetResult();
                
                // Mettre à jour le service avec le responsable
                existingService.Responsable = $"{su.Nom} {su.Prenom}";
                serviceContext.SaveChanges();
                
                logger.LogInformation("Seeded service user '{Email}' with role ServiceExpéditeur (password: {Pwd}) associated with service '{ServiceName}'.", serviceEmail, servicePassword, existingService.Nom);
            }
            else
            {
                logger.LogWarning("Failed to create service user: {Errors}", string.Join(';', createdService.Errors.Select(e => e.Description)));
            }

            // Seed an Archiviste user for testing
            var archivisteEmail = builder.Configuration["ArchivisteUser:Email"] ?? "archiviste@local";
            var archivistePassword = builder.Configuration["ArchivisteUser:Password"] ?? "Archive123!";

            var archivisteUser = userManager.FindByEmailAsync(archivisteEmail).GetAwaiter().GetResult();
            if (archivisteUser == null)
            {
                // Utiliser le contexte existant
                using var archivisteContext = scope.ServiceProvider.GetRequiredService<ProjetIdDbContext>();
                
                // Créer un service par défaut s'il n'en existe pas
                var archivisteService = archivisteContext.Services!.FirstOrDefault();
                if (archivisteService == null)
                {
                    archivisteService = new Service
                    {
                        Nom = "Service Archives",
                        Code = "SERV-ARCH",
                        Description = "Service d'archives par défaut",
                        Actif = true,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    };
                    archivisteContext.Services!.Add(archivisteService);
                    archivisteContext.SaveChanges();
                }

                var au = new User
                {
                    UserName = archivisteEmail,
                    Email = archivisteEmail,
                    Nom = "Archiviste",
                    Prenom = "User",
                    Societe = "Administration",
                    Telephone = "0000000000",
                    Role = UserRole.Archiviste,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ServiceId = archivisteService.Id
                };

                var createdArchiviste = userManager.CreateAsync(au, archivistePassword).GetAwaiter().GetResult();
                if (createdArchiviste.Succeeded)
                {
                    userManager.AddToRoleAsync(au, UserRole.Archiviste.ToString()).GetAwaiter().GetResult();
                    logger.LogInformation("Seeded archiviste user '{Email}' with role Archiviste (password: {Pwd}).", archivisteEmail, archivistePassword);
                }
                else
                {
                    logger.LogWarning("Failed to create archiviste user: {Errors}", string.Join(';', createdArchiviste.Errors.Select(e => e.Description)));
                }
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

// Map specific controller routes
app.MapControllerRoute(
    name: "correspondants",
    pattern: "Correspondants/{action=Index}/{id?}");

app.Run();

