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
    public class CourriersArchiveController : Controller
    {
        private readonly ProjetIdDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<CourriersArchiveController> _logger;

        public CourriersArchiveController(
            ProjetIdDbContext context,
            UserManager<User> userManager,
            ILogger<CourriersArchiveController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: CourriersArchive/Index
        public async Task<IActionResult> Index()
        {
            var courriers = await _context.Courriers
                .Include(c => c.CreatedBy)
                .Include(c => c.Correspondant)
                .Include(c => c.CategorieCourrier)
                .Include(c => c.NatureCourrier)
                .Where(c => c.IsArchive)
                .OrderByDescending(c => c.DateReception)
                .ToListAsync();

            return View(courriers);
        }

        // GET: CourriersArchive/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courrier = await _context.Courriers
                .Include(c => c.CreatedBy)
                .Include(c => c.Correspondant)
                .Include(c => c.Documents)
                .Include(c => c.CategorieCourrier)
                .Include(c => c.NatureCourrier)
                .Include(c => c.DossierClassement)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsArchive);

            if (courrier == null)
            {
                return NotFound();
            }

            return View(courrier);
        }

        // POST: CourriersArchive/Archiver/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Archiviste")]
        public async Task<IActionResult> Archiver(int id)
        {
            var courrier = await _context.Courriers.FindAsync(id);
            if (courrier == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            courrier.IsArchive = true;

            // Enregistrer l'action dans l'historique
            var historiqueAction = new CourrierHistory
            {
                CourrierId = courrier.Id,
                Action = "ARCHIVE",
                Note = "Courrier archivé",
                UserId = user.Id
            };
            _context.CourrierHistories.Add(historiqueAction);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Courrier archivé avec succès !";
            return RedirectToAction(nameof(Index));
        }

        // POST: CourriersArchive/Restaurer/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Archiviste")]
        public async Task<IActionResult> Restaurer(int id)
        {
            var courrier = await _context.Courriers.FindAsync(id);
            if (courrier == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            courrier.IsArchive = false;

            // Enregistrer l'action dans l'historique
            var historiqueAction = new CourrierHistory
            {
                CourrierId = courrier.Id,
                Action = "RESTORE",
                Note = "Courrier restauré depuis l'archive",
                UserId = user.Id
            };
            _context.CourrierHistories.Add(historiqueAction);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Courrier restauré avec succès !";
            return RedirectToAction(nameof(Index));
        }

        // GET: CourriersArchive/Recherche
        public async Task<IActionResult> Recherche(string? searchTerm, DateTime? dateDebut, DateTime? dateFin)
        {
            var query = _context.Courriers
                .Include(c => c.CreatedBy)
                .Include(c => c.Correspondant)
                .Include(c => c.CategorieCourrier)
                .Include(c => c.NatureCourrier)
                .Where(c => c.IsArchive);

            // Filtre par terme de recherche
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => 
                    c.Objet.Contains(searchTerm) ||
                    c.ReferenceNumero.Contains(searchTerm) ||
                    (c.Correspondant != null && c.Correspondant.Nom.Contains(searchTerm)) ||
                    (c.ServiceConcerne != null && c.ServiceConcerne.Contains(searchTerm))
                );
            }

            // Filtre par date
            if (dateDebut.HasValue)
            {
                query = query.Where(c => c.DateReception >= dateDebut.Value);
            }

            if (dateFin.HasValue)
            {
                query = query.Where(c => c.DateReception <= dateFin.Value);
            }

            var courriers = await query
                .OrderByDescending(c => c.DateReception)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.DateDebut = dateDebut;
            ViewBag.DateFin = dateFin;

            return View("Index", courriers);
        }

        // GET: CourriersArchive/Stats
        [Authorize(Roles = "Archiviste,Admin")]
        public async Task<IActionResult> Stats()
        {
            var stats = new
            {
                TotalArchives = await _context.Courriers.CountAsync(c => c.IsArchive),
                ArchivesParMois = await _context.Courriers
                    .Where(c => c.IsArchive)
                    .GroupBy(c => new { c.DateReception.Value.Year, c.DateReception.Value.Month })
                    .Select(g => new { 
                        Annee = g.Key.Year, 
                        Mois = g.Key.Month, 
                        Count = g.Count() 
                    })
                    .OrderByDescending(x => x.Annee)
                    .ThenByDescending(x => x.Mois)
                    .Take(12)
                    .ToListAsync(),
                ArchivesParCategorie = await _context.Courriers
                    .Where(c => c.IsArchive && c.CategorieCourrier != null)
                    .GroupBy(c => c.CategorieCourrier.Nom)
                    .Select(g => new { 
                        Categorie = g.Key, 
                        Count = g.Count() 
                    })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync()
            };

            return View(stats);
        }

        // GET: CourriersArchive/Dashboard - Vue pour l'archiviste
        [Authorize(Roles = "Archiviste")]
        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.Today;
            
            // Récupérer les courriers signés du jour
            var courriersSignesAujourdhui = await _context.Courriers
                .Include(c => c.CreatedBy)
                .Include(c => c.Correspondant)
                .Include(c => c.CategorieCourrier)
                .Include(c => c.NatureCourrier)
                .Where(c => c.Statut == "Signé" && !c.IsArchive &&
                    _context.CourrierHistories.Any(h =>
                        h.CourrierId == c.Id &&
                        h.Action == "Signé" &&
                        h.Timestamp.Date == today))
                .OrderByDescending(c => c.DateEnregistrement)
                .ToListAsync();

            // Statistiques pour le dashboard
            var stats = new
            {
                TotalCourriersSignesAujourdhui = courriersSignesAujourdhui.Count,
                TotalCourriersArchives = await _context.Courriers.CountAsync(c => c.IsArchive),
                CourriersEnAttenteArchivage = await _context.Courriers
                    .CountAsync(c => c.Statut == "Signé" && !c.IsArchive &&
                        _context.CourrierHistories.Any(h =>
                            h.CourrierId == c.Id &&
                            h.Action == "Signé" &&
                            h.Timestamp.Date == today)),
                CourriersArchivesAujourdhui = await _context.Courriers
                    .CountAsync(c => c.IsArchive && c.DateEnregistrement.Date == today)
            };

            var viewModel = new
            {
                CourriersSignes = courriersSignesAujourdhui,
                Statistiques = stats
            };

            return View(viewModel);
        }

        // POST: CourriersArchive/ArchiverMultiple - Archivage multiple des courriers signés
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Archiviste")]
        public async Task<IActionResult> ArchiverMultiple(List<int> courrierIds)
        {
            if (courrierIds == null || !courrierIds.Any())
            {
                TempData["ErrorMessage"] = "Aucun courrier sélectionné pour l'archivage.";
                return RedirectToAction(nameof(Dashboard));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var courriers = await _context.Courriers
                .Where(c => courrierIds.Contains(c.Id) && c.Statut == "Signé" && !c.IsArchive)
                .ToListAsync();

            if (!courriers.Any())
            {
                TempData["ErrorMessage"] = "Aucun courrier signé valide trouvé pour l'archivage.";
                return RedirectToAction(nameof(Dashboard));
            }

            foreach (var courrier in courriers)
            {
                courrier.IsArchive = true;

                // Enregistrer l'action dans l'historique
                var historiqueAction = new CourrierHistory
                {
                    CourrierId = courrier.Id,
                    Action = "ARCHIVE",
                    Note = "Courrier archivé par l'archiviste",
                    UserId = user.Id
                };
                _context.CourrierHistories.Add(historiqueAction);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{courriers.Count} courrier(s) archivé(s) avec succès !";
            return RedirectToAction(nameof(Dashboard));
        }

        // GET: CourriersArchive/CourriersSignes - Liste de tous les courriers signés non archivés
        [Authorize(Roles = "Archiviste")]
        public async Task<IActionResult> CourriersSignes()
        {
            var courriersSignes = await _context.Courriers
                .Include(c => c.CreatedBy)
                .Include(c => c.Correspondant)
                .Include(c => c.CategorieCourrier)
                .Include(c => c.NatureCourrier)
                .Where(c => c.Statut == "Signé" && !c.IsArchive)
                .OrderByDescending(c => c.DateEnregistrement)
                .ToListAsync();

            return View(courriersSignes);
        }

        // POST: CourriersArchive/ArchiverTousAujourdhui - Archivage de tous les courriers signés du jour
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Archiviste")]
        public async Task<IActionResult> ArchiverTousAujourdhui()
        {
            var today = DateTime.Today;
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var courriers = await _context.Courriers
                .Where(c => c.Statut == "Signé" && !c.IsArchive &&
                    _context.CourrierHistories.Any(h =>
                        h.CourrierId == c.Id &&
                        h.Action == "Signé" &&
                        h.Timestamp.Date == today))
                .ToListAsync();

            if (!courriers.Any())
            {
                TempData["ErrorMessage"] = "Aucun courrier signé disponible pour l'archivage aujourd'hui.";
                return RedirectToAction(nameof(Dashboard));
            }

            foreach (var courrier in courriers)
            {
                courrier.IsArchive = true;

                // Enregistrer l'action dans l'historique
                var historiqueAction = new CourrierHistory
                {
                    CourrierId = courrier.Id,
                    Action = "ARCHIVE",
                    Note = "Courrier archivé automatiquement par l'archiviste",
                    UserId = user.Id
                };
                _context.CourrierHistories.Add(historiqueAction);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Tous les courriers signés du jour ({courriers.Count}) ont été archivés avec succès !";
            return RedirectToAction(nameof(Dashboard));
        }
    }
}