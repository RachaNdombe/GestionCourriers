using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JconsultGC.Data;
using JconsultGC.Models;

namespace JconsultGC.Controllers;

[Authorize]
public class CourriersController : Controller
{
    private readonly ProjetIdDbContext _context;
    private readonly UserManager<User> _userManager;

    public CourriersController(ProjetIdDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Courriers
    public async Task<IActionResult> Index()
    {
        var courriers = await _context.Courriers
            .Include(c => c.CategorieCourrier)
            .Include(c => c.Correspondant)
            .Include(c => c.Service)
            .Include(c => c.NatureCourrier)
            .Include(c => c.DossierClassement)
            .Include(c => c.TypeDossier)
            .Include(c => c.ModeEnvoi)
            .Include(c => c.CreatedBy)
            .Include(c => c.CreatedBy)
            .ToListAsync();

        return View(courriers);
    }

    // GET: Courriers/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var courrier = await _context.Courriers
            .Include(c => c.CategorieCourrier)
            .Include(c => c.Correspondant)
            .Include(c => c.Service)
            .Include(c => c.NatureCourrier)
            .Include(c => c.DossierClassement)
            .Include(c => c.TypeDossier)
            .Include(c => c.ModeEnvoi)
            .Include(c => c.CreatedBy)
            .Include(c => c.CreatedBy)
            .Include(c => c.Documents)
            .Include(c => c.Annotations)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (courrier == null)
        {
            return NotFound();
        }

        return View(courrier);
    }

    // GET: Courriers/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _context.CategorieCourriers.ToListAsync();
        ViewBag.Correspondants = await _context.Correspondants.ToListAsync();
        ViewBag.Services = await _context.Services.ToListAsync();
        ViewBag.Natures = await _context.NatureCourriers.ToListAsync();
        ViewBag.DossiersClassement = await _context.DossierClassements.ToListAsync();
        ViewBag.TypesDossier = await _context.TypeDossiers.ToListAsync();
        ViewBag.ModesEnvoi = await _context.ModeEnvois.ToListAsync();
        ViewBag.Users = await _context.Users.ToListAsync();

        return View();
    }

    // POST: Courriers/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Courrier courrier)
    {
        if (ModelState.IsValid)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            courrier.CreatedById = currentUser?.Id;
            courrier.DateEnregistrement = DateTime.UtcNow;
            courrier.Statut = "Brouillon";

            _context.Add(courrier);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Categories = await _context.CategorieCourriers.ToListAsync();
        ViewBag.Correspondants = await _context.Correspondants.ToListAsync();
        ViewBag.Services = await _context.Services.ToListAsync();
        ViewBag.Natures = await _context.NatureCourriers.ToListAsync();
        ViewBag.DossiersClassement = await _context.DossierClassements.ToListAsync();
        ViewBag.TypesDossier = await _context.TypeDossiers.ToListAsync();
        ViewBag.ModesEnvoi = await _context.ModeEnvois.ToListAsync();

        return View(courrier);
    }

    // GET: Courriers/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var courrier = await _context.Courriers.FindAsync(id);
        if (courrier == null)
        {
            return NotFound();
        }

        ViewBag.Categories = await _context.CategorieCourriers.ToListAsync();
        ViewBag.Correspondants = await _context.Correspondants.ToListAsync();
        ViewBag.Services = await _context.Services.ToListAsync();
        ViewBag.Natures = await _context.NatureCourriers.ToListAsync();
        ViewBag.DossiersClassement = await _context.DossierClassements.ToListAsync();
        ViewBag.TypesDossier = await _context.TypeDossiers.ToListAsync();
        ViewBag.ModesEnvoi = await _context.ModeEnvois.ToListAsync();

        return View(courrier);
    }

    // POST: Courriers/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Courrier courrier)
    {
        if (id != courrier.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(courrier);
                await _context.SaveChangesAsync();
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
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Categories = await _context.CategorieCourriers.ToListAsync();
        ViewBag.Correspondants = await _context.Correspondants.ToListAsync();
        ViewBag.Services = await _context.Services.ToListAsync();
        ViewBag.Natures = await _context.NatureCourriers.ToListAsync();
        ViewBag.DossiersClassement = await _context.DossierClassements.ToListAsync();
        ViewBag.TypesDossier = await _context.TypeDossiers.ToListAsync();
        ViewBag.ModesEnvoi = await _context.ModeEnvois.ToListAsync();

        return View(courrier);
    }

    // GET: Courriers/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var courrier = await _context.Courriers
            .Include(c => c.CategorieCourrier)
            .Include(c => c.Correspondant)
            .Include(c => c.Service)
            .Include(c => c.NatureCourrier)
            .Include(c => c.DossierClassement)
            .Include(c => c.TypeDossier)
            .Include(c => c.ModeEnvoi)
            .Include(c => c.CreatedBy)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (courrier == null)
        {
            return NotFound();
        }

        return View(courrier);
    }

    // POST: Courriers/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var courrier = await _context.Courriers.FindAsync(id);
        if (courrier != null)
        {
            _context.Courriers.Remove(courrier);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CourrierExists(int id)
    {
        return _context.Courriers.Any(e => e.Id == id);
    }
}
