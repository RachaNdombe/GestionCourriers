using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JconsultGC.Models;
using JconsultGC.Data;
using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Controllers
{
    [Authorize(Roles = "Administrateur,SupervUser")]
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ProjetIdDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ProjetIdDbContext context,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        // GET: Users
        public async Task<IActionResult> Index(string searchTerm = "", string roleFilter = "", string statusFilter = "", int page = 1, int pageSize = 10)
        {
            var query = _userManager.Users
                .Include(u => u.Service)
                .AsQueryable();

            // Filtre par terme de recherche
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => 
                    u.Nom.Contains(searchTerm) || 
                    u.Prenom.Contains(searchTerm) || 
                    u.Email.Contains(searchTerm) ||
                    u.UserName.Contains(searchTerm));
            }

            // Filtre par statut
            if (!string.IsNullOrEmpty(statusFilter))
            {
                switch (statusFilter)
                {
                    case "active":
                        query = query.Where(u => u.IsActive);
                        break;
                    case "inactive":
                        query = query.Where(u => !u.IsActive);
                        break;
                    case "confirmed":
                        query = query.Where(u => u.EmailConfirmed);
                        break;
                    case "unconfirmed":
                        query = query.Where(u => !u.EmailConfirmed);
                        break;
                }
            }

            // Pagination
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userViewModels = new List<UserViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Nom = user.Nom,
                    Prenom = user.Prenom,
                    Phone = user.PhoneNumber,
                    Service = user.Service?.Nom,
                    Roles = roles.ToList(),
                    EmailConfirmed = user.EmailConfirmed,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt
                });
            }

            // Filtre par rôle (après récupération des rôles)
            if (!string.IsNullOrEmpty(roleFilter))
            {
                userViewModels = userViewModels.Where(u => u.Roles.Contains(roleFilter)).ToList();
            }

            var viewModel = new UserIndexViewModel
            {
                Users = userViewModels,
                SearchTerm = searchTerm,
                RoleFilter = roleFilter,
                StatusFilter = statusFilter,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                AvailableRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync()
            };

            return View(viewModel);
        }

        // GET: Users/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateUserViewModel
            {
                AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync(),
                AllServices = await _context.Services.Select(s => new ServiceOption
                {
                    Id = s.Id,
                    Nom = s.Nom
                }).ToListAsync()
            };
            return View(viewModel);
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Nom = model.Nom,
                    Prenom = model.Prenom,
                    PhoneNumber = model.Phone,
                    ServiceId = model.ServiceId,
                    EmailConfirmed = model.EmailConfirmed,
                    IsActive = true
                };

                // Générer un mot de passe temporaire
                var temporaryPassword = "TempPassword123!"; // À remplacer par un générateur de mot de passe

                var result = await _userManager.CreateAsync(user, temporaryPassword);

                if (result.Succeeded)
                {
                    if (model.SelectedRoles != null && model.SelectedRoles.Any())
                    {
                        await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                    }

                    TempData["SuccessMessage"] = $"Utilisateur créé avec succès. Mot de passe temporaire: {temporaryPassword}";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            model.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            model.AllServices = await _context.Services.Select(s => new ServiceOption
            {
                Id = s.Id,
                Nom = s.Nom
            }).ToListAsync();
            return View(model);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.Users
                .Include(u => u.Service)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            var viewModel = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Nom = user.Nom,
                Prenom = user.Prenom,
                Phone = user.PhoneNumber,
                ServiceId = user.ServiceId ?? 0,
                SelectedRoles = roles.ToList(),
                AllRoles = allRoles,
                EmailConfirmed = user.EmailConfirmed,
                IsActive = user.IsActive
            };

            return View(viewModel);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                user.UserName = model.Email;
                user.Email = model.Email;
                user.Nom = model.Nom;
                user.Prenom = model.Prenom;
                user.PhoneNumber = model.Phone;
                user.ServiceId = model.ServiceId;
                user.EmailConfirmed = model.EmailConfirmed;
                user.IsActive = model.IsActive;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // Mettre à jour les rôles
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    var rolesToRemove = currentRoles.Except(model.SelectedRoles ?? new List<string>());
                    var rolesToAdd = (model.SelectedRoles ?? new List<string>()).Except(currentRoles);

                    if (rolesToRemove.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    }

                    if (rolesToAdd.Any())
                    {
                        await _userManager.AddToRolesAsync(user, rolesToAdd);
                    }

                    TempData["SuccessMessage"] = "Utilisateur modifié avec succès.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            model.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(model);
        }

        // POST: Users/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Utilisateur supprimé avec succès.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Erreur lors de la suppression de l'utilisateur.";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Users/ResetPassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var newPassword = "NewPassword123!"; // À remplacer par un générateur de mot de passe
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
                
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"Mot de passe réinitialisé avec succès. Nouveau mot de passe: {newPassword}";
                }
                else
                {
                    TempData["ErrorMessage"] = "Erreur lors de la réinitialisation du mot de passe.";
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }

    // ViewModels
    public class UserIndexViewModel
    {
        public List<UserViewModel> Users { get; set; } = new();
        public string SearchTerm { get; set; } = string.Empty;
        public string RoleFilter { get; set; } = string.Empty;
        public string StatusFilter { get; set; } = string.Empty;
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<string> AvailableRoles { get; set; } = new();
        
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int StartPage => Math.Max(1, CurrentPage - 2);
        public int EndPage => Math.Min(TotalPages, CurrentPage + 2);
    }

    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Service { get; set; }
        public List<string> Roles { get; set; } = new();
        public bool EmailConfirmed { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "L'email est requis.")]
        [EmailAddress(ErrorMessage = "L'email n'est pas valide.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est requis.")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est requis.")]
        public string Prenom { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Le numéro de téléphone n'est pas valide.")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Le service est requis")]
        public int ServiceId { get; set; }

        public bool EmailConfirmed { get; set; } = true;
        public List<string> SelectedRoles { get; set; } = new();
        public List<string> AllRoles { get; set; } = new();
        public List<ServiceOption> AllServices { get; set; } = new();
    }

    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est requis.")]
        [EmailAddress(ErrorMessage = "L'email n'est pas valide.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est requis.")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est requis.")]
        public string Prenom { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Le numéro de téléphone n'est pas valide.")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Le service est requis")]
        public int ServiceId { get; set; }
        public List<string> SelectedRoles { get; set; } = new();
        public List<string> AllRoles { get; set; } = new();
        public bool EmailConfirmed { get; set; }
        public bool IsActive { get; set; }
    }


}