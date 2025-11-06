using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using JconsultGC.Models;
using JconsultGC.Data;

namespace JconsultGC.Controllers;

[Authorize]
public class NatureCourriersController : Controller
{
    private readonly ProjetIdDbContext _context;
    private readonly ILogger<NatureCourriersController> _logger;

    public NatureCourriersController(ProjetIdDbContext context, ILogger<NatureCourriersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] NatureCourrier model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Les données saisies sont invalides" });
            }

            if (_context.NatureCourriers == null)
            {
                return Json(new { success = false, message = "Service non disponible" });
            }

            // Vérifier si une nature avec le même nom existe déjà
            var existingNature = await _context.NatureCourriers
                .FirstOrDefaultAsync(n => n.Nom == model.Nom);
                
            if (existingNature != null)
            {
                return Json(new { success = false, message = "Une nature avec ce nom existe déjà" });
            }

            // Nettoyer les données
            model.Nom = model.Nom?.Trim();
            model.Description = model.Description?.Trim();

            await _context.NatureCourriers.AddAsync(model);
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                id = model.Id, 
                nom = model.Nom,
                message = "Nature enregistrée avec succès"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création d'une nature");
            return Json(new { success = false, message = "Une erreur est survenue lors de l'enregistrement" });
        }
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        try
        {
            if (_context.NatureCourriers == null)
            {
                return Json(new { success = false, message = "Service non disponible" });
            }

            var natures = _context.NatureCourriers
                .OrderBy(n => n.Nom)
                .Select(n => new { 
                    id = n.Id, 
                    nom = n.Nom,
                    description = n.Description
                })
                .ToList();

            return Json(new { success = true, data = natures });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des natures");
            return Json(new { success = false, message = "Une erreur est survenue" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            if (_context.NatureCourriers == null)
            {
                return Json(new { success = false, message = "Service non disponible" });
            }

            var nature = await _context.NatureCourriers.FindAsync(id);
            if (nature == null)
            {
                return NotFound();
            }

            return Json(new { success = true, data = nature });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de la nature {Id}", id);
            return Json(new { success = false, message = "Une erreur est survenue" });
        }
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, [FromForm] NatureCourrier model)
    {
        try
        {
            if (id != model.Id)
            {
                return BadRequest(new { success = false, message = "Les identifiants ne correspondent pas" });
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Les données saisies sont invalides" });
            }

            if (_context.NatureCourriers == null)
            {
                return Json(new { success = false, message = "Service non disponible" });
            }

            // Vérifier si une autre nature avec le même nom existe déjà
            var existingNature = await _context.NatureCourriers
                .FirstOrDefaultAsync(n => n.Nom == model.Nom && n.Id != id);
                
            if (existingNature != null)
            {
                return Json(new { success = false, message = "Une autre nature avec ce nom existe déjà" });
            }

            // Nettoyer les données
            model.Nom = model.Nom?.Trim();
            model.Description = model.Description?.Trim();

            try
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await NatureCourrierExists(id))
                {
                    return NotFound(new { success = false, message = "Nature introuvable" });
                }
                throw;
            }

            return Json(new { 
                success = true, 
                message = "Nature modifiée avec succès" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la modification de la nature {Id}", id);
            return Json(new { success = false, message = "Une erreur est survenue lors de la modification" });
        }
    }

    [HttpDelete]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            if (_context.NatureCourriers == null)
            {
                return Json(new { success = false, message = "Service non disponible" });
            }

            var nature = await _context.NatureCourriers
                .Include(n => n.Courriers)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (nature == null)
            {
                return NotFound(new { success = false, message = "Nature introuvable" });
            }

            // Vérifier s'il existe des courriers liés à cette nature
            if (nature.Courriers != null && nature.Courriers.Any())
            {
                return Json(new { 
                    success = false, 
                    message = "Impossible de supprimer cette nature car elle est utilisée par des courriers" 
                });
            }

            _context.NatureCourriers.Remove(nature);
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                message = "Nature supprimée avec succès" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de la nature {Id}", id);
            return Json(new { success = false, message = "Une erreur est survenue lors de la suppression" });
        }
    }

    private async Task<bool> NatureCourrierExists(int id)
    {
        return _context.NatureCourriers != null && 
               await _context.NatureCourriers.AnyAsync(n => n.Id == id);
    }
}