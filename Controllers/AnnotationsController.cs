using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JconsultGC.Data;
using JconsultGC.Models;
using JconsultGC.ViewModels;

namespace JconsultGC.Controllers;

/// <summary>
/// Contrôleur pour la gestion des annotations de courrier
/// </summary>
[Authorize]
public class AnnotationsController : Controller
{
    private readonly ProjetIdDbContext _context;
    private readonly UserManager<User> _userManager;

    public AnnotationsController(
        ProjetIdDbContext context,
        UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    /// <summary>
    /// Affiche la liste des annotations d'un courrier
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(int courrierId)
    {
        var courrier = await _context.Courriers
            .FirstOrDefaultAsync(c => c.Id == courrierId);

        if (courrier == null)
        {
            return NotFound();
        }

        var annotations = await _context.Annotations!
            .Include(a => a.Auteur)
            .Where(a => a.CourrierId == courrierId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        var viewModel = new AnnotationsListViewModel
        {
            CourrierId = courrierId,
            NumeroOrdre = courrier.OrdreNumero,
            Objet = courrier.Objet,
            Annotations = annotations.Select(a => new AnnotationViewModel
            {
                Id = a.Id,
                CourrierId = a.CourrierId ?? 0,
                Note = a.Texte,
                UserNoteNom = $"{a.Auteur?.Nom} {a.Auteur?.Prenom}",
                DateNote = a.CreatedAt,
                CreatedAt = a.CreatedAt
            }).ToList(),
            CreateAnnotation = new CreateAnnotationViewModel
            {
                CourrierId = courrierId,
                AvailableUsers = await GetAvailableUsersAsync()
            }
        };

        return View(viewModel);
    }

    /// <summary>
    /// Crée une nouvelle annotation
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAnnotationViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var annotation = new Annotation
            {
                CourrierId = model.CourrierId,
                Texte = model.Note,
                AuteurUserId = int.Parse(user.Id),
                CreatedAt = DateTime.UtcNow
            };

            _context.Annotations!.Add(annotation);
            await _context.SaveChangesAsync();

            // Enregistrer l'action dans l'historique
            var historiqueAction = new CourrierHistory
            {
                CourrierId = model.CourrierId,
                UserId = user.Id,
                Action = "Annotation ajoutée",
                Note = $"Annotation ajoutée : {model.Note.Substring(0, Math.Min(100, model.Note.Length))}...",
                Timestamp = DateTime.UtcNow
            };
            _context.CourrierHistories!.Add(historiqueAction);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Annotation ajoutée avec succès !";
        }
        else
        {
            TempData["ErrorMessage"] = "Erreur lors de l'ajout de l'annotation.";
        }

        return RedirectToAction("Index", new { courrierId = model.CourrierId });
    }

    /// <summary>
    /// Supprime une annotation
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var annotation = await _context.Annotations!.FindAsync(id);
        if (annotation == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        // Vérifier que l'utilisateur peut supprimer cette annotation
        if (annotation.AuteurUserId != int.Parse(user.Id) && !User.IsInRole("Administrateur"))
        {
            TempData["ErrorMessage"] = "Vous n'êtes pas autorisé à supprimer cette annotation.";
            return RedirectToAction("Index", new { courrierId = annotation.CourrierId });
        }

        // Enregistrer l'action dans l'historique
        var historiqueAction = new CourrierHistory
        {
            CourrierId = annotation.CourrierId ?? 0,
            UserId = user.Id,
            Action = "Annotation supprimée",
            Note = $"Annotation supprimée : {annotation.Texte?.Substring(0, Math.Min(100, annotation.Texte?.Length ?? 0))}...",
            Timestamp = DateTime.UtcNow
        };
        _context.CourrierHistories!.Add(historiqueAction);

        _context.Annotations!.Remove(annotation);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Annotation supprimée avec succès !";
        return RedirectToAction("Index", new { courrierId = annotation.CourrierId });
    }

    /// <summary>
    /// Obtient la liste des utilisateurs disponibles
    /// </summary>
    private async Task<List<JconsultGC.Models.LookupOption>> GetAvailableUsersAsync()
    {
        var users = await _userManager.Users
            .OrderBy(u => u.Nom)
            .ThenBy(u => u.Prenom)
            .ToListAsync();

        return users.Select(u => new JconsultGC.Models.LookupOption
        {
            Id = int.Parse(u.Id),
            Nom = $"{u.Nom} {u.Prenom}"
        }).ToList();
    }
}
