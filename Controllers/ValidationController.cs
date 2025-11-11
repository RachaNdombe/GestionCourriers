using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using JconsultGC.Data;
using JconsultGC.Models;
using Microsoft.Extensions.Logging;

namespace JconsultGC.Controllers
{
    [Authorize]
    public class ValidationController : Controller
    {
        private readonly ProjetIdDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ValidationController> _logger;

        public ValidationController(
            ProjetIdDbContext context,
            UserManager<User> userManager,
            ILogger<ValidationController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Validation/Index - Liste des courriers à valider
        [Authorize(Roles = "Viseur")]
        public async Task<IActionResult> Index()
        {
            var courriers = await _context.Courriers!
                .Include(c => c.CategorieCourrier)
                .Include(c => c.NatureCourrier)
                .Include(c => c.ModeEnvoi)
                .Include(c => c.Correspondant)
                .Include(c => c.Service)
                .Include(c => c.TypeDossier)
                .Include(c => c.DossierClassement)
                .Include(c => c.CreatedBy)
                .Where(c => c.Statut == "A valider")
                .OrderByDescending(c => c.DateEnregistrement)
                .ToListAsync();

            var viewModel = new ValidationIndexViewModel
            {
                Courriers = courriers,
                AvailableServices = await _context.Services!
                    .Where(s => s.Actif)
                    .Select(s => new ServiceOption { Id = s.Id, Nom = s.Nom })
                    .ToListAsync() ?? new List<ServiceOption>()
            };

            return View(viewModel);
        }

        // GET: Validation/Details/5 - Détails d'un courrier à valider
        [Authorize(Roles = "Viseur")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courrier = await _context.Courriers!
                .Include(c => c.CategorieCourrier)
                .Include(c => c.NatureCourrier)
                .Include(c => c.ModeEnvoi)
                .Include(c => c.Correspondant)
                .Include(c => c.Service)
                .Include(c => c.TypeDossier)
                .Include(c => c.DossierClassement)
                .Include(c => c.CreatedBy)
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (courrier == null)
            {
                return NotFound();
            }

            var viewModel = new ValidationDetailsViewModel
            {
                Courrier = courrier,
                AvailableServices = await _context.Services!
                    .Where(s => s.Actif)
                    .Select(s => new ServiceOption { Id = s.Id, Nom = s.Nom })
                    .ToListAsync() ?? new List<ServiceOption>()
            };

            return View(viewModel);
        }

        // POST: Validation/AssignService/5 - Affecter un service à un courrier
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Viseur")]
        public async Task<IActionResult> AssignService(int id, int serviceId)
        {
            var courrier = await _context.Courriers!.FindAsync(id);
            if (courrier == null)
            {
                return NotFound();
            }

            // Vérifier que le service existe et est actif
            var service = await _context.Services!
                .FirstOrDefaultAsync(s => s.Id == serviceId && s.Actif);
            if (service == null)
            {
                TempData["ErrorMessage"] = "Service invalide ou inactif.";
                return RedirectToAction("Details", new { id });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            try
            {
                // Mettre à jour le service destinataire
                courrier.ServiceId = serviceId;
                courrier.Statut = "Assigné";

                _context.Courriers!.Update(courrier);

                // Ajouter l'historique
                var hist = new CourrierHistory
                {
                    CourrierId = courrier.Id,
                    UserId = user.Id,
                    Action = "Assigné",
                    Note = $"Courrier validé et affecté au service: {service.Nom}",
                    Timestamp = DateTime.UtcNow
                };
                _context.CourrierHistories!.Add(hist);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Courrier validé et assigné au service {service.Nom} avec succès !";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'affectation du service au courrier");
                TempData["ErrorMessage"] = "Erreur lors de l'affectation du service. Veuillez réessayer.";
                return RedirectToAction("Details", new { id });
            }
        }

        // POST: Validation/Reject/5 - Rejeter un courrier
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Viseur")]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            var courrier = await _context.Courriers!.FindAsync(id);
            if (courrier == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            try
            {
                // Revenir au statut "A saisir" pour que l'indexateur puisse corriger
                courrier.Statut = "A saisir";

                _context.Courriers!.Update(courrier);

                // Ajouter l'historique
                var hist = new CourrierHistory
                {
                    CourrierId = courrier.Id,
                    UserId = user.Id,
                    Action = "Rejeté",
                    Note = $"Courrier rejeté par le viseur. Raison: {reason ?? "Non spécifiée"}",
                    Timestamp = DateTime.UtcNow
                };
                _context.CourrierHistories!.Add(hist);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Courrier rejeté avec succès. L'indexateur pourra le corriger.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du rejet du courrier");
                TempData["ErrorMessage"] = "Erreur lors du rejet du courrier. Veuillez réessayer.";
                return RedirectToAction("Details", new { id });
            }
        }
    }

    public class ValidationIndexViewModel
    {
        public List<Courrier> Courriers { get; set; } = new List<Courrier>();
        public List<ServiceOption> AvailableServices { get; set; } = new List<ServiceOption>();
    }

    public class ValidationDetailsViewModel
    {
        public Courrier Courrier { get; set; } = new Courrier();
        public List<ServiceOption> AvailableServices { get; set; } = new List<ServiceOption>();
    }
}