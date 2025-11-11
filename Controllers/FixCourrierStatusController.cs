using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using JconsultGC.Data;
using JconsultGC.Models;
using Microsoft.Extensions.Logging;

namespace JconsultGC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FixCourrierStatusController : Controller
    {
        private readonly ProjetIdDbContext _context;
        private readonly ILogger<FixCourrierStatusController> _logger;

        public FixCourrierStatusController(
            ProjetIdDbContext context,
            ILogger<FixCourrierStatusController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: FixCourrierStatus
        public async Task<IActionResult> Index()
        {
            var viewModel = new FixCourrierStatusViewModel
            {
                // Statistiques actuelles
                CourriersAvecMauvaisStatut = await _context.Courriers!
                    .Where(c => c.ServiceId != null && c.Statut == "Validé")
                    .CountAsync(),
                
                CourriersAssignes = await _context.Courriers!
                    .Where(c => c.ServiceId != null && c.Statut == "Assigné")
                    .CountAsync(),
                
                CourriersEnTraitement = await _context.Courriers!
                    .Where(c => c.ServiceId != null && c.Statut == "En traitement")
                    .CountAsync(),
                
                CourriersTraites = await _context.Courriers!
                    .Where(c => c.ServiceId != null && c.Statut == "Traité")
                    .CountAsync(),
                
                // Détails par service
                CourriersParService = await _context.Courriers!
                    .Include(c => c.Service)
                    .Where(c => c.ServiceId != null)
                    .GroupBy(c => c.ServiceId)
                    .Select(g => new ServiceCourrierStats
                    {
                        ServiceId = g.Key ?? 0,
                        ServiceNom = g.First().Service != null ? g.First().Service.Nom : "Inconnu",
                        Total = g.Count(),
                        Assignes = g.Count(c => c.Statut == "Assigné"),
                        EnTraitement = g.Count(c => c.Statut == "En traitement"),
                        Traites = g.Count(c => c.Statut == "Traité"),
                        Valides = g.Count(c => c.Statut == "Validé")
                    })
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // POST: FixCourrierStatus/FixStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FixStatus()
        {
            try
            {
                // Trouver tous les courriers qui ont un ServiceId mais le statut "Validé"
                var courriersAvecMauvaisStatut = await _context.Courriers!
                    .Where(c => c.ServiceId != null && c.Statut == "Validé")
                    .ToListAsync();
                
                _logger.LogInformation($"Correction de {courriersAvecMauvaisStatut.Count} courriers avec statut 'Validé'");

                foreach (var courrier in courriersAvecMauvaisStatut)
                {
                    courrier.Statut = "Assigné";
                    _logger.LogInformation($"Courrier ID {courrier.Id} mis à jour: ServiceId={courrier.ServiceId}, Statut=Assigné");
                }

                if (courriersAvecMauvaisStatut.Any())
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Correction terminée: {courriersAvecMauvaisStatut.Count} courriers mis à jour avec le statut 'Assigné'";
                }
                else
                {
                    TempData["InfoMessage"] = "Aucun courrier à corriger. Tous les courriers ont déjà le bon statut.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la correction des statuts de courriers");
                TempData["ErrorMessage"] = $"Erreur lors de la correction: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }

    public class FixCourrierStatusViewModel
    {
        public int CourriersAvecMauvaisStatut { get; set; }
        public int CourriersAssignes { get; set; }
        public int CourriersEnTraitement { get; set; }
        public int CourriersTraites { get; set; }
        public List<ServiceCourrierStats> CourriersParService { get; set; } = new List<ServiceCourrierStats>();
    }

    public class ServiceCourrierStats
    {
        public int ServiceId { get; set; }
        public string ServiceNom { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Assignes { get; set; }
        public int EnTraitement { get; set; }
        public int Traites { get; set; }
        public int Valides { get; set; }
    }
}