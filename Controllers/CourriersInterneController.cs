using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JconsultGC.Models;
using JconsultGC.Data;
using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Controllers
{
    [Authorize]
    public class CourriersInterneController : Controller
    {
        private readonly ProjetIdDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<CourriersInterneController> _logger;

        public CourriersInterneController(
            ProjetIdDbContext context,
            UserManager<User> userManager,
            ILogger<CourriersInterneController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: CourriersInterne/Create
        [Authorize(Policy = "CanCreateCourrier")]
        public async Task<IActionResult> Create()
        {
            var viewModel = new CourrierCreateViewModel
            {
                DateReception = DateTime.Now
            };

            await LoadCreateViewModelData(viewModel);
            return View(viewModel);
        }

        // POST: CourriersInterne/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Viseur")]
        public async Task<IActionResult> Create(CourrierCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                var courrier = new Courrier
                {
                    Objet = model.Objet,
                    DateReception = model.DateReception,
                    HeureRecu = model.HeureRecu,
                    ServiceConcerne = model.ServiceConcerne,
                    CorrespondantId = model.CorrespondantId,
                    DossierClassementId = model.DossierClassementId,
                    CategorieCourrierId = model.CategorieCourrierId,
                    NatureCourrierId = model.NatureCourrierId,
                    ModeEnvoiId = model.ModeEnvoiId,
                    CreatedById = user.Id
                };

                _context.Courriers.Add(courrier);
                await _context.SaveChangesAsync();

                // Enregistrer l'action dans l'historique
                var historiqueAction = new CourrierHistory
                {
                    CourrierId = courrier.Id,
                    Action = "CREATE_INTERNE",
                    Note = "Courrier interne créé",
                    UserId = user.Id
                };
                _context.CourrierHistories.Add(historiqueAction);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Courrier interne créé avec succès !";
                return RedirectToAction("Index", "Courriers");
            }

            // Recharger les données pour la réaffichage du formulaire
            await LoadCreateViewModelData(model);
            return View(model);
        }

        // GET: CourriersInterne/Index
        public async Task<IActionResult> Index()
        {
            var courriers = await _context.Courriers
                .Include(c => c.CreatedBy)
                .Include(c => c.ServiceConcerne)
                .Include(c => c.Correspondant)
                .Where(c => c.NatureCourrier != null && c.NatureCourrier.Nom.Contains("Interne"))
                .OrderByDescending(c => c.DateReception)
                .ToListAsync();

            return View(courriers);
        }

        // GET: CourriersInterne/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courrier = await _context.Courriers
                .Include(c => c.CreatedBy)
                .Include(c => c.ServiceConcerne)
                .Include(c => c.Correspondant)
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (courrier == null)
            {
                return NotFound();
            }

            return View(courrier);
        }

        private async Task LoadCreateViewModelData(CourrierCreateViewModel viewModel)
        {
            // Charger les services
            var availableServices = await _context.Services
                .Where(s => s.Actif)
                .Select(s => new { Id = s.Id, Nom = s.Nom })
                .ToListAsync();
            ViewBag.AvailableServices = availableServices;

            // Charger les catégories
            var availableCategories = await _context.CategorieCourriers
                .Select(c => new { Id = c.Id, Libelle = c.Nom })
                .ToListAsync();
            ViewBag.AvailableCategories = availableCategories;

            // Charger les natures
            var availableNatures = await _context.NatureCourriers
                .Select(n => new { Id = n.Id, Libelle = n.Nom })
                .ToListAsync();
            ViewBag.AvailableNatures = availableNatures;

            // Charger les modes d'envoi
            var availableModesEnvoi = await _context.ModeEnvois
                .Select(m => new { Id = m.Id, Libelle = m.Libelle })
                .ToListAsync();
            ViewBag.AvailableModesEnvoi = availableModesEnvoi;

            // Charger les correspondants
            var availableCorrespondants = await _context.Correspondants
                .Select(c => new { Id = c.Id, Nom = c.Nom })
                .ToListAsync();
            ViewBag.AvailableCorrespondants = availableCorrespondants;

            // Charger les dossiers de classement
            var availableDossiers = await _context.DossierClassements
                .Select(d => new { Id = d.Id, Libelle = d.Titre })
                .ToListAsync();
            ViewBag.AvailableDossiers = availableDossiers;
        }
    }
}