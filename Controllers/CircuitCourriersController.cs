
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
    public class CircuitCourriersController : Controller
    {
        private readonly ProjetIdDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<CircuitCourriersController> _logger;

        public CircuitCourriersController(
            ProjetIdDbContext context, 
            UserManager<User> userManager,
            ILogger<CircuitCourriersController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: CircuitCourriers
        public async Task<IActionResult> Index(string searchTerm = "", int? typeCircuitFilter = null, int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.CircuitCourriers!
                    .Include(c => c.Service)
                    .Include(c => c.UsersCircuit)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(c => 
                        c.NomActions.Contains(searchTerm) || 
                        (c.Commentaire != null && c.Commentaire.Contains(searchTerm)) ||
                        (c.Service != null && c.Service.Nom.Contains(searchTerm)) ||
                        (c.UsersCircuit != null && (c.UsersCircuit.Nom.Contains(searchTerm) || c.UsersCircuit.Prenom.Contains(searchTerm))));
                }

                if (typeCircuitFilter.HasValue)
                {
                    query = query.Where(c => c.TypeCircuit == typeCircuitFilter.Value);
                }

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var circuits = await query
                    .OrderBy(c => c.TypeCircuit)
                    .ThenBy(c => c.OrdreExecution)
                    .ThenBy(c => c.NomActions)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new CircuitCourriersViewModel
                    {
                        Id = c.Id,
                        NomActions = c.NomActions,
                        ServiceId = c.ServiceId,
                        ServiceNom = c.Service != null ? c.Service.Nom : null,
                        UsersCircuitId = c.UsersCircuitId,
                        UsersCircuitNom = c.UsersCircuit != null ? $"{c.UsersCircuit.Nom} {c.UsersCircuit.Prenom}" : null,
                        Commentaire = c.Commentaire,
                        TypeCircuit = c.TypeCircuit,
                        TypeCircuitLibelle = c.TypeCircuit == 1 ? "Courrier Entrant" : "Courrier Sortant",
                        OrdreExecution = c.OrdreExecution,
                        Obligatoire = c.Obligatoire,
                        Actif = c.Actif,
                        CreatedAt = c.CreatedAtUtc
                    })
                    .ToListAsync();

                var viewModel = new CircuitCourriersIndexViewModel
                {
                    CircuitsCourriers = circuits,
                    SearchTerm = searchTerm,
                    TypeCircuitFilter = typeCircuitFilter,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des circuits de courriers");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la récupération des circuits de courriers.";
                return View(new CircuitCourriersIndexViewModel());
            }
        }

        // GET: CircuitCourriers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var circuit = await _context.CircuitCourriers!
                    .Include(c => c.Service)
                    .Include(c => c.UsersCircuit)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (circuit == null)
                {
                    return NotFound();
                }

                var viewModel = new CircuitCourriersDetailsViewModel
                {
                    Id = circuit.Id,
                    NomActions = circuit.NomActions,
                    ServiceId = circuit.ServiceId,
                    ServiceNom = circuit.Service != null ? circuit.Service.Nom : null,
                    UsersCircuitId = circuit.UsersCircuitId,
                    UsersCircuitNom = circuit.UsersCircuit != null ? $"{circuit.UsersCircuit.Nom} {circuit.UsersCircuit.Prenom}" : null,
                    Commentaire = circuit.Commentaire,
                    TypeCircuit = circuit.TypeCircuit,
                    TypeCircuitLibelle = circuit.TypeCircuit == 1 ? "Courrier Entrant" : "Courrier Sortant",
                    OrdreExecution = circuit.OrdreExecution,
                    Obligatoire = circuit.Obligatoire,
                    Actif = circuit.Actif,
                    CreatedAt = circuit.CreatedAtUtc,
                    UpdatedAt = circuit.UpdatedAtUtc
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du circuit de courriers {Id}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la récupération du circuit de courriers.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: CircuitCourriers/Create
        public async Task<IActionResult> Create()
        {
            var model = new CreateCircuitCourriersViewModel();
            await PopulateDropdowns(model);
            return View(model);
        }

        // POST: CircuitCourriers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCircuitCourriersViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var circuit = new CircuitCourrier
                    {
                        NomActions = model.NomActions,
                        ServiceId = model.ServiceId,
                        UsersCircuitId = model.UsersCircuitId,
                        Commentaire = model.Commentaire,
                        TypeCircuit = model.TypeCircuit,
                        OrdreExecution = model.OrdreExecution,
                        Obligatoire = model.Obligatoire,
                        Actif = model.Actif
                    };

                    _context.CircuitCourriers!.Add(circuit);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Le circuit de courriers a été créé avec succès.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de la création du circuit de courriers");
                    TempData["ErrorMessage"] = "Une erreur est survenue lors de la création du circuit de courriers.";
                }
            }

            await PopulateDropdowns(model);
            return View(model);
        }

        // GET: CircuitCourriers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var circuit = await _context.CircuitCourriers!.FindAsync(id);
                if (circuit == null)
                {
                    return NotFound();
                }

                var viewModel = new EditCircuitCourriersViewModel
                {
                    Id = circuit.Id,
                    NomActions = circuit.NomActions,
                    ServiceId = circuit.ServiceId,
                    UsersCircuitId = circuit.UsersCircuitId,
                    Commentaire = circuit.Commentaire,
                    TypeCircuit = circuit.TypeCircuit,
                    OrdreExecution = circuit.OrdreExecution,
                    Obligatoire = circuit.Obligatoire,
                    Actif = circuit.Actif
                };

                await PopulateDropdowns(viewModel);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du circuit de courriers {Id}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la récupération du circuit de courriers.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: CircuitCourriers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditCircuitCourriersViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var circuit = await _context.CircuitCourriers!.FindAsync(id);
                    if (circuit == null)
                    {
                        return NotFound();
                    }

                    circuit.NomActions = model.NomActions;
                    circuit.ServiceId = model.ServiceId;
                    circuit.UsersCircuitId = model.UsersCircuitId;
                    circuit.Commentaire = model.Commentaire;
                    circuit.TypeCircuit = model.TypeCircuit;
                    circuit.OrdreExecution = model.OrdreExecution;
                    circuit.Obligatoire = model.Obligatoire;
                    circuit.Actif = model.Actif;

                    _context.Update(circuit);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Le circuit de courriers a été modifié avec succès.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CircuitCourriersExistsAsync(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de la modification du circuit de courriers {Id}", id);
                    TempData["ErrorMessage"] = "Une erreur est survenue lors de la modification du circuit de courriers.";
                }
            }

            await PopulateDropdowns(model);
            return View(model);
        }

        // GET: CircuitCourriers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var circuit = await _context.CircuitCourriers!
                    .Include(c => c.Service)
                    .Include(c => c.UsersCircuit)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (circuit == null)
                {
                    return NotFound();
                }

                var viewModel = new CircuitCourriersDetailsViewModel
                {
                    Id = circuit.Id,
                    NomActions = circuit.NomActions,
                    ServiceId = circuit.ServiceId,
                    ServiceNom = circuit.Service != null ? circuit.Service.Nom : null,
                    UsersCircuitId = circuit.UsersCircuitId,
                    UsersCircuitNom = circuit.UsersCircuit != null ? $"{circuit.UsersCircuit.Nom} {circuit.UsersCircuit.Prenom}" : null,
                    Commentaire = circuit.Commentaire,
                    TypeCircuit = circuit.TypeCircuit,
                    TypeCircuitLibelle = circuit.TypeCircuit == 1 ? "Courrier Entrant" : "Courrier Sortant",
                    OrdreExecution = circuit.OrdreExecution,
                    Obligatoire = circuit.Obligatoire,
                    Actif = circuit.Actif,
                    CreatedAt = circuit.CreatedAtUtc,
                    UpdatedAt = circuit.UpdatedAtUtc
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du circuit de courriers {Id}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la récupération du circuit de courriers.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: CircuitCourriers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var circuit = await _context.CircuitCourriers!.FindAsync(id);
                if (circuit != null)
                {
                    _context.CircuitCourriers!.Remove(circuit);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Le circuit de courriers a été supprimé avec succès.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Le circuit de courriers n'a pas été trouvé.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du circuit de courriers {Id}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la suppression du circuit de courriers.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns(CreateCircuitCourriersViewModel model)
        {
            // Services
            var services = await _context.Services!
                .Where(s => s.Actif)
                .OrderBy(s => s.Nom)
                .Select(s => new LookupOption { Id = s.Id, Nom = s.Nom  })
                .ToListAsync();
            model.AvailableServices = services;

            // Utilisateurs
            var users = await _userManager.Users
                .OrderBy(u => u.Nom)
                .ThenBy(u => u.Prenom)
                .Select(u => new UserLookupOption { Id = u.Id, Nom = $"{u.Nom} {u.Prenom}" })
                .ToListAsync();
            model.AvailableUsers = users;
        }

        private async Task PopulateDropdowns(EditCircuitCourriersViewModel model)
        {
            // Services
            var services = await _context.Services!
                .Where(s => s.Actif)
                .OrderBy(s => s.Nom)
                .Select(s => new LookupOption { Id = s.Id, Nom = s.Nom })
                .ToListAsync();
            model.AvailableServices = services;

            // Utilisateurs
            var users = await _userManager.Users
                .OrderBy(u => u.Nom)
                .ThenBy(u => u.Prenom)
                .Select(u => new UserLookupOption { Id = u.Id, Nom = $"{u.Nom} {u.Prenom}" })
                .ToListAsync();
            model.AvailableUsers = users;
        }

        private async Task<bool> CircuitCourriersExistsAsync(int id)
        {
            return await _context.CircuitCourriers!.AnyAsync(e => e.Id == id);
        }

        // GET: CircuitCourriers/GetUsersByRole
        [HttpGet]
        public async Task<IActionResult> GetUsersByRole(string roleName)
        {
            try
            {
                if (string.IsNullOrEmpty(roleName))
                {
                    return Json(new { success = false, message = "Nom de rôle requis" });
                }

                var users = await _userManager.GetUsersInRoleAsync(roleName);
                
                var userOptions = users
                    .OrderBy(u => u.Nom)
                    .ThenBy(u => u.Prenom)
                    .Select(u => new UserLookupOption 
                    { 
                        Id = u.Id, 
                        Nom = $"{u.Nom} {u.Prenom}" 
                    })
                    .ToList();

                return Json(new { success = true, users = userOptions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des utilisateurs pour le rôle {RoleName}", roleName);
                return Json(new { success = false, message = "Erreur lors de la récupération des utilisateurs" });
            }
        }
    }

    // ViewModels pour CircuitCourriers
    public class CircuitCourriersViewModel
    {
        public int Id { get; set; }
        public string NomActions { get; set; } = string.Empty;
        public int? ServiceId { get; set; }
        public string? ServiceNom { get; set; }
        public string? UsersCircuitId { get; set; }
        public string? UsersCircuitNom { get; set; }
        public string? Commentaire { get; set; }
        public int TypeCircuit { get; set; }
        public string TypeCircuitLibelle { get; set; } = string.Empty;
        public int OrdreExecution { get; set; }
        public bool Obligatoire { get; set; }
        public bool Actif { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CircuitCourriersIndexViewModel
    {
        public List<CircuitCourriersViewModel> CircuitsCourriers { get; set; } = new();
        public string SearchTerm { get; set; } = string.Empty;
        public int? TypeCircuitFilter { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public class CircuitCourriersDetailsViewModel
    {
        public int Id { get; set; }
        public string NomActions { get; set; } = string.Empty;
        public int? ServiceId { get; set; }
        public string? ServiceNom { get; set; }
        public string? UsersCircuitId { get; set; }
        public string? UsersCircuitNom { get; set; }
        public string? Commentaire { get; set; }
        public int TypeCircuit { get; set; }
        public string TypeCircuitLibelle { get; set; } = string.Empty;
        public int OrdreExecution { get; set; }
        public bool Obligatoire { get; set; }
        public bool Actif { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateCircuitCourriersViewModel
    {
        public string NomActions { get; set; } = string.Empty;
        public int? ServiceId { get; set; }
        public string? UsersCircuitId { get; set; }
        public string? Commentaire { get; set; }
        public int TypeCircuit { get; set; }
        public int OrdreExecution { get; set; }
        public bool Obligatoire { get; set; }
        public bool Actif { get; set; } = true;
        public List<LookupOption> AvailableServices { get; set; } = new();
        public List<UserLookupOption> AvailableUsers { get; set; } = new();
    }

    public class EditCircuitCourriersViewModel
    {
        public int Id { get; set; }
        public string NomActions { get; set; } = string.Empty;
        public int? ServiceId { get; set; }
        public string? UsersCircuitId { get; set; }
        public string? Commentaire { get; set; }
        public int TypeCircuit { get; set; }
        public int OrdreExecution { get; set; }
        public bool Obligatoire { get; set; }
        public bool Actif { get; set; } = true;
        public List<LookupOption> AvailableServices { get; set; } = new();
        public List<UserLookupOption> AvailableUsers { get; set; } = new();
    }

    public class LookupOption
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
    }

    public class UserLookupOption
    {
        public string Id { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
    }
}