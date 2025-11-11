using Microsoft.EntityFrameworkCore;
using JconsultGC.Data;
using JconsultGC.Models;
using Microsoft.AspNetCore.Identity;

namespace JconsultGC
{
    public class DiagnosticTool
    {
        public static async Task Run()
        {
            Console.WriteLine("=== Diagnostic Tool for JconsultGC ===");
            
            var optionsBuilder = new DbContextOptionsBuilder<ProjetIdDbContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=JconsultGC;Trusted_Connection=true;MultipleActiveResultSets=true");
            
            using var context = new ProjetIdDbContext(optionsBuilder.Options);
            
            // Check services
            var services = await context.Services!.ToListAsync();
            Console.WriteLine($"\nServices in database: {services.Count}");
            foreach (var service in services)
            {
                Console.WriteLine($"  - {service.Id}: {service.Nom} (Code: {service.Code}, Actif: {service.Actif})");
            }
            
            // Check users
            var users = await context.Users.ToListAsync();
            Console.WriteLine($"\nUsers in database: {users.Count}");
            foreach (var user in users)
            {
                Console.WriteLine($"  - {user.Id}: {user.UserName} ({user.Email})");
                Console.WriteLine($"    ServiceId: {user.ServiceId}, Role: {user.Role}");
            }
            
            // Check users without services
            var usersWithoutServices = users.Where(u => u.ServiceId == null).ToList();
            Console.WriteLine($"\nUsers WITHOUT service association: {usersWithoutServices.Count}");
            foreach (var user in usersWithoutServices)
            {
                Console.WriteLine($"  - {user.UserName} ({user.Email}) - Role: {user.Role}");
            }
            
            Console.WriteLine("\n=== Diagnostic Complete ===");
        }
    }
}