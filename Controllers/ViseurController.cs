using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using JconsultGC.Data;
using JconsultGC.Models;
using Microsoft.Extensions.Logging;

namespace JconsultGC.Controllers
{
    [Authorize]
    public class ViseurController : Controller
    {
        private readonly ProjetIdDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ViseurController> _logger;

        public ViseurController(
            ProjetIdDbContext context,
            UserManager<User> userManager,
            ILogger<ViseurController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Viseur/Dashboard - Tableau de bord du viseur
        [Authorize(Roles = "Viseur")]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var viewModel = new ViseurDashboardViewModel
            {
                // Nombre de courriers en attente de validation
                CourriersAValiderCount = await _context.Courriers!
                    .Where(c => c.Statut == "A valider")
                    .CountAsync(),

                // Nombre de courriers validés aujourd'hui
                CourriersValidesAujourdhui = await _context.CourrierHistories!
                    .Where(h => h.Action == "Validé" && 
                                h.Timestamp.Date == DateTime.Today)
                    .CountAsync(),

                // Nombre de courriers rejetés aujourd'hui
                CourriersRejetesAujourdhui = await _context.CourrierHistories!
                    .Where(h => h.Action == "Rejeté" && 
                                h.Timestamp.Date == DateTime.Today)
                    .CountAsync(),

                // Actions récentes du viseur
                RecentActions = await _context.CourrierHistories!
                    .Where(h => h.UserId == user.Id && 
                                h.Timestamp.Date == DateTime.Today)
                    .OrderByDescending(h => h.Timestamp)
                    .Take(10)
                    .ToListAsync(),

                // Courriers en attente de validation
                CourriersAValider = await _context.Courriers!
                    .Include(c => c.CategorieCourrier)
                    .Include(c => c.Correspondant)
                    .Include(c => c.CreatedBy)
                    .Where(c => c.Statut == "A valider")
                    .OrderByDescending(c => c.DateEnregistrement)
                    .Take(5)
                    .ToListAsync()
            };

            return View(viewModel);
        }
    }

    public class ViseurDashboardViewModel
    {
        public int CourriersAValiderCount { get; set; }
        public int CourriersValidesAujourdhui { get; set; }
        public int CourriersRejetesAujourdhui { get; set; }
        public List<CourrierHistory> RecentActions { get; set; } = new List<CourrierHistory>();
        public List<Courrier> CourriersAValider { get; set; } = new List<Courrier>();
    }
}