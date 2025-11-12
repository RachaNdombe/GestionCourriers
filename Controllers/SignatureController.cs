using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JconsultGC.Models;
using JconsultGC.Data;

namespace JconsultGC.Controllers
{
    [Authorize]
    public class SignatureController : Controller
    {
        private readonly ProjetIdDbContext _context;
        private readonly UserManager<User> _userManager;

        public SignatureController(ProjetIdDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Signature/Index - Liste des signatures de l'utilisateur
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var signatures = await _context.Signatures
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.EstParDefaut)
                .ThenBy(s => s.Nom)
                .ToListAsync();

            return View(signatures);
        }

        // GET: Signature/Create - Créer une nouvelle signature
        public IActionResult Create()
        {
            return View();
        }

        // POST: Signature/Create - Créer une nouvelle signature
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Signature signature)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                signature.UserId = userId!;
                signature.DateCreation = DateTime.Now;

                // Si c'est la première signature ou si elle est marquée comme par défaut
                var existingSignatures = await _context.Signatures
                    .Where(s => s.UserId == userId)
                    .ToListAsync();

                if (signature.EstParDefaut)
                {
                    // Désactiver les autres signatures par défaut
                    foreach (var existing in existingSignatures)
                    {
                        existing.EstParDefaut = false;
                    }
                }
                else if (!existingSignatures.Any())
                {
                    // Première signature, la définir comme par défaut
                    signature.EstParDefaut = true;
                }

                _context.Add(signature);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(signature);
        }

        // GET: Signature/Edit/5 - Modifier une signature
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var signature = await _context.Signatures
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (signature == null)
            {
                return NotFound();
            }

            return View(signature);
        }

        // POST: Signature/Edit/5 - Modifier une signature
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Signature signature)
        {
            if (id != signature.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = _userManager.GetUserId(User);
                    var existingSignature = await _context.Signatures
                        .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

                    if (existingSignature == null)
                    {
                        return NotFound();
                    }

                    existingSignature.Nom = signature.Nom;
                    existingSignature.Description = signature.Description;
                    existingSignature.CheminSignature = signature.CheminSignature;
                    existingSignature.Type = signature.Type;
                    existingSignature.DateModification = DateTime.Now;

                    // Gérer la signature par défaut
                    if (signature.EstParDefaut && !existingSignature.EstParDefaut)
                    {
                        // Désactiver les autres signatures par défaut
                        var otherSignatures = await _context.Signatures
                            .Where(s => s.UserId == userId && s.Id != id)
                            .ToListAsync();

                        foreach (var other in otherSignatures)
                        {
                            other.EstParDefaut = false;
                        }
                    }

                    existingSignature.EstParDefaut = signature.EstParDefaut;

                    _context.Update(existingSignature);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SignatureExists(signature.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(signature);
        }

        // POST: Signature/Delete/5 - Supprimer une signature
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var signature = await _context.Signatures
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (signature == null)
            {
                return NotFound();
            }

            _context.Signatures.Remove(signature);
            await _context.SaveChangesAsync();

            // Si c'était la signature par défaut, définir une autre comme par défaut
            if (signature.EstParDefaut)
            {
                var newDefault = await _context.Signatures
                    .Where(s => s.UserId == userId)
                    .FirstOrDefaultAsync();

                if (newDefault != null)
                {
                    newDefault.EstParDefaut = true;
                    _context.Update(newDefault);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Signature/SetDefault/5 - Définir comme signature par défaut
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDefault(int id)
        {
            var userId = _userManager.GetUserId(User);
            
            // Désactiver toutes les signatures par défaut
            var allSignatures = await _context.Signatures
                .Where(s => s.UserId == userId)
                .ToListAsync();

            foreach (var signature in allSignatures)
            {
                signature.EstParDefaut = false;
            }

            // Activer la signature sélectionnée
            var selectedSignature = allSignatures.FirstOrDefault(s => s.Id == id);
            if (selectedSignature != null)
            {
                selectedSignature.EstParDefaut = true;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SignatureExists(int id)
        {
            return _context.Signatures.Any(e => e.Id == id);
        }
    }
}