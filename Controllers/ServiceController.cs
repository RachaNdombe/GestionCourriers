using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using JconsultGC.Data;
using JconsultGC.Models;
using JconsultGC.ViewModels;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Controllers
{
    [Authorize]
    public class ServiceController : Controller
    {
        private readonly ProjetIdDbContext _context;
        private readonly ILogger<ServiceController> _logger;

        public ServiceController(
            ProjetIdDbContext context,
            ILogger<ServiceController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ACTIONS PRINCIPALES
       

        // GET: Service
        public async Task<IActionResult> Index(string searchTerm = "", string statusFilter = "", int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.Services!.AsQueryable();

                // Filtre par terme de recherche
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(s => 
                        s.Nom!.Contains(searchTerm) || 
                        (s.Description != null && s.Description.Contains(searchTerm)) ||
                        s.Code!.Contains(searchTerm));
                }

                // Filtre par statut
                if (!string.IsNullOrEmpty(statusFilter))
                {
                    switch (statusFilter)
                    {
                        case "active":
                            query = query.Where(s => s.Actif);
                            break;
                        case "inactive":
                            query = query.Where(s => !s.Actif);
                            break;
                    }
                }

                // Pagination
                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                
                var services = await query
                    .OrderBy(s => s.Nom)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var viewModel = new ServiceIndexViewModel
                {
                    Services = services.Select(s => new ServiceViewModel
                    {
                        Id = s.Id,
                        Nom = s.Nom ?? string.Empty,
                        Code = s.Code ?? string.Empty,
                        Description = s.Description ?? string.Empty,
                        IsActive = s.Actif,
                        CreatedAt = s.CreatedAtUtc,
                        UpdatedAt = s.UpdatedAtUtc
                    }).ToList(),
                    SearchTerm = searchTerm,
                    StatusFilter = statusFilter,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des services");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la récupération des services.";
                return View(new ServiceIndexViewModel());
            }
        }

        // GET: Service/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var service = await _context.Services!.FindAsync(id);
                if (service == null)
                {
                    return NotFound();
                }

                var viewModel = new ServiceDetailsViewModel
                {
                    Id = service.Id,
                    Nom = service.Nom ?? string.Empty,
                    Code = service.Code ?? string.Empty,
                    Description = service.Description ?? string.Empty,
                    IsActive = service.Actif,
                    CreatedAt = service.CreatedAtUtc,
                    UpdatedAt = service.UpdatedAtUtc
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du service {Id}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la récupération du service.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =============================================
        // ACTIONS DE CRÉATION
        // =============================================

        // GET: Service/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Service/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateServiceViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var service = new Service
                    {
                        Nom = model.Nom,
                        Code = model.Code,
                        Description = model.Description,
                        Actif = model.IsActive,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    };

                    _context.Services!.Add(service);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Service créé avec succès.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de la création du service");
                    TempData["ErrorMessage"] = "Une erreur est survenue lors de la création du service.";
                }
            }

            return View(model);
        }

        // POST: Service/CreateAjax - Pour les requêtes AJAX
        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] CreateServiceViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var service = new Service
                    {
                        Nom = model.Nom,
                        Code = model.Code,
                        Description = model.Description,
                        Actif = model.IsActive,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    };

                    _context.Services!.Add(service);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, id = service.Id, message = "Service créé avec succès." });
                }

                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, errors = errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création du service via AJAX");
                return Json(new { success = false, message = "Erreur lors de la création du service." });
            }
        }

        // =============================================
        // ACTIONS DE MODIFICATION
        // =============================================

        // GET: Service/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var service = await _context.Services!.FindAsync(id);
                if (service == null)
                {
                    return NotFound();
                }

                var viewModel = new EditServiceViewModel
                {
                    Id = service.Id,
                    Nom = service.Nom,
                    Code = service.Code,
                    Description = service.Description,
                    IsActive = service.Actif
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du service {Id}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la récupération du service.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Service/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditServiceViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var service = await _context.Services!.FindAsync(id);
                    if (service == null)
                    {
                        return NotFound();
                    }

                    service.Nom = model.Nom;
                    service.Code = model.Code;
                    service.Description = model.Description;
                    service.Actif = model.IsActive;
                    service.UpdatedAtUtc = DateTime.UtcNow;

                    _context.Update(service);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Service modifié avec succès.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ServiceExistsAsync(id))
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
                    _logger.LogError(ex, "Erreur lors de la modification du service {Id}", id);
                    TempData["ErrorMessage"] = "Une erreur est survenue lors de la modification du service.";
                }
            }

            return View(model);
        }

        // =============================================
        // ACTIONS DE SUPPRESSION
        // =============================================

        // GET: Service/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var service = await _context.Services!.FindAsync(id);
                if (service == null)
                {
                    return NotFound();
                }

                var viewModel = new ServiceViewModel
                {
                    Id = service.Id,
                    Nom = service.Nom ?? string.Empty,
                    Code = service.Code ?? string.Empty,
                    Description = service.Description ?? string.Empty,
                    IsActive = service.Actif,
                    CreatedAt = service.CreatedAtUtc,
                    UpdatedAt = service.UpdatedAtUtc
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du service {Id}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la récupération du service.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Service/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var service = await _context.Services!.FindAsync(id);
                if (service != null)
                {
                    _context.Services!.Remove(service);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Service supprimé avec succès.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Le service n'a pas été trouvé.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du service {Id}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la suppression du service.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =============================================
        // ACTIONS DE GESTION DE STATUT
        // =============================================

        // POST: Service/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var service = await _context.Services!.FindAsync(id);
                if (service != null)
                {
                    service.Actif = !service.Actif;
                    service.UpdatedAtUtc = DateTime.UtcNow;
                    
                    _context.Update(service);
                    await _context.SaveChangesAsync();

                    var status = service.Actif ? "activé" : "désactivé";
                    TempData["SuccessMessage"] = $"Service {status} avec succès.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Le service n'a pas été trouvé.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du changement de statut du service {Id}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors du changement de statut du service.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =============================================
        // MÉTHODES PRIVÉES
        // =============================================

        private async Task<bool> ServiceExistsAsync(int id)
        {
            return await _context.Services!.AnyAsync(e => e.Id == id);
        }
    }
}

// =============================================
// VIEWMODELS
// =============================================

namespace JconsultGC.ViewModels
{
    public class ServiceIndexViewModel
    {
        public List<ServiceViewModel> Services { get; set; } = new();
        public string SearchTerm { get; set; } = string.Empty;
        public string StatusFilter { get; set; } = string.Empty;
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int StartPage => Math.Max(1, CurrentPage - 2);
        public int EndPage => Math.Min(TotalPages, CurrentPage + 2);
    }

    public class ServiceViewModel
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ServiceDetailsViewModel : ServiceViewModel
    {
        // Peut être étendu avec des propriétés spécifiques aux détails
    }

    public class CreateServiceViewModel
    {
        [Required(ErrorMessage = "Le nom est requis.")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères.")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le code est requis.")]
        [StringLength(20, ErrorMessage = "Le code ne peut pas dépasser 20 caractères.")]
        public string Code { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La description ne peut pas dépasser 500 caractères.")]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class EditServiceViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom est requis.")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères.")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le code est requis.")]
        [StringLength(20, ErrorMessage = "Le code ne peut pas dépasser 20 caractères.")]
        public string Code { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La description ne peut pas dépasser 500 caractères.")]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
