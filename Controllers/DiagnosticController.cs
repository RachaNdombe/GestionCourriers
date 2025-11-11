using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JconsultGC.Models;
using JconsultGC.Data;
using Microsoft.AspNetCore.Authorization;

namespace JconsultGC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DiagnosticController : Controller
    {
        private readonly ProjetIdDbContext _context;
        private readonly UserManager<User> _userManager;

        public DiagnosticController(ProjetIdDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> FixServiceAssociation()
        {
            var results = new List<string>();

            try
            {
                // 1. Vérifier les services existants
                var services = await _context.Services.ToListAsync();
                results.Add($"Services trouvés: {services.Count}");
                
                foreach (var service in services)
                {
                    results.Add($"Service: {service.Id} - {service.Nom}");
                }

                // 2. Vérifier les utilisateurs avec rôle ServiceExpéditeur
                var serviceUsers = await _userManager.GetUsersInRoleAsync("ServiceExpéditeur");
                results.Add($"Utilisateurs ServiceExpéditeur trouvés: {serviceUsers.Count}");
                
                foreach (var user in serviceUsers)
                {
                    results.Add($"Utilisateur: {user.Id} - {user.Email} - ServiceId: {user.ServiceId}");
                }

                // 3. Créer un service par défaut s'il n'existe pas
                if (!services.Any())
                {
                    var defaultService = new Service
                    {
                        Nom = "Service par défaut",
                        Description = "Service créé automatiquement pour les utilisateurs ServiceExpéditeur"
                    };
                    
                    _context.Services.Add(defaultService);
                    await _context.SaveChangesAsync();
                    results.Add("Service par défaut créé avec ID: " + defaultService.Id);
                    
                    services = await _context.Services.ToListAsync();
                }

                // 4. Associer les utilisateurs ServiceExpéditeur au premier service disponible
                var firstService = services.First();
                int fixedCount = 0;
                
                foreach (var user in serviceUsers)
                {
                    if (user.ServiceId == null)
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
                    else
                    {
                        results.Add($"✓ Utilisateur {user.Email} a déjà ServiceId: {user.ServiceId}");
                    }
                }

                results.Add($"Total des associations corrigées: {fixedCount}");

                // 5. Vérifier l'état final
                var finalServiceUsers = await _userManager.GetUsersInRoleAsync("ServiceExpéditeur");
                results.Add("=== ÉTAT FINAL ===");
                foreach (var user in finalServiceUsers)
                {
                    var serviceName = user.ServiceId.HasValue 
                        ? services.FirstOrDefault(s => s.Id == user.ServiceId.Value)?.Nom ?? "Service inconnu"
                        : "AUCUN SERVICE";
                    
                    results.Add($"Utilisateur: {user.Email} - Service: {serviceName}");
                }

            }
            catch (Exception ex)
            {
                results.Add($"ERREUR: {ex.Message}");
                results.Add($"Stack trace: {ex.StackTrace}");
            }

            return View("DiagnosticResults", results);
        }

        public IActionResult DiagnosticResults()
        {
            return View();
        }
    }
}