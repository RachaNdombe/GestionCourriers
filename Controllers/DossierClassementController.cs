using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using JconsultGC.Models;
using JconsultGC.Data;

namespace JconsultGC.Controllers;

[Authorize]
public class DossierClassementController : Controller
{
    private readonly ProjetIdDbContext _context;
    private readonly ILogger<DossierClassementController> _logger;

    public DossierClassementController(ProjetIdDbContext context, ILogger<DossierClassementController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] DossierClassement model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Les données saisies sont invalides" });
            }

            if (_context.DossierClassements == null)
            {
                return Json(new { success = false, message = "Service non disponible" });
            }

            // Vérifier si un dossier avec le même titre existe déjà
            var existingDossier = await _context.DossierClassements
                .FirstOrDefaultAsync(d => d.Titre == model.Titre);

            if (existingDossier != null)
            {
                return Json(new { success = false, message = "Un dossier de classement avec ce titre existe déjà" });
            }

            // Nettoyer les données
            model.Titre = model.Titre?.Trim();
            model.Description = model.Description?.Trim();
            model.CreatedAt = DateTime.UtcNow;

            await _context.DossierClassements.AddAsync(model);
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                id = model.Id, 
                titre = model.Titre,
                message = "Dossier de classement enregistré avec succès"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création d'un dossier de classement");
            return Json(new { success = false, message = "Une erreur est survenue lors de l'enregistrement" });
        }
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        try
        {
            if (_context.DossierClassements == null)
            {
                return Json(new { success = false, message = "Service non disponible" });
            }

            var dossiers = _context.DossierClassements
                .Include(d => d.TypeDossier)
                .OrderBy(d => d.Titre)
                .Select(d => new { 
                    id = d.Id, 
                    titre = d.Titre,
                    description = d.Description,
                    typeDossier = d.TypeDossier == null ? null : new {
                        id = d.TypeDossier.Id,
                        libelle = d.TypeDossier.Libelle
                    },
                    createdAt = d.CreatedAt
                })
                .ToList();

            return Json(new { success = true, data = dossiers });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des dossiers de classement");
            return Json(new { success = false, message = "Une erreur est survenue" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            if (_context.DossierClassements == null)
            {
                return Json(new { success = false, message = "Service non disponible" });
            }

            var dossier = await _context.DossierClassements
                .Include(d => d.TypeDossier)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (dossier == null)
            {
                return NotFound();
            }

            return Json(new { success = true, data = dossier });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du dossier de classement {Id}", id);
            return Json(new { success = false, message = "Une erreur est survenue" });
        }
    }
}