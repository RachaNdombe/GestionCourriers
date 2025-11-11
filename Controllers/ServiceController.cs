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
    public class ServiceController : Controller
    {
        private readonly ProjetIdDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ServiceController> _logger;

        public ServiceController(
            ProjetIdDbContext context,
            UserManager<User> userManager,
            ILogger<ServiceController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Service/ - Page d'accueil du service (redirige vers le tableau de bord)
        [Authorize(Roles = "ServiceExpéditeur")]
        
        //public async Task<IActionResult> Dashboard()
        //{
            //return RedirectToAction(nameof(Dashboard));
        //}

        // GET: Service/Dashboard - Tableau de bord du service
        [Authorize(Roles = "ServiceExpéditeur")]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var serviceId = user.ServiceId;
            if (serviceId == null)
            {
                return BadRequest("Utilisateur non associé à un service");
            }

            // Vérifier que l'utilisateur est bien associé à ce service
            var service = await _context.Services!.FirstOrDefaultAsync(s => s.Id == serviceId);
            if (service == null)
            {
                return BadRequest("Service non trouvé");
            }

            var viewModel = new ServiceDashboardViewModel
            {
                // Courriers assignés au service
                CourriersAssignesCount = await _context.Courriers!
                    .Where(c => c.ServiceId == serviceId && c.Statut == "Assigné")
                    .CountAsync(),

                // Courriers en cours de traitement
                CourriersEnTraitementCount = await _context.Courriers!
                    .Where(c => c.ServiceId == serviceId && c.Statut == "En traitement")
                    .CountAsync(),

                // Courriers traités aujourd'hui
                CourriersTraitesAujourdhui = await _context.CourrierHistories!
                    .Where(h => h.Action == "Traité" && 
                                h.Timestamp.Date == DateTime.Today)
                    .CountAsync(),

                // Courriers signés aujourd'hui
                CourriersSignesAujourdhui = await _context.CourrierHistories!
                    .Where(h => h.Action == "Signé" && 
                                h.Timestamp.Date == DateTime.Today)
                    .CountAsync(),

                // Courriers assignés récents
                CourriersAssignes = await _context.Courriers!
                    .Include(c => c.CategorieCourrier)
                    .Include(c => c.Correspondant)
                    .Include(c => c.CreatedBy)
                    .Where(c => c.ServiceId == serviceId && (c.Statut == "Assigné" || c.Statut == "En traitement" || c.Statut == "Traité"))
                    .OrderByDescending(c => c.DateEnregistrement)
                    .Take(5)
                    .ToListAsync(),

                // Actions récentes du service
                RecentActions = await _context.CourrierHistories!
                    .Where(h => h.UserId == user.Id && 
                                h.Timestamp.Date == DateTime.Today)
                    .OrderByDescending(h => h.Timestamp)
                    .Take(10)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // GET: Service/Courriers - Liste des courriers assignés au service
        [Authorize(Roles = "ServiceExpéditeur")]
        public async Task<IActionResult> Courriers()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.ServiceId == null)
            {
                return Unauthorized();
            }

            var courriers = await _context.Courriers!
                .Include(c => c.CategorieCourrier)
                .Include(c => c.Correspondant)
                .Include(c => c.CreatedBy)
                .Include(c => c.Service)
                .Where(c => c.ServiceId == user.ServiceId &&
                           (c.Statut == "Assigné" || c.Statut == "En traitement" || c.Statut == "Traité"))
                .OrderByDescending(c => c.DateEnregistrement)
                .ToListAsync();

            return View(courriers);
        }

        // GET: Service/Details/5 - Détails d'un courrier
        [Authorize(Roles = "ServiceExpéditeur")]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.ServiceId == null)
            {
                return Unauthorized();
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
                .FirstOrDefaultAsync(c => c.Id == id && c.ServiceId == user.ServiceId);

            if (courrier == null)
            {
                return NotFound();
            }

            var services = await _context.Services!
                .Where(s => s.Actif)
                .ToListAsync();

            ViewBag.Services = services;
            return View(courrier);
        }

        // POST: Service/CommencerTraitement/5 - Commencer le traitement d'un courrier
        [HttpPost]
        [Authorize(Roles = "ServiceExpéditeur")]
        public async Task<IActionResult> CommencerTraitement(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.ServiceId == null)
            {
                return Unauthorized();
            }

            var courrier = await _context.Courriers!
                .FirstOrDefaultAsync(c => c.Id == id && c.ServiceId == user.ServiceId);

            if (courrier == null)
            {
                return NotFound();
            }

            courrier.Statut = "En traitement";
            // On utilise CreatedById pour stocker l'utilisateur qui traite le courrier
            courrier.CreatedById = user.Id;

            // Historique
            var history = new CourrierHistory
            {
                CourrierId = courrier.Id,
                UserId = user.Id,
                Action = "Traitement commencé",
                Note = $"Le traitement a été commencé par {user.Nom} {user.Prenom}",
                Timestamp = DateTime.UtcNow
            };

            _context.CourrierHistories!.Add(history);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Traitement du courrier commencé avec succès";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Service/TerminerTraitement/5 - Terminer le traitement d'un courrier
        [HttpPost]
        [Authorize(Roles = "ServiceExpéditeur")]
        public async Task<IActionResult> TerminerTraitement(int id, string noteTraitement)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.ServiceId == null)
            {
                return Unauthorized();
            }

            var courrier = await _context.Courriers!
                .FirstOrDefaultAsync(c => c.Id == id && c.ServiceId == user.ServiceId);

            if (courrier == null)
            {
                return NotFound();
            }

            courrier.Statut = "Traité";

            // Historique
            var history = new CourrierHistory
            {
                CourrierId = courrier.Id,
                UserId = user.Id,
                Action = "Traité",
                Note = $"Traitement terminé par {user.Nom} {user.Prenom}. Note: {noteTraitement}",
                Timestamp = DateTime.UtcNow
            };

            _context.CourrierHistories!.Add(history);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Traitement du courrier terminé avec succès";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Service/Signer/5 - Page de signature
        [Authorize(Roles = "ServiceExpéditeur")]
        public async Task<IActionResult> Signer(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.ServiceId == null)
            {
                return Unauthorized();
            }

            var courrier = await _context.Courriers!
                .Include(c => c.CategorieCourrier)
                .Include(c => c.Correspondant)
                .Include(c => c.Service)
                .FirstOrDefaultAsync(c => c.Id == id && c.ServiceId == user.ServiceId);

            if (courrier == null)
            {
                return NotFound();
            }

            if (courrier.Statut != "Traité")
            {
                TempData["ErrorMessage"] = "Le courrier doit être traité avant d'être signé";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(courrier);
        }

        // POST: Service/Signer/5 - Effectuer la signature
        [HttpPost]
        [Authorize(Roles = "ServiceExpéditeur")]
        public async Task<IActionResult> Signer(int id, string signatureNote)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.ServiceId == null)
            {
                return Unauthorized();
            }

            var courrier = await _context.Courriers!
                .FirstOrDefaultAsync(c => c.Id == id && c.ServiceId == user.ServiceId);

            if (courrier == null)
            {
                return NotFound();
            }

            if (courrier.Statut != "Traité")
            {
                TempData["ErrorMessage"] = "Le courrier doit être traité avant d'être signé";
                return RedirectToAction(nameof(Details), new { id });
            }

            courrier.Statut = "Signé";

            // Historique de signature
            var history = new CourrierHistory
            {
                CourrierId = courrier.Id,
                UserId = user.Id,
                Action = "Signé",
                Note = $"Courrier signé par {user.Nom} {user.Prenom}. Note: {signatureNote}",
                Timestamp = DateTime.UtcNow
            };

            _context.CourrierHistories!.Add(history);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Courrier signé avec succès";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    public class ServiceDashboardViewModel
    {
        public int CourriersAssignesCount { get; set; }
        public int CourriersEnTraitementCount { get; set; }
        public int CourriersTraitesAujourdhui { get; set; }
        public int CourriersSignesAujourdhui { get; set; }
        public List<Courrier> CourriersAssignes { get; set; } = new List<Courrier>();
        public List<CourrierHistory> RecentActions { get; set; } = new List<CourrierHistory>();
    }
}
