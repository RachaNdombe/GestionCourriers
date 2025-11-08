
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using JconsultGC.Data;
using JconsultGC.Models;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Controllers
{
    [Authorize]
    public class CourriersEntrantController : Controller
    {
        private readonly ProjetIdDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<CourriersEntrantController> _logger;

        public CourriersEntrantController(
            ProjetIdDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<CourriersEntrantController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // GET: CourriersEntrant/Create
        [Authorize(Roles = "Indexateur")]
        public async Task<IActionResult> Create()
        {
            // Générer automatiquement le numéro d'ordre
            var numeroOrdre = await GenererNumeroOrdreAsync();
            
            var viewModel = new CreateCourrierViewModel
            {
                OrdreNumero = numeroOrdre,
                DateReception = DateTime.Now,
                AvailableServices = await _context.Services!.Where(s => s.Actif).Select(s => new ServiceOption
                {
                    Id = s.Id,
                    Nom = s.Nom
                }).ToListAsync() ?? new List<ServiceOption>(),
                AvailableCategories = await _context.CategorieCourriers!.Select(c => new LookupOption
                {
                    Id = c.Id,
                    Nom = c.Nom
                }).ToListAsync() ?? new List<LookupOption>(),
                AvailableNatures = await _context.NatureCourriers!.Select(n => new LookupOption
                {
                    Id = n.Id,
                    Nom = n.Nom
                }).ToListAsync() ?? new List<LookupOption>(),
                AvailableModesEnvoi = await _context.ModeEnvois!.Select(m => new LookupOption
                {
                    Id = m.Id,
                    Nom = m.Libelle
                }).ToListAsync() ?? new List<LookupOption>(),
                AvailableCorrespondants = await _context.Correspondants!.Select(c => new LookupOption
                {
                    Id = c.Id,
                    Nom = c.Nom
                }).ToListAsync() ?? new List<LookupOption>(),
                AvailableDossiers = await _context.DossierClassements!.Select(d => new LookupOption
                {
                    Id = d.Id,
                    Nom = d.Titre
                }).ToListAsync() ?? new List<LookupOption>(),
                AvailableTypeDossiers = await _context.TypeDossiers!.Select(t => new LookupOption
                {
                    Id = t.Id,
                    Nom = t.Libelle ?? "Sans libellé"
                }).ToListAsync() ?? new List<LookupOption>()
            };

            return View(viewModel);
        }

        // POST: CourriersEntrant/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Indexateur")]
        public async Task<IActionResult> Create(CreateCourrierViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                // Vérifier si la combinaison OrdreNumero + RegistreNumero existe déjà
                var numeroRegistre = model.RegistreNumero ?? "";
                var existingCourrier = await _context.Courriers!
                    .FirstOrDefaultAsync(c => c.OrdreNumero == model.OrdreNumero && 
                                            c.RegistreNumero == numeroRegistre);

                if (existingCourrier != null)
                {
                    ModelState.AddModelError("OrdreNumero", 
                        $"Un courrier avec le numéro d'ordre '{model.OrdreNumero}' et le numéro de registre '{numeroRegistre}' existe déjà. Veuillez utiliser un numéro différent.");
                    
                    // Recharger les données pour la réaffichage du formulaire
                    await RechargerDonneesFormulaire(model);
                    return View(model);
                }

                // Obtenir un ServiceId valide
                int validServiceId;
                try
                {
                    validServiceId = await GetValidServiceIdAsync(model.ServiceId);
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("ServiceId", ex.Message);
                    await RechargerDonneesFormulaire(model);
                    return View(model);
                }

                var courrier = new Courrier
                {
                    OrdreNumero = model.OrdreNumero,
                    RegistreNumero = model.RegistreNumero ?? "",
                    ReferenceNumero = model.ReferenceNumero,
                    Objet = model.Objet,
                    DateReception = model.DateReception,
                    HeureRecu = string.IsNullOrEmpty(model.HeureRecu) ? null : TimeSpan.Parse(model.HeureRecu),
                    ServiceConcerne = model.ServiceConcerne,
                    UtilisateursEnCopie = model.UtilisateursEnCopie,
                    CorrespondantId = model.CorrespondantId,
                    ServiceId = model.ServiceId,
                    CategorieCourrierId = model.CategorieCourrierId,
                    ModeEnvoiId = model.ModeEnvoiId,
                    NatureCourrierId = model.NatureCourrierId,
                    TypeDossierId = model.TypeDossierId,
                    DossierClassementId = model.DossierClassementId,
                    Confidentialite = (ConfidentialiteLevel)model.Confidentialite,
                    Priorite = (PrioriteLevel)model.Priorite,
                    DateEnregistrement = DateTime.UtcNow,
                    Statut = "A valider",
                    CreatedById = user.Id
                };

                try
                {
                    _context.Courriers!.Add(courrier);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de la sauvegarde du courrier entrant");
                    ModelState.AddModelError("", "Une erreur inattendue s'est produite lors de la sauvegarde. Veuillez réessayer.");
                    
                    // Recharger les données pour la réaffichage du formulaire
                    await RechargerDonneesFormulaire(model);
                    return View(model);
                }

                // Gérer les pièces jointes
                if (Request.Form.Files.Any())
                {
                    foreach (var file in Request.Form.Files)
                    {
                        if (file.Length > 0)
                        {
                            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "courriers");
                            if (!Directory.Exists(uploadsDir))
                                Directory.CreateDirectory(uploadsDir);

                            var fileName = $"{courrier.Id}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                            var filePath = Path.Combine(uploadsDir, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            var document = new Document
                            {
                                FileName = file.FileName,
                                ContentType = file.ContentType,
                                FileSize = file.Length,
                                FilePath = Path.Combine("/uploads/courriers", fileName).Replace("\\", "/"),
                                CourrierId = courrier.Id,
                                UploadedAt = DateTime.UtcNow
                            };

                            _context.Documents!.Add(document);
                        }
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erreur lors de la sauvegarde des pièces jointes");
                        ModelState.AddModelError("", "Erreur lors de la sauvegarde des pièces jointes. Veuillez réessayer.");
                        await RechargerDonneesFormulaire(model);
                        return View(model);
                    }
                }

                // Ajouter l'historique
                var hist = new CourrierHistory
                {
                    CourrierId = courrier.Id,
                    UserId = user.Id,
                    Action = "Saisi",
                    Note = "Courrier entrant saisi et envoyé à validation",
                    Timestamp = DateTime.UtcNow
                };
                _context.CourrierHistories!.Add(hist);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Courrier entrant créé avec succès !";
                return RedirectToAction("Dashboard", "Indexateur");
            }

            // Recharger les données pour la réaffichage du formulaire
            await RechargerDonneesFormulaire(model);

            return View(model);
        }

        // POST: CourriersEntrant/GenerateNumeroOrdre
        [HttpPost]
        public async Task<IActionResult> GenerateNumeroOrdre()
        {
            try
            {
                var numeroOrdre = await GenererNumeroOrdreAsync();
                return Json(new { success = true, numeroOrdre = numeroOrdre });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du numéro d'ordre");
                return Json(new { success = false, message = "Erreur lors de la génération du numéro d'ordre" });
            }
        }

        // POST: CourriersEntrant/CheckNumeroOrdre
        [HttpPost]
        public async Task<IActionResult> CheckNumeroOrdre([FromBody] CheckNumeroOrdreRequest request)
        {
            try
            {
                var numeroRegistre = request.NumeroRegistre ?? "";
                var existingCourrier = await _context.Courriers!
                    .FirstOrDefaultAsync(c => c.OrdreNumero == request.NumeroOrdre && 
                                            c.RegistreNumero == numeroRegistre);

                if (existingCourrier != null)
                {
                    return Json(new { 
                        success = false, 
                        message = $"Un courrier avec le numéro d'ordre '{request.NumeroOrdre}' et le numéro de registre '{numeroRegistre}' existe déjà.",
                        isDuplicate = true
                    });
                }

                return Json(new { 
                    success = true, 
                    message = "Numéro d'ordre disponible.",
                    isDuplicate = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification du numéro d'ordre");
                return Json(new { success = false, message = "Erreur lors de la vérification du numéro d'ordre" });
            }
        }

        // POST: CourriersEntrant/UploadFile
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                _logger.LogInformation("UploadFile appelé avec fichier: {FileName}, taille: {FileSize}", 
                    file?.FileName ?? "null", file?.Length ?? 0);

                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Aucun fichier fourni");
                    return Json(new { success = false, message = "Aucun fichier sélectionné" });
                }

                // Vérifier la taille du fichier (10MB max)
                if (file.Length > 10 * 1024 * 1024)
                {
                    _logger.LogWarning("Fichier trop volumineux: {FileSize} bytes", file.Length);
                    return Json(new { success = false, message = "Le fichier est trop volumineux (max 10MB)" });
                }

                // Vérifier le type de fichier
                var allowedTypes = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".tiff" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedTypes.Contains(fileExtension))
                {
                    _logger.LogWarning("Type de fichier non autorisé: {FileExtension}", fileExtension);
                    return Json(new { success = false, message = "Type de fichier non autorisé" });
                }

                // Créer le dossier d'upload s'il n'existe pas
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "courriers");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                    _logger.LogInformation("Dossier d'upload créé: {UploadsFolder}", uploadsFolder);
                }

                // Générer un nom de fichier unique
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Sauvegarder le fichier
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("Fichier sauvegardé: {FilePath}", filePath);

                return Json(new { 
                    success = true, 
                    message = "Fichier uploadé avec succès",
                    fileName = file.FileName,
                    fileSize = file.Length,
                    filePath = $"/uploads/courriers/{fileName}",
                    mimeType = file.ContentType,
                    extension = fileExtension
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'upload du fichier: {Message}", ex.Message);
                return Json(new { success = false, message = $"Erreur lors de l'upload du fichier: {ex.Message}" });
            }
        }

        private async Task<string> GenererNumeroOrdreAsync()
        {
            var lastCourrier = await _context.Courriers!
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();
            
            var nextOrdreNumero = 1;
            if (lastCourrier != null && !string.IsNullOrEmpty(lastCourrier.OrdreNumero))
            {
                if (int.TryParse(lastCourrier.OrdreNumero, out int lastNumero))
                {
                    nextOrdreNumero = lastNumero + 1;
                }
            }

            return nextOrdreNumero.ToString("D6"); // Format 000001, 000002, etc.
        }

        private async Task<int> GetValidServiceIdAsync(int? serviceConcerneId)
        {
            // Vérifier que ServiceConcerneId est valide
            if (serviceConcerneId.HasValue && serviceConcerneId.Value > 0)
            {
                // Vérifier que le service existe
                var serviceExists = await _context.Services!.AnyAsync(s => s.Id == serviceConcerneId.Value && s.Actif);
                if (serviceExists)
                {
                    return serviceConcerneId.Value;
                }
            }
            
            // Utiliser le premier service actif comme valeur par défaut
            var defaultService = await _context.Services!.Where(s => s.Actif).FirstOrDefaultAsync();
            if (defaultService != null)
            {
                return defaultService.Id;
            }
            
            // Si aucun service n'est disponible, lancer une exception
            throw new InvalidOperationException("Aucun service actif n'est disponible. Veuillez créer au moins un service.");
        }

        private async Task RechargerDonneesFormulaire(CreateCourrierViewModel model)
        {
            model.AvailableServices = await _context.Services!.Where(s => s.Actif).Select(s => new ServiceOption
            {
                Id = s.Id,
                Nom = s.Nom
            }).ToListAsync() ?? new List<ServiceOption>();
            model.AvailableCategories = await _context.CategorieCourriers!.Select(c => new LookupOption
            {
                Id = c.Id,
                Nom = c.Nom
            }).ToListAsync() ?? new List<LookupOption>();
            model.AvailableNatures = await _context.NatureCourriers!.Select(n => new LookupOption
            {
                Id = n.Id,
                Nom = n.Nom
            }).ToListAsync() ?? new List<LookupOption>();
            model.AvailableModesEnvoi = await _context.ModeEnvois!.Select(m => new LookupOption
            {
                Id = m.Id,
                Nom = m.Libelle
            }).ToListAsync() ?? new List<LookupOption>();
            model.AvailableCorrespondants = await _context.Correspondants!.Select(c => new LookupOption
            {
                Id = c.Id,
                Nom = c.Nom
            }).ToListAsync() ?? new List<LookupOption>();
            model.AvailableDossiers = await _context.DossierClassements!.Select(d => new LookupOption
            {
                Id = d.Id,
                Nom = d.Titre
            }).ToListAsync() ?? new List<LookupOption>();
            model.AvailableTypeDossiers = await _context.TypeDossiers!.Select(t => new LookupOption
            {
                Id = t.Id,
                Nom = t.Libelle ?? "Sans libellé"
            }).ToListAsync() ?? new List<LookupOption>();
        }
    }

    // ViewModels pour CourriersEntrant
    public class CreateCourrierViewModel
    {
        public string OrdreNumero { get; set; } = string.Empty;
        public string? RegistreNumero { get; set; }
        public string? ReferenceNumero { get; set; }
        public string Objet { get; set; } = string.Empty;
        public DateTime DateReception { get; set; }
        public string? HeureRecu { get; set; }
        public string? ServiceConcerne { get; set; }
        public string? UtilisateursEnCopie { get; set; }
        public int? CorrespondantId { get; set; }
        public int? ServiceId { get; set; }
        public int? CategorieCourrierId { get; set; }
        public int? ModeEnvoiId { get; set; }
        public int? NatureCourrierId { get; set; }
        public int? TypeDossierId { get; set; }
        public int? DossierClassementId { get; set; }
        public int Confidentialite { get; set; }
        public int Priorite { get; set; }
        public List<ServiceOption> AvailableServices { get; set; } = new();
        public List<LookupOption> AvailableCategories { get; set; } = new();
        public List<LookupOption> AvailableNatures { get; set; } = new();
        public List<LookupOption> AvailableModesEnvoi { get; set; } = new();
        public List<LookupOption> AvailableCorrespondants { get; set; } = new();
        public List<LookupOption> AvailableDossiers { get; set; } = new();
        public List<LookupOption> AvailableTypeDossiers { get; set; } = new();
    }

    public class CheckNumeroOrdreRequest
    {
        public string NumeroOrdre { get; set; } = string.Empty;
        public string? NumeroRegistre { get; set; }
    }

    public class ServiceOption
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
    }

}
