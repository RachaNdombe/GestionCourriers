using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using JconsultGC.Models;
using JconsultGC.Data;

namespace JconsultGC.Controllers;

[Authorize]
public class TypeDossierController : Controller
{
    private readonly ProjetIdDbContext _context;
    private readonly ILogger<TypeDossierController> _logger;

    public TypeDossierController(ProjetIdDbContext context, ILogger<TypeDossierController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] TypeDossier model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Les données saisies sont invalides" });
            }

            if (_context.TypeDossiers == null)
            {
                return Json(new { success = false, message = "Service non disponible" });
            }

            // Vérifier si un type de dossier avec le même libellé existe déjà
            var existingType = await _context.TypeDossiers
                .FirstOrDefaultAsync(t => t.Libelle == model.Libelle);

            if (existingType != null)
            {
                return Json(new { success = false, message = "Un type de dossier avec ce libellé existe déjà" });
            }

            // Nettoyer les données
            model.Libelle = model.Libelle?.Trim();
            model.Description = model.Description?.Trim();

            await _context.TypeDossiers.AddAsync(model);
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                id = model.Id, 
                libelle = model.Libelle,
                message = "Type de dossier enregistré avec succès"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création d'un type de dossier");
            return Json(new { success = false, message = "Une erreur est survenue lors de l'enregistrement" });
        }
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        try
        {
            if (_context.TypeDossiers == null)
            {
                return Json(new { success = false, message = "Service non disponible" });
            }

            var types = _context.TypeDossiers
                .OrderBy(t => t.Libelle)
                .Select(t => new { 
                    id = t.Id, 
                    libelle = t.Libelle,
                    description = t.Description
                })
                .ToList();

            return Json(new { success = true, data = types });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des types de dossier");
            return Json(new { success = false, message = "Une erreur est survenue" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            if (_context.TypeDossiers == null)
            {
                return Json(new { success = false, message = "Service non disponible" });
            }

            var type = await _context.TypeDossiers.FindAsync(id);
            if (type == null)
            {
                return NotFound();
            }

            return Json(new { success = true, data = type });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du type de dossier {Id}", id);
            return Json(new { success = false, message = "Une erreur est survenue" });
        }
    }
}