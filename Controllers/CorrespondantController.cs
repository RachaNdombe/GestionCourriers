using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using JconsultGC.Models;
using JconsultGC.Data;

namespace JconsultGC.Controllers;

[Authorize]
[Route("Correspondants")]
public class CorrespondantController : Controller
{
    private readonly ProjetIdDbContext _context;
    private readonly ILogger<CorrespondantController> _logger;

    public CorrespondantController(ProjetIdDbContext context, ILogger<CorrespondantController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var correspondants = await _context.Correspondants
            .OrderBy(c => c.Nom)
            .ToListAsync();
        return View(correspondants);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Correspondant correspondant)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Vérifier si un correspondant avec le même email existe déjà
                if (!string.IsNullOrEmpty(correspondant.Email))
                {
                    var existingCorrespondant = await _context.Correspondants
                        .FirstOrDefaultAsync(c => c.Email == correspondant.Email);
                    if (existingCorrespondant != null)
                    {
                        return Json(new { success = false, message = "Un expéditeur avec cet email existe déjà" });
                    }
                }

                // Nettoyer les données
                correspondant.Nom = correspondant.Nom?.Trim();
                correspondant.Email = correspondant.Email?.Trim().ToLower();
                correspondant.Telephone = correspondant.Telephone?.Trim();
                correspondant.Societe = correspondant.Societe?.Trim();
                correspondant.Adresse = correspondant.Adresse?.Trim();

                await _context.Correspondants.AddAsync(correspondant);
                await _context.SaveChangesAsync();

                var displayName = !string.IsNullOrEmpty(correspondant.Societe)
                    ? correspondant.Societe
                    : correspondant.Nom;

                return Json(new {
                    success = true,
                    id = correspondant.Id,
                    nom = displayName,
                    message = "Expéditeur créé avec succès"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur: {ex.Message}" });
            }
        }

        // Retourner les erreurs de validation
        var errors = ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();
        
        return Json(new { success = false, message = "Erreurs de validation", errors = errors });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var correspondant = await _context.Correspondants.FindAsync(id);
        if (correspondant == null)
        {
            return NotFound();
        }
        return View(correspondant);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Correspondant correspondant)
    {
        if (id != correspondant.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Vérifier si un correspondant avec le même email existe déjà (excluant l'actuel)
                if (!string.IsNullOrEmpty(correspondant.Email))
                {
                    var existingCorrespondant = await _context.Correspondants
                        .FirstOrDefaultAsync(c => c.Email == correspondant.Email && c.Id != id);
                    if (existingCorrespondant != null)
                    {
                        ModelState.AddModelError("Email", "Un expéditeur avec cet email existe déjà");
                        return View(correspondant);
                    }
                }

                // Nettoyer les données
                correspondant.Nom = correspondant.Nom?.Trim();
                correspondant.Email = correspondant.Email?.Trim().ToLower();
                correspondant.Telephone = correspondant.Telephone?.Trim();
                correspondant.Societe = correspondant.Societe?.Trim();
                correspondant.Adresse = correspondant.Adresse?.Trim();

                _context.Correspondants.Update(correspondant);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Expéditeur modifié avec succès";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CorrespondantExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        return View(correspondant);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var correspondant = await _context.Correspondants
            .Include(c => c.Courriers)
            .FirstOrDefaultAsync(c => c.Id == id);
            
        if (correspondant == null)
        {
            return NotFound();
        }
        return View(correspondant);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var correspondant = await _context.Correspondants.FindAsync(id);
        if (correspondant == null)
        {
            return NotFound();
        }

        _context.Correspondants.Remove(correspondant);
        await _context.SaveChangesAsync();
        
        TempData["SuccessMessage"] = "Expéditeur supprimé avec succès";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route("CreateAjax")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAjax([FromBody] CorrespondantAjaxRequest request)
    {
        try
        {
            _logger.LogInformation("CreateAjax called with data: {@Request}", request);

            // Vérifier si les données sont présentes
            if (request == null)
            {
                return Json(new { success = false, message = "Aucune donnée reçue" });
            }

            // Validation des champs obligatoires
            if (string.IsNullOrWhiteSpace(request.Nom))
            {
                return Json(new { success = false, message = "Le nom est obligatoire" });
            }

            // Vérifier si un correspondant avec le même email existe déjà
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var existingCorrespondant = await _context.Correspondants
                    .FirstOrDefaultAsync(c => c.Email == request.Email);
                if (existingCorrespondant != null)
                {
                    return Json(new { success = false, message = "Un expéditeur avec cet email existe déjà" });
                }
            }

            // Créer le correspondant
            var correspondant = new Correspondant
            {
                Nom = request.Nom.Trim(),
                Societe = request.RaisonSociale == "Organisation" ? request.Nom.Trim() : null,
                Email = !string.IsNullOrWhiteSpace(request.Email) ? request.Email.Trim().ToLower() : null,
                Telephone = !string.IsNullOrWhiteSpace(request.Contact1) ? request.Contact1.Trim() : null,
                Adresse = !string.IsNullOrWhiteSpace(request.Adresse) ? request.Adresse.Trim() : null
            };

            _logger.LogInformation("Creating correspondant: {@Correspondant}", correspondant);

            await _context.Correspondants.AddAsync(correspondant);
            await _context.SaveChangesAsync();

            var displayName = !string.IsNullOrEmpty(correspondant.Societe)
                ? correspondant.Societe
                : correspondant.Nom;

            _logger.LogInformation("Correspondant created successfully with ID: {Id}", correspondant.Id);

            return Json(new {
                success = true,
                id = correspondant.Id,
                nom = displayName,
                message = "Expéditeur enregistré avec succès"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création d'un correspondant via AJAX");
            return Json(new { success = false, message = $"Erreur: {ex.Message}" });
        }
    }

    [HttpGet]
    [Route("GetAll")]
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

    private bool CorrespondantExists(int id)
    {
        return _context.Correspondants.Any(e => e.Id == id);
    }
}

public class CorrespondantAjaxRequest
{
    public string RaisonSociale { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string? Civilite { get; set; }
    public string? Email { get; set; }
    public string? Contact1 { get; set; }
    public string? Adresse { get; set; }
    public string? Ville { get; set; }
    public string? CodePostal { get; set; }
    public string? Pays { get; set; }
}