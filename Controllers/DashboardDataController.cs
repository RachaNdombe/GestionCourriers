using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JconsultGC.Models;
using JconsultGC.Data;
using System.Text.Json;

namespace JconsultGC.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardDataController : ControllerBase
{
    private readonly ProjetIdDbContext _context;
    private readonly UserManager<User> _userManager;

    public DashboardDataController(ProjetIdDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboardData()
    {
        var userId = _userManager.GetUserId(User);
        var model = new IndexateurDashboardViewModel();

        // Get recent actions by this indexateur
        model.RecentActions = await _context.CourrierHistories!
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.Timestamp)
            .Take(10)
            .ToListAsync();

        // Get courriers created by this indexateur
        var courriers = await _context.Courriers
            .Include(c => c.CategorieCourrier)
            .Where(c => c.CreatedById == userId)
            .ToListAsync();

        model.RecentCourriers = courriers
            .OrderByDescending(c => c.DateEnregistrement)
            .Take(5);

        // Get courriers waiting for validation
        model.CourriersAValider = await _context.Courriers
            .Include(c => c.CategorieCourrier)
            .Where(c => c.Statut == "A valider" && c.CreatedById == userId)
            .OrderByDescending(c => c.DateEnregistrement)
            .Take(5)
            .ToListAsync();

        // Calculate statistics
        model.TotalCourriersIndexes = courriers.Count;
        model.CourriersTresPrioritaires = courriers.Count(c => c.Priorite == PrioriteLevel.Urgent);
        model.CourriersEnAttente = courriers.Count(c => c.Statut == "A valider");

        // Distribution by category
        model.CourriersByCategory = courriers
            .Where(c => c.CategorieCourrier != null)
            .GroupBy(c => c.CategorieCourrier!.Nom!)
            .ToDictionary(g => g.Key, g => g.Count());

        // Distribution by status
        model.CourriersByStatus = courriers
            .Where(c => c.Statut != null)
            .GroupBy(c => c.Statut!)
            .ToDictionary(g => g.Key, g => g.Count());

        return Ok(model);
    }
}