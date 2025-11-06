using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JconsultGC.Models;
using JconsultGC.Data;

namespace JconsultGC.Controllers;

[Authorize]
public class CorrespondantController : Controller
{
    private readonly ProjetIdDbContext _context;
    private readonly ILogger<CorrespondantController> _logger;

    public CorrespondantController(ProjetIdDbContext context, ILogger<CorrespondantController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] Correspondant model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Les données saisies sont invalides" });
            }

            // Vérifier si un correspondant avec le même email existe déjà
            if (!string.IsNullOrEmpty(model.Email))
            {
                var existingCorrespondant = _context.Correspondants.FirstOrDefault(c => c.Email == model.Email);
                if (existingCorrespondant != null)
                {
                    return Json(new { success = false, message = "Un expéditeur avec cet email existe déjà" });
                }
            }

            // Nettoyer les données
            model.Nom = model.Nom?.Trim();
            model.Email = model.Email?.Trim().ToLower();
            model.Telephone = model.Telephone?.Trim();
            model.Societe = model.Societe?.Trim();
            model.Adresse = model.Adresse?.Trim();

            await _context.Correspondants.AddAsync(model);
            await _context.SaveChangesAsync();

            var displayName = !string.IsNullOrEmpty(model.Societe) 
                ? $"{model.Nom} ({model.Societe})"
                : model.Nom;

            return Json(new { 
                success = true, 
                id = model.Id, 
                nom = displayName,
                message = "Expéditeur enregistré avec succès"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création d'un correspondant");
            return Json(new { success = false, message = "Une erreur est survenue lors de l'enregistrement" });
        }
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        try
        {
            var correspondants = _context.Correspondants
                .OrderBy(c => c.Nom)
                .Select(c => new { 
                    id = c.Id, 
                    nom = !string.IsNullOrEmpty(c.Societe) 
                        ? $"{c.Nom} ({c.Societe})"
                        : c.Nom
                })
                .ToList();

            return Json(new { success = true, data = correspondants });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des correspondants");
            return Json(new { success = false, message = "Une erreur est survenue" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var correspondant = await _context.Correspondants.FindAsync(id);
            if (correspondant == null)
            {
                return NotFound();
            }

            return Json(new { success = true, data = correspondant });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du correspondant {Id}", id);
            return Json(new { success = false, message = "Une erreur est survenue" });
        }
    }
}