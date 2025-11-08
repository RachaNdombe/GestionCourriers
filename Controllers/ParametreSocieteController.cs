using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using JconsultGC.Models;
using JconsultGC.Data;

namespace JconsultGC.Controllers
{
    [Authorize]
    public class ParametreSocieteController : Controller
    {
        private readonly ProjetIdDbContext _context;
        private readonly ILogger<ParametreSocieteController> _logger;

        public ParametreSocieteController(ProjetIdDbContext context, ILogger<ParametreSocieteController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: ParametreSociete
        public async Task<IActionResult> Index()
        {
            try
            {
                var parametre = await _context.ParametresSociete.FirstOrDefaultAsync();
                
                if (parametre == null)
                {
                    // Aucun paramètre existant, rediriger vers la création
                    return RedirectToAction(nameof(Create));
                }

                return View(parametre);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des paramètres de société");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la récupération des paramètres de société.";
                return View(new ParametreSociete());
            }
        }

        // GET: ParametreSociete/Create
        public IActionResult Create()
        {
            return View(new ParametreSociete());
        }

        // POST: ParametreSociete/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ParametreSociete model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Vérifier s'il existe déjà des paramètres
                    var existingParametre = await _context.ParametresSociete.FirstOrDefaultAsync();
                    if (existingParametre != null)
                    {
                        TempData["ErrorMessage"] = "Des paramètres de société existent déjà. Utilisez la modification pour les mettre à jour.";
                        return RedirectToAction(nameof(Index));
                    }

                    _context.ParametresSociete.Add(model);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Les paramètres de société ont été créés avec succès.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de la création des paramètres de société");
                    TempData["ErrorMessage"] = "Une erreur est survenue lors de la création des paramètres de société.";
                }
            }

            return View(model);
        }

        // GET: ParametreSociete/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var parametre = await _context.ParametresSociete.FindAsync(id);
                if (parametre == null)
                {
                    return NotFound();
                }

                return View(parametre);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des paramètres de société {Id}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la récupération des paramètres de société.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: ParametreSociete/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ParametreSociete model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var parametre = await _context.ParametresSociete.FindAsync(id);
                    if (parametre == null)
                    {
                        return NotFound();
                    }

                    parametre.NomSociete = model.NomSociete;
                    parametre.Adresse = model.Adresse;
                    parametre.Telephone = model.Telephone;
                    parametre.Email = model.Email;
                    parametre.LogoPath = model.LogoPath;

                    _context.Update(parametre);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Les paramètres de société ont été modifiés avec succès.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ParametreSocieteExistsAsync(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de la modification des paramètres de société {Id}", id);
                    TempData["ErrorMessage"] = "Une erreur est survenue lors de la modification des paramètres de société.";
                }
            }

            return View(model);
        }

        private async Task<bool> ParametreSocieteExistsAsync(int id)
        {
            return await _context.ParametresSociete.AnyAsync(e => e.Id == id);
        }
    }
}