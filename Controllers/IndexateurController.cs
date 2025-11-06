using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JconsultGC.Data;
using JconsultGC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JconsultGC.Controllers
{
    [Authorize(Roles = "Indexateur")]
    public class IndexateurController : Controller
    {
        private readonly ProjetIdDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<User> _userManager;

        public IndexateurController(ProjetIdDbContext context, IWebHostEnvironment env, UserManager<User> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }

        // Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);
            var model = new IndexateurDashboardViewModel();

            // Get recent actions by this indexateur
            model.RecentActions = await _context.CourrierHistories!
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.Timestamp)
                .Take(10)
                .ToListAsync();

            // Get courriers created by this indexateur
            var courriers = await _context.Courriers
                .Include(c => c.CategorieCourrier)
                .Where(c => c.CreatedById == userId)
                .ToListAsync();

            model.RecentCourriers = courriers
                .OrderByDescending(c => c.DateEnregistrement)
                .Take(5);

            // Get courriers waiting for validation
            model.CourriersAValider = await _context.Courriers
                .Include(c => c.CategorieCourrier)
                .Where(c => c.Statut == "A valider" && c.CreatedById == userId)
                .OrderByDescending(c => c.DateEnregistrement)
                .Take(5)
                .ToListAsync();

            // Calculate statistics
            model.TotalCourriersIndexes = courriers.Count;
            model.CourriersTresPrioritaires = courriers.Count(c => c.Priorite == PrioriteLevel.Urgent);
            model.CourriersEnAttente = courriers.Count(c => c.Statut == "A valider");

            // Distribution by category
            model.CourriersByCategory = courriers
                .Where(c => c.CategorieCourrier != null)
                .GroupBy(c => c.CategorieCourrier!.Nom!)
                .ToDictionary(g => g.Key, g => g.Count());

            // Distribution by status
            model.CourriersByStatus = courriers
                .Where(c => c.Statut != null)
                .GroupBy(c => c.Statut!)
                .ToDictionary(g => g.Key, g => g.Count());

            return View(model);
        }

        public async Task<IActionResult> Index()
        {
            var list = await _context.Courriers
                .Include(c => c.CategorieCourrier)
                .Include(c => c.ModeEnvoi)
                .OrderByDescending(c => c.DateEnregistrement)
                .ToListAsync();
            return View(list);
        }

        // List courriers ready for validation
        public async Task<IActionResult> ToValidate()
        {
            var list = await _context.Courriers
                .Include(c => c.CategorieCourrier)
                .Include(c => c.ModeEnvoi)
                .Where(c => c.Statut == "A valider")
                .OrderByDescending(c => c.DateEnregistrement)
                .ToListAsync();
            return View(list);
        }

        // Details and history
        public async Task<IActionResult> Details(int id)
        {
            var courrier = await _context.Courriers
                .Include(c => c.Documents)
                .Include(c => c.CategorieCourrier)
                .Include(c => c.ModeEnvoi)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (courrier == null) return NotFound();

            var histories = await _context.CourrierHistories!
                .Where(h => h.CourrierId == id)
                .OrderByDescending(h => h.Timestamp)
                .ToListAsync();

            ViewBag.Histories = histories;
            return View(courrier);
        }

        [HttpPost]
        public async Task<IActionResult> MarkReady(int id)
        {
            var courrier = await _context.Courriers.FindAsync(id);
            if (courrier == null) return NotFound();

            courrier.Statut = "A valider";
            await _context.SaveChangesAsync();

            var userId = _userManager.GetUserId(User);
            var hist = new CourrierHistory
            {
                CourrierId = courrier.Id,
                UserId = userId,
                Action = "Envoyé à validation",
                Note = "Marqué prêt pour validation",
                Timestamp = DateTime.UtcNow
            };
            _context.CourrierHistories!.Add(hist);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.CategorieCourriers!.ToListAsync(), "Id", "Nom");
            ViewBag.Modes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.ModeEnvois!.ToListAsync(), "Id", "Nom");
            ViewBag.TypeDossiers = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.TypeDossiers!.ToListAsync(), "Id", "Libelle");
            ViewBag.Services = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Services!.ToListAsync(), "Id", "Nom");
            ViewBag.Correspondants = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Correspondants!.ToListAsync(), "Id", "Nom");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CourrierCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSelects();
                return View(model);
            }

            // Ensure the indexateur previewed files before saving
            if (model.Attachments != null && model.Attachments.Count > 0 && !model.VisualizedBeforeSave)
            {
                ModelState.AddModelError(string.Empty, "Vous devez visualiser les pièces jointes avant d'enregistrer le courrier.");
                await PopulateSelects();
                return View(model);
            }

            // If user provided a new category name, create it
            int? categorieId = model.CategorieCourrierId;
            if (!string.IsNullOrWhiteSpace(model.NewCategorie))
            {
                var cat = new CategorieCourrier { Nom = model.NewCategorie };
                _context.CategorieCourriers!.Add(cat);
                await _context.SaveChangesAsync();
                categorieId = cat.Id;
            }

            var courrier = new Courrier
            {
                OrdreNumero = model.OrdreNumero,
                RegistreNumero = model.RegistreNumero,
                ReferenceNumero = model.ReferenceNumero,
                Objet = model.Objet,
                DateReception = model.DateReception,
                HeureRecu = model.HeureRecu,
                Nature = model.Nature,
                TypeDossierId = model.TypeDossierId,
                ServiceConcerne = model.ServiceConcerne,
                DossierDeClassement = model.DossierDeClassement,
                UtilisateursEnCopie = model.UtilisateursEnCopie,
                CorrespondantId = model.CorrespondantId,
                ServiceId = model.ServiceId,
                CategorieCourrierId = categorieId,
                ModeEnvoiId = model.ModeEnvoiId,
                Confidentialite = model.Confidentialite,
                Priorite = model.Priorite,
                DateEnregistrement = DateTime.UtcNow,
                // When created by indexateur, move to validation
                Statut = "A valider"
            };

            // set creator
            var userId = _userManager.GetUserId(User);
            courrier.CreatedById = userId;

            _context.Courriers.Add(courrier);
            await _context.SaveChangesAsync();

            // If user previewed attachments, add a history record noting that
            if (model.Attachments != null && model.Attachments.Count > 0 && model.VisualizedBeforeSave)
            {
                var previewHist = new CourrierHistory
                {
                    CourrierId = courrier.Id,
                    UserId = userId,
                    Action = "Fichiers visualisés",
                    Note = "Pièces jointes visualisées par l'indexateur avant enregistrement.",
                    Timestamp = DateTime.UtcNow
                };
                _context.CourrierHistories!.Add(previewHist);
                await _context.SaveChangesAsync();
            }

            // add main saisie history record
            var hist = new CourrierHistory
            {
                CourrierId = courrier.Id,
                UserId = userId,
                Action = "Saisi",
                Note = "Courrier saisi et envoyé à validation",
                Timestamp = DateTime.UtcNow
            };
            _context.CourrierHistories!.Add(hist);
            await _context.SaveChangesAsync();

            // Handle attachments
            if (model.Attachments != null && model.Attachments.Count > 0)
            {
                var uploadRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
                if (!Directory.Exists(uploadRoot)) Directory.CreateDirectory(uploadRoot);

                foreach (var file in model.Attachments)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Path.GetRandomFileName() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadRoot, fileName);
                        using (var stream = System.IO.File.Create(filePath))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var doc = new Document
                        {
                            FileName = file.FileName,
                            ContentType = file.ContentType,
                            FileSize = file.Length,
                            FilePath = Path.Combine("/uploads", fileName).Replace("\\", "/"),
                            CourrierId = courrier.Id,
                            UploadedAt = DateTime.UtcNow
                        };
                        _context.Documents!.Add(doc);
                    }
                }
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Courrier enregistré avec succès.";
            TempData["RefreshDashboard"] = true; // Signal to refresh the dashboard
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateSelects()
        {
            ViewBag.Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.CategorieCourriers!.ToListAsync(), "Id", "Nom");
            ViewBag.Modes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.ModeEnvois!.ToListAsync(), "Id", "Nom");
            ViewBag.TypeDossiers = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.TypeDossiers!.ToListAsync(), "Id", "Libelle");
            ViewBag.Services = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Services!.ToListAsync(), "Id", "Nom");
            ViewBag.Correspondants = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Correspondants!.ToListAsync(), "Id", "Nom");
        }
    }
}
