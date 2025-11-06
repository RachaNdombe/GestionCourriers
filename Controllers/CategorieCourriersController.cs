using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using JconsultGC.Models;
using JconsultGC.Data;

namespace JconsultGC.Controllers;

[Authorize]
public class CategorieCourriersController : Controller
{
    private readonly ProjetIdDbContext _context;
    private readonly ILogger<CategorieCourriersController> _logger;

    public CategorieCourriersController(ProjetIdDbContext context, ILogger<CategorieCourriersController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] CategorieCourrier model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Les données saisies sont invalides" });
            }

            if (_context.CategorieCourriers == null)
            {
                return Json(new { success = false, message = "Service non disponible" });
            }

            // Vérifier si une catégorie avec le même nom ou code existe déjà
            var existingCategorie = await _context.CategorieCourriers
                .FirstOrDefaultAsync(c => c.Nom == model.Nom || c.Code == model.Code);

            if (existingCategorie != null)
            {
                if (existingCategorie.Nom == model.Nom)
                {
                    return Json(new { success = false, message = "Une catégorie avec ce nom existe déjà" });
                }
                if (existingCategorie.Code == model.Code)
                {
                    return Json(new { success = false, message = "Une catégorie avec ce code existe déjà" });
                }
            }

            // Nettoyer les données
            model.Nom = model.Nom?.Trim();
            model.Code = model.Code?.Trim()?.ToUpper(); // Les codes sont généralement en majuscules

            await _context.CategorieCourriers.AddAsync(model);
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                id = model.Id, 
                nom = model.Nom,
                message = "Catégorie enregistrée avec succès"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création d'une catégorie");
            return Json(new { success = false, message = "Une erreur est survenue lors de l'enregistrement" });
        }
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        try
        {
            if (_context.CategorieCourriers == null)
            {
                return Json(new { success = false, message = "Service non disponible" });
            }

            var categories = _context.CategorieCourriers
                .OrderBy(c => c.Nom)
                .Select(c => new { 
                    id = c.Id, 
                    nom = c.Nom,
                    code = c.Code,
                    nbCourriers = c.Courriers != null ? c.Courriers.Count : 0
                })
                .ToList();

            return Json(new { success = true, data = categories });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des catégories");
            return Json(new { success = false, message = "Une erreur est survenue" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            if (_context.CategorieCourriers == null)
            {
                return Json(new { success = false, message = "Service non disponible" });
            }

            var categorie = await _context.CategorieCourriers
                .Include(c => c.Courriers)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (categorie == null)
            {
                return NotFound();
            }

            return Json(new { 
                success = true, 
                data = new {
                    id = categorie.Id,
                    nom = categorie.Nom,
                    code = categorie.Code,
                    nbCourriers = categorie.Courriers?.Count ?? 0
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de la catégorie {Id}", id);
            return Json(new { success = false, message = "Une erreur est survenue" });
        }
    }

    [HttpDelete]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            if (_context.CategorieCourriers == null)
            {
                return Json(new { success = false, message = "Service non disponible" });
            }

            var categorie = await _context.CategorieCourriers
                .Include(c => c.Courriers)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categorie == null)
            {
                return NotFound();
            }

            // Vérifier si la catégorie est utilisée
            if (categorie.Courriers != null && categorie.Courriers.Any())
            {
                return Json(new { 
                    success = false, 
                    message = "Impossible de supprimer cette catégorie car elle est utilisée par des courriers" 
                });
            }

            _context.CategorieCourriers.Remove(categorie);
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                message = "Catégorie supprimée avec succès" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de la catégorie {Id}", id);
            return Json(new { success = false, message = "Une erreur est survenue lors de la suppression" });
        }
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [FromForm] CategorieCourrier model)
    {
        try
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Les données saisies sont invalides" });
            }

            if (_context.CategorieCourriers == null)
            {
                return Json(new { success = false, message = "Service non disponible" });
            }

            // Vérifier si une autre catégorie avec le même nom ou code existe déjà
            var existingCategorie = await _context.CategorieCourriers
                .FirstOrDefaultAsync(c => c.Id != id && (c.Nom == model.Nom || c.Code == model.Code));

            if (existingCategorie != null)
            {
                if (existingCategorie.Nom == model.Nom)
                {
                    return Json(new { success = false, message = "Une autre catégorie avec ce nom existe déjà" });
                }
                if (existingCategorie.Code == model.Code)
                {
                    return Json(new { success = false, message = "Une autre catégorie avec ce code existe déjà" });
                }
            }

            // Nettoyer les données
            model.Nom = model.Nom?.Trim();
            model.Code = model.Code?.Trim()?.ToUpper();

            _context.Entry(model).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.CategorieCourriers.AnyAsync(c => c.Id == id))
                {
                    return NotFound();
                }
                throw;
            }

            return Json(new { 
                success = true, 
                id = model.Id,
                nom = model.Nom,
                code = model.Code,
                message = "Catégorie modifiée avec succès"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la modification de la catégorie {Id}", id);
            return Json(new { success = false, message = "Une erreur est survenue lors de la modification" });
        }
    }
}