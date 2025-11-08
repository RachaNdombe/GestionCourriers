using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JconsultGC.Data;
using JconsultGC.Models;
using Microsoft.AspNetCore.Authorization;

namespace JconsultGC.Controllers
{
    [Authorize]
    public class ModesEnvoiController : Controller
    {
        private readonly ProjetIdDbContext _context;

        public ModesEnvoiController(ProjetIdDbContext context)
        {
            _context = context;
        }

        // GET: ModesEnvoi
        public async Task<IActionResult> Index()
        {
            return View(await _context.ModeEnvois!.ToListAsync());
        }

        // GET: ModesEnvoi/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ModesEnvoi/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ModeEnvoi modeEnvoi)
        {
            if (ModelState.IsValid)
            {
                _context.Add(modeEnvoi);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(modeEnvoi);
        }

        // POST: ModesEnvoi/CreateAjax
        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] ModeEnvoiCreateRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Libelle))
                {
                    return Json(new { success = false, message = "Le code et le libellé sont obligatoires" });
                }

                // Vérifier si le code existe déjà
                var existing = await _context.ModeEnvois!
                    .FirstOrDefaultAsync(m => m.Code == request.Code);
                
                if (existing != null)
                {
                    return Json(new { success = false, message = "Un mode d'envoi avec ce code existe déjà" });
                }

                var modeEnvoi = new ModeEnvoi
                {
                    //Nom = request.Nom,
                    Code = request.Code,
                    Libelle = request.Libelle,
                    Description = request.Description,
                    Actif = request.Actif,
                    Ordre = request.Ordre
                };

                _context.ModeEnvois!.Add(modeEnvoi);
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    id = modeEnvoi.Id, 
                    libelle = modeEnvoi.Libelle,
                    message = "Mode d'envoi créé avec succès" 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur: {ex.Message}" });
            }
        }

        // GET: ModesEnvoi/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var modeEnvoi = await _context.ModeEnvois!.FindAsync(id);
            if (modeEnvoi == null)
            {
                return NotFound();
            }
            return View(modeEnvoi);
        }

        // POST: ModesEnvoi/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ModeEnvoi modeEnvoi)
        {
            if (id != modeEnvoi.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(modeEnvoi);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ModeEnvoiExists(modeEnvoi.Id))
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
            return View(modeEnvoi);
        }

        // GET: ModesEnvoi/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var modeEnvoi = await _context.ModeEnvois!
                .FirstOrDefaultAsync(m => m.Id == id);
            if (modeEnvoi == null)
            {
                return NotFound();
            }

            return View(modeEnvoi);
        }

        // POST: ModesEnvoi/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var modeEnvoi = await _context.ModeEnvois!.FindAsync(id);
            if (modeEnvoi != null)
            {
                _context.ModeEnvois!.Remove(modeEnvoi);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ModeEnvoiExists(int id)
        {
            return _context.ModeEnvois!.Any(e => e.Id == id);
        }
    }

    public class ModeEnvoiCreateRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Libelle { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Actif { get; set; } = true;
        public int Ordre { get; set; } = 0;
    }
}