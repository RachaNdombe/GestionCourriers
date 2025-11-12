using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JconsultGC.Models;
using JconsultGC.Data;

namespace JconsultGC.Controllers
{
    [Authorize]
    public class ModificationCourrierController : Controller
    {
        private readonly ProjetIdDbContext _context;
        private readonly UserManager<User> _userManager;

        public ModificationCourrierController(ProjetIdDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: ModificationCourrier/Modifier/5 - Modifier un courrier signé
        public async Task<IActionResult> Modifier(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courrier = await _context.Courriers
                .Include(c => c.Service)
                .Include(c => c.Correspondant)
                .Include(c => c.NatureCourrier)
                .Include(c => c.DossierClassement)
                .Include(c => c.TypeDossier)
                .Include(c => c.ModeEnvoi)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (courrier == null)
            {
                return NotFound();
            }

            // Vérifier que le courrier est signé
            if (courrier.Statut != "Signé")
            {
                TempData["Error"] = "Seuls les courriers signés peuvent être modifiés.";
                return RedirectToAction("Details", "Courriers", new { id = courrier.Id });
            }

            // Vérifier que l'utilisateur a le droit de modifier ce courrier
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId!);
            
            if (user == null || (courrier.CreatedById != userId && !await _userManager.IsInRoleAsync(user, "Admin")))
            {
                TempData["Error"] = "Vous n'avez pas l'autorisation de modifier ce courrier.";
                return RedirectToAction("Details", "Courriers", new { id = courrier.Id });
            }

            return View(courrier);
        }

        // POST: ModificationCourrier/Modifier/5 - Enregistrer les modifications
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Modifier(int id, Courrier courrier, string raisonModification)
        {
            if (id != courrier.Id)
            {
                return NotFound();
            }

            var existingCourrier = await _context.Courriers
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existingCourrier == null)
            {
                return NotFound();
            }

            // Vérifier que le courrier est signé
            if (existingCourrier.Statut != "Signé")
            {
                TempData["Error"] = "Seuls les courriers signés peuvent être modifiés.";
                return RedirectToAction("Details", "Courriers", new { id = courrier.Id });
            }

            // Vérifier les autorisations
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId!);
            
            if (user == null || (existingCourrier.CreatedById != userId && !await _userManager.IsInRoleAsync(user, "Admin")))
            {
                TempData["Error"] = "Vous n'avez pas l'autorisation de modifier ce courrier.";
                return RedirectToAction("Details", "Courriers", new { id = courrier.Id });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Enregistrer les modifications dans l'historique
                    var modifications = new List<ModificationCourrier>();

                    // Vérifier chaque champ modifié
                    if (existingCourrier.Objet != courrier.Objet)
                    {
                        modifications.Add(new ModificationCourrier
                        {
                            CourrierId = courrier.Id,
                            UserId = userId!,
                            ChampModifie = "Objet",
                            AncienneValeur = existingCourrier.Objet,
                            NouvelleValeur = courrier.Objet,
                            RaisonModification = raisonModification
                        });
                        existingCourrier.Objet = courrier.Objet;
                    }

                    // Ajouter d'autres champs modifiables selon le modèle Courrier
                    // Note: Les champs spécifiques comme Note, Reference, DateCourrier
                    // doivent être vérifiés selon la structure réelle du modèle Courrier

                    // Ajouter les modifications à la base de données
                    if (modifications.Any())
                    {
                        _context.ModificationsCourrier.AddRange(modifications);
                        _context.Update(existingCourrier);
                        await _context.SaveChangesAsync();

                        TempData["Success"] = "Modifications enregistrées avec succès.";
                    }
                    else
                    {
                        TempData["Info"] = "Aucune modification détectée.";
                    }

                    return RedirectToAction("Details", "Courriers", new { id = courrier.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourrierExists(courrier.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Recharger les données de navigation si le modèle n'est pas valide
            var courrierWithNav = await _context.Courriers
                .Include(c => c.Service)
                .Include(c => c.Correspondant)
                .Include(c => c.NatureCourrier)
                .Include(c => c.DossierClassement)
                .Include(c => c.TypeDossier)
                .Include(c => c.ModeEnvoi)
                .FirstOrDefaultAsync(c => c.Id == id);

            return View(courrierWithNav);
        }

        // GET: ModificationCourrier/Historique/5 - Voir l'historique des modifications
        public async Task<IActionResult> Historique(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courrier = await _context.Courriers
                .FirstOrDefaultAsync(c => c.Id == id);

            if (courrier == null)
            {
                return NotFound();
            }

            var modifications = await _context.ModificationsCourrier
                .Include(m => m.User)
                .Where(m => m.CourrierId == id)
                .OrderByDescending(m => m.DateModification)
                .ToListAsync();

            ViewData["Courrier"] = courrier;
            return View(modifications);
        }

        private bool CourrierExists(int id)
        {
            return _context.Courriers.Any(e => e.Id == id);
        }
    }
}