using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JconsultGC.Models;
using JconsultGC.Data;
using Microsoft.AspNetCore.Authorization;

namespace JconsultGC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FixExistingUsersController : Controller
    {
        private readonly ProjetIdDbContext _context;
        private readonly UserManager<User> _userManager;

        public FixExistingUsersController(ProjetIdDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var results = new List<string>();

            try
            {
                // 1. Vérifier les services existants
                var services = await _context.Services.Where(s => s.Actif).ToListAsync();
                results.Add($"Services actifs trouvés: {services.Count}");
                
                foreach (var service in services)
                {
                    results.Add($"Service: {service.Id} - {service.Nom}");
                }

                // 2. Vérifier tous les utilisateurs sans ServiceId
                var usersWithoutService = await _userManager.Users
                    .Where(u => u.ServiceId == null)
                    .ToListAsync();
                
                results.Add($"Utilisateurs sans service: {usersWithoutService.Count}");

                // 3. Créer un service par défaut s'il n'existe pas
                if (!services.Any())
                {
                    var defaultService = new Service
                    {
                        Nom = "Service par défaut",
                        Code = "DEFAULT",
                        Description = "Service créé automatiquement pour les utilisateurs existants",
                        Actif = true
                    };
                    
                    _context.Services.Add(defaultService);
                    await _context.SaveChangesAsync();
                    results.Add("Service par défaut créé avec ID: " + defaultService.Id);
                    
                    services = await _context.Services.Where(s => s.Actif).ToListAsync();
                }

                // 4. Associer tous les utilisateurs sans service au premier service disponible
                var firstService = services.First();
                int fixedCount = 0;
                
                foreach (var user in usersWithoutService)
                {
                    user.ServiceId = firstService.Id;
                    var updateResult = await _userManager.UpdateAsync(user);
                    
                    if (updateResult.Succeeded)
                    {
                        results.Add($"✓ Utilisateur {user.Email} associé au service {firstService.Nom}");
                        fixedCount++;
                    }
                    else
                    {
                        results.Add($"✗ Erreur lors de l'association de {user.Email}: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
                    }
                }

                results.Add($"Total des associations corrigées: {fixedCount}");

                // 5. Vérifier l'état final
                var finalUsersWithoutService = await _userManager.Users
                    .Where(u => u.ServiceId == null)
                    .CountAsync();
                
                results.Add("=== ÉTAT FINAL ===");
                results.Add($"Utilisateurs restant sans service: {finalUsersWithoutService}");

                // 6. Vérifier par rôle
                var roles = new[] { "ServiceExpéditeur", "Indexateur", "Viseur", "Signataire" };
                foreach (var role in roles)
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                    var usersWithoutServiceInRole = usersInRole.Count(u => u.ServiceId == null);
                    results.Add($"Rôle {role}: {usersInRole.Count} utilisateurs, {usersWithoutServiceInRole} sans service");
                }

            }
            catch (Exception ex)
            {
                results.Add($"ERREUR: {ex.Message}");
                results.Add($"Stack trace: {ex.StackTrace}");
            }

            return View("FixResults", results);
        }

        public IActionResult FixResults()
        {
            return View();
        }
    }
}