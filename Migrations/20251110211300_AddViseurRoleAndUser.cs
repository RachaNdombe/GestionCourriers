using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.AspNetCore.Identity;
using JconsultGC.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

#nullable disable

namespace JconsultGC.Migrations
{
    public partial class AddViseurRoleAndUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cette migration ne fait rien dans la base de données car les rôles et utilisateurs
            // sont gérés par Identity, mais nous allons créer un script d'initialisation
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback - ne fait rien
        }
    }

    public static class ViseurSeeder
    {
        public static async Task SeedViseurAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            // Créer le rôle Viseur s'il n'existe pas
            if (!await roleManager.RoleExistsAsync("Viseur"))
            {
                await roleManager.CreateAsync(new IdentityRole("Viseur"));
            }

            // Créer l'utilisateur viseur s'il n'existe pas
            var viseurUser = await userManager.FindByEmailAsync("viseur@local");
            if (viseurUser == null)
            {
                var user = new User
                {
                    UserName = "viseur@local",
                    Email = "viseur@local",
                    Nom = "Viseur",
                    Prenom = "System",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Viseur123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Viseur");
                }
            }
        }
    }
}