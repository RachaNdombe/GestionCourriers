using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JconsultGC.Models;
using JconsultGC.Data;

// Script de diagnostic rapide pour vérifier l'état des utilisateurs et services
public class QuickDiagnostic
{
    public static async Task Run()
    {
        var builder = WebApplication.CreateBuilder();
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        
        builder.Services.AddDbContext<ProjetIdDbContext>(options => 
            options.UseSqlServer(connectionString));
            
        builder.Services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<ProjetIdDbContext>()
            .AddDefaultTokenProviders();

        var app = builder.Build();
        
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProjetIdDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        Console.WriteLine("=== DIAGNOSTIC RAPIDE ===");
        
        // 1. Vérifier les services
        var services = await context.Services!.ToListAsync();
        Console.WriteLine($"Services trouvés: {services.Count}");
        foreach (var service in services)
        {
            Console.WriteLine($"- Service: {service.Nom} (ID: {service.Id})");
        }
        
        // 2. Vérifier les rôles
        var roles = await roleManager.Roles.ToListAsync();
        Console.WriteLine($"Rôles trouvés: {roles.Count}");
        foreach (var role in roles)
        {
            Console.WriteLine($"- Rôle: {role.Name}");
        }
        
        // 3. Vérifier l'utilisateur service
        var serviceUser = await userManager.FindByEmailAsync("service@jconsult.com");
        if (serviceUser != null)
        {
            Console.WriteLine($"Utilisateur service trouvé: {serviceUser.Email}");
            Console.WriteLine($"ServiceId: {serviceUser.ServiceId}");
            Console.WriteLine($"Rôle: {serviceUser.Role}");
            
            // Vérifier les rôles ASP.NET Identity
            var userRoles = await userManager.GetRolesAsync(serviceUser);
            Console.WriteLine($"Rôles ASP.NET: {string.Join(", ", userRoles)}");
            
            // Vérifier si le service existe
            if (serviceUser.ServiceId.HasValue)
            {
                var userService = await context.Services!.FindAsync(serviceUser.ServiceId.Value);
                Console.WriteLine($"Service associé: {userService?.Nom ?? "NON TROUVÉ"}");
            }
            else
            {
                Console.WriteLine("AUCUN SERVICE ASSOCIÉ");
            }
        }
        else
        {
            Console.WriteLine("UTILISATEUR SERVICE NON TROUVÉ");
        }
        
        Console.WriteLine("=== FIN DIAGNOSTIC ===");
    }
}