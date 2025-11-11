using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using JconsultGC.Data;
using JconsultGC.Models;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Controllers
{
    [Authorize]
    public class IndexateurController : Controller
    {
        private readonly ProjetIdDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<IndexateurController> _logger;

        public IndexateurController(
            ProjetIdDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<IndexateurController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // GET: Indexateur/Dashboard
        [Authorize(Roles = "Indexateur")]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var today = DateTime.Today;
            
            var viewModel = new IndexateurDashboardViewModel
            {
                TotalCourriersIndexes = await _context.Courriers!
                    .Where(c => c.CreatedById == user.Id)
                    .CountAsync(),
                CourriersTresPrioritaires = await _context.Courriers!
                    .Where(c => c.CreatedById == user.Id && c.Priorite == (PrioriteLevel)3) // Urgent
                    .CountAsync(),
                CourriersEnAttente = await _context.Courriers!
                    .Where(c => c.CreatedById == user.Id && c.Statut == "A valider")
                    .CountAsync(),
                ActionsAujourdhui = await _context.CourrierHistories!
                    .Where(h => h.UserId == user.Id && h.Timestamp.Date == today)
                    .CountAsync(),
                CourriersASaisir = await _context.Courriers!
                    .Where(c => c.CreatedById == user.Id && c.Statut == "A saisir")
                    .CountAsync(),
                CourriersSignes = await _context.Courriers!
                    .Where(c => c.Statut == "Signé")
                    .CountAsync(),
                RecentActions = await _context.CourrierHistories!
                    .Where(h => h.UserId == user.Id && h.Timestamp.Date == today)
                    .OrderByDescending(h => h.Timestamp)
                    .Take(10)
                    .ToListAsync(),
                CourriersAValider = await _context.Courriers!
                    .Where(c => c.CreatedById == user.Id && c.Statut == "A valider")
                    .OrderByDescending(c => c.DateEnregistrement)
                    .Take(10)
                    .Include(c => c.CategorieCourrier)
                    .ToListAsync(),
                CourriersSignesList = await _context.Courriers!
                    .Where(c => c.Statut == "Signé")
                    .OrderByDescending(c => c.DateEnregistrement)
                    .Take(10)
                    .Include(c => c.CategorieCourrier)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // GET: Indexateur/Create
        [Authorize(Roles = "Indexateur")]
        public async Task<IActionResult> Create()
        {
            // Générer automatiquement le numéro d'ordre
            var numeroOrdre = await GenererNumeroOrdreAsync();
            
            var viewModel = new CourrierCreateViewModel
            {
                OrdreNumero = numeroOrdre,
                DateReception = DateTime.Now,
                AvailableServices = await _context.Services!.Select(s => new ServiceOption
                {
                    Id = s.Id,
                    Nom = s.Nom ?? "Nom non défini"
                }).ToListAsync() ?? new List<ServiceOption>(),
                AvailableCategories = await _context.CategorieCourriers!.Select(c => new JconsultGC.Models.LookupOption
                {
                    Id = c.Id,
                    Nom = c.Nom
                }).ToListAsync(),
                AvailableNatures = await _context.NatureCourriers!.Select(n => new JconsultGC.Models.LookupOption
                {
                    Id = n.Id,
                    Nom = n.Nom
                }).ToListAsync(),
                AvailableModesEnvoi = await _context.ModeEnvois!.Select(m => new JconsultGC.Models.LookupOption
                {
                    Id = m.Id,
                    Nom = m.Libelle
                }).ToListAsync(),
                AvailableCorrespondants = await _context.Correspondants!.Select(c => new JconsultGC.Models.LookupOption
                {
                    Id = c.Id,
                    Nom = c.Nom
                }).ToListAsync(),
                AvailableDossiers = await _context.DossierClassements!.Select(d => new JconsultGC.Models.LookupOption
                {
                    Id = d.Id,
                    Nom = d.Titre
                }).ToListAsync(),
                AvailableTypeDossiers = await _context.TypeDossiers!.Select(t => new JconsultGC.Models.LookupOption
                {
                    Id = t.Id,
                    Nom = t.Libelle ?? "Sans libellé"
                }).ToListAsync(),

            };
           

            return View(viewModel);
        }

        // POST: Indexateur/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Indexateur")]
        public async Task<IActionResult> Create(CourrierCreateViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                var courrier = new Courrier
                {
                    OrdreNumero = model.OrdreNumero,
                    RegistreNumero = model.RegistreNumero ?? "",
                    ReferenceNumero = model.ReferenceNumero,
                    Objet = model.Objet,
                    DateReception = model.DateReception,
                    HeureRecu = model.HeureRecu,
                    ServiceConcerne = model.ServiceConcerne,
                    UtilisateursEnCopie = model.UtilisateursEnCopie,
                    CorrespondantId = model.CorrespondantId,
                    ServiceId = model.ServiceId,
                    CategorieCourrierId = model.CategorieCourrierId,
                    ModeEnvoiId = model.ModeEnvoiId,
                    NatureCourrierId = model.NatureCourrierId,
                    TypeDossierId = model.TypeDossierId,
                    DossierClassementId = model.DossierClassementId,
                    Confidentialite = model.Confidentialite,
                    Priorite = model.Priorite,
                    DateEnregistrement = DateTime.UtcNow,
                    Statut = "A valider",
                    CreatedById = user.Id
                };

                _context.Courriers!.Add(courrier);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Courrier entrant créé avec succès !";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la sauvegarde du courrier entrant");
                ModelState.AddModelError("", "Une erreur s'est produite lors de la sauvegarde. Veuillez réessayer.");
                await RechargerDonneesFormulaire(model);
                return View(model);
            }
        }

        private async Task<string> GenererNumeroOrdreAsync()
        {
            var lastCourrier = await _context.Courriers!
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();
            
            var nextOrdreNumero = 1;
            if (lastCourrier != null && !string.IsNullOrEmpty(lastCourrier.OrdreNumero))
            {
                if (int.TryParse(lastCourrier.OrdreNumero, out int lastNumero))
                {
                    nextOrdreNumero = lastNumero + 1;
                }
            }

            return nextOrdreNumero.ToString("D6"); // Format 000001, 000002, etc.
        }

        private async Task<int> GetValidServiceIdAsync(int? serviceConcerneId)
        {
            // Vérifier que ServiceConcerneId est valide
            if (serviceConcerneId.HasValue && serviceConcerneId.Value > 0)
            {
                // Vérifier que le service existe
                var serviceExists = await _context.Services!.AnyAsync(s => s.Id == serviceConcerneId.Value && s.Actif);
                if (serviceExists)
                {
                    return serviceConcerneId.Value;
                }
            }
            
            // Utiliser le premier service actif comme valeur par défaut
            var defaultService = await _context.Services!.Where(s => s.Actif).FirstOrDefaultAsync();
            if (defaultService != null)
            {
                return defaultService.Id;
            }
            
            // Si aucun service n'est disponible, lancer une exception
            throw new InvalidOperationException("Aucun service actif n'est disponible. Veuillez créer au moins un service.");
        }

        private async Task RechargerDonneesFormulaire(CourrierCreateViewModel model)
        {
            model.AvailableServices = await _context.Services!.Select(s => new ServiceOption
            {
                Id = s.Id,
                Nom = s.Nom
            }).ToListAsync() ?? new List<ServiceOption>();
            model.AvailableCategories = await _context.CategorieCourriers!.Select(c => new JconsultGC.Models.LookupOption
            {
                Id = c.Id,
                Nom = c.Nom
            }).ToListAsync();
            model.AvailableNatures = await _context.NatureCourriers!.Select(n => new JconsultGC.Models.LookupOption
            {
                Id = n.Id,
                Nom = n.Nom
            }).ToListAsync();
            model.AvailableModesEnvoi = await _context.ModeEnvois!.Select(m => new JconsultGC.Models.LookupOption
            {
                Id = m.Id,
                Nom = m.Libelle
            }).ToListAsync();
            model.AvailableCorrespondants = await _context.Correspondants!.Select(c => new JconsultGC.Models.LookupOption
            {
                Id = c.Id,
                Nom = c.Nom
            }).ToListAsync();
            model.AvailableDossiers = await _context.DossierClassements!.Select(d => new JconsultGC.Models.LookupOption
            {
                Id = d.Id,
                Nom = d.Titre
            }).ToListAsync();
            model.AvailableTypeDossiers = await _context.TypeDossiers!.Select(t => new JconsultGC.Models.LookupOption
            {
                Id = t.Id,
                Nom = t.Libelle ?? "Sans libellé"
            }).ToListAsync();
        }
    }
}

