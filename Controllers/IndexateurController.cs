
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using JconsultGC.Data;
using JconsultGC.Models;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JconsultGC.Controllers
{
    [Authorize]
    public class IndexateurController : Controller
    {
        private readonly ProjetIdDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<IndexateurController> _logger;
        private readonly IWebHostEnvironment _environment;

        public IndexateurController(
            ProjetIdDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<IndexateurController> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _environment = environment;
        }

        // GET: Indexateur/Dashboard
        [Authorize(Roles = "Indexateur")]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var today = DateTime.Today;
            
            var viewModel = new IndexateurDashboardViewModel
            {
                TotalCourriersIndexes = await _context.Courriers!
                    .Where(c => c.CreatedById == user.Id)
                    .CountAsync(),
                CourriersTresPrioritaires = await _context.Courriers!
                    .Where(c => c.CreatedById == user.Id && c.Priorite == (PrioriteLevel)3) // Urgent
                    .CountAsync(),
                CourriersEnAttente = await _context.Courriers!
                    .Where(c => c.CreatedById == user.Id && c.Statut == "A valider")
                    .CountAsync(),
                ActionsAujourdhui = await _context.CourrierHistories!
                    .Where(h => h.UserId == user.Id && h.Timestamp.Date == today)
                    .CountAsync(),
                CourriersASaisir = await _context.Courriers!
                    .Where(c => c.CreatedById == user.Id && c.Statut == "A saisir")
                    .CountAsync(),
                CourriersSignes = await _context.Courriers!
                    .Where(c => c.Statut == "Signé")
                    .CountAsync(),
                RecentActions = await _context.CourrierHistories!
                    .Where(h => h.UserId == user.Id && h.Timestamp.Date == today)
                    .OrderByDescending(h => h.Timestamp)
                    .Take(10)
                    .ToListAsync(),
                CourriersAValider = await _context.Courriers!
                    .Where(c => c.CreatedById == user.Id && c.Statut == "A valider")
                    .OrderByDescending(c => c.DateEnregistrement)
                    .Take(10)
                    .Include(c => c.CategorieCourrier)
                    .ToListAsync(),
                CourriersSignesList = await _context.Courriers!
                    .Where(c => c.Statut == "Signé")
                    .OrderByDescending(c => c.DateEnregistrement)
                    .Take(10)
                    .Include(c => c.CategorieCourrier)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // GET: Indexateur/Create
        [Authorize(Roles = "Indexateur")]
        public async Task<IActionResult> Create()
        {
            // Générer automatiquement le numéro d'ordre
            var numeroOrdre = await GenererNumeroOrdreAsync();
            
            var viewModel = new CourrierCreateViewModel
            {
                OrdreNumero = numeroOrdre,
                DateReception = DateTime.Now,
                AvailableServices = await _context.Services!.Select(s => new ServiceOption
                {
                    Id = s.Id,
                    Nom = s.Nom ?? "Nom non défini"
                }).ToListAsync() ?? new List<ServiceOption>(),
                AvailableCategories = await _context.CategorieCourriers!.Select(c => new JconsultGC.Models.LookupOption
                {
                    Id = c.Id,
                    Nom = c.Nom
                }).ToListAsync(),
                AvailableNatures = await _context.NatureCourriers!.Select(n => new JconsultGC.Models.LookupOption
                {
                    Id = n.Id,
                    Nom = n.Nom
                }).ToListAsync(),
                AvailableModesEnvoi = await _context.ModeEnvois!.Select(m => new JconsultGC.Models.LookupOption
                {
                    Id = m.Id,
                    Nom = m.Libelle
                }).ToListAsync(),
                AvailableCorrespondants = await _context.Correspondants!.Select(c => new JconsultGC.Models.LookupOption
                {
                    Id = c.Id,
                    Nom = c.Nom
                }).ToListAsync(),
                AvailableDossiers = await _context.DossierClassements!.Select(d => new JconsultGC.Models.LookupOption
                {
                    Id = d.Id,
                    Nom = d.Titre
                }).ToListAsync(),
                AvailableTypeDossiers = await _context.TypeDossiers!.Select(t => new JconsultGC.Models.LookupOption
                {
                    Id = t.Id,
                    Nom = t.Libelle ?? "Sans libellé"
                }).ToListAsync(),

            };
           

            return View(viewModel);
        }

        // POST: Indexateur/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Indexateur")]
        public async Task<IActionResult> Create(CourrierCreateViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                var courrier = new Courrier
                {
                    OrdreNumero = model.OrdreNumero,
                    RegistreNumero = model.RegistreNumero ?? "",
                    ReferenceNumero = model.ReferenceNumero,
                    Objet = model.Objet,
                    DateReception = model.DateReception,
                    HeureRecu = model.HeureRecu,
                    ServiceConcerne = model.ServiceConcerne,
                    UtilisateursEnCopie = model.UtilisateursEnCopie,
                    CorrespondantId = model.CorrespondantId,
                    ServiceId = model.ServiceId,
                    CategorieCourrierId = model.CategorieCourrierId,
                    ModeEnvoiId = model.ModeEnvoiId,
                    NatureCourrierId = model.NatureCourrierId,
                    TypeDossierId = model.TypeDossierId,
                    DossierClassementId = model.DossierClassementId,
                    Confidentialite = model.Confidentialite,
                    Priorite = model.Priorite,
                    DateEnregistrement = DateTime.UtcNow,
                    Statut = "A valider",
                    CreatedById = user.Id
                };

                _context.Courriers!.Add(courrier);
                await _context.SaveChangesAsync();

                // Generate PDF from courrier data
                try
                {
                    var pdfPath = await GenerateAndSaveCourrierPdfAsync(courrier);
                    
                    // Create a document record for the generated PDF
                    var document = new Document
                    {
                        NomFichier = $"Courrier_{courrier.OrdreNumero}.pdf",
                        StoragePath = pdfPath,
                        MimeType = "application/pdf",
                        Taille = new FileInfo(Path.Combine("wwwroot", pdfPath.TrimStart('/'))).Length,
                        CourrierId = courrier.Id,
                        CreatedAtUtc = DateTime.UtcNow
                    };

                    _context.Documents!.Add(document);
                    await _context.SaveChangesAsync();
                }
                catch (Exception pdfEx)
                {
                    _logger.LogWarning(pdfEx, "Erreur lors de la génération du PDF pour le courrier {CourrierId}", courrier.Id);
                    // Continue without PDF - courrier is still saved
                }

                TempData["SuccessMessage"] = "Courrier entrant créé avec succès !";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la sauvegarde du courrier entrant");
                ModelState.AddModelError("", "Une erreur s'est produite lors de la sauvegarde. Veuillez réessayer.");
                await RechargerDonneesFormulaire(model);
                return View(model);
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

        private async Task RechargerDonneesFormulaire(CourrierCreateViewModel model)
        {
            model.AvailableServices = await _context.Services!.Select(s => new ServiceOption
            {
                Id = s.Id,
                Nom = s.Nom
            }).ToListAsync() ?? new List<ServiceOption>();
            model.AvailableCategories = await _context.CategorieCourriers!.Select(c => new JconsultGC.Models.LookupOption
            {
                Id = c.Id,
                Nom = c.Nom
            }).ToListAsync();
            model.AvailableNatures = await _context.NatureCourriers!.Select(n => new JconsultGC.Models.LookupOption
            {
                Id = n.Id,
                Nom = n.Nom
            }).ToListAsync();
            model.AvailableModesEnvoi = await _context.ModeEnvois!.Select(m => new JconsultGC.Models.LookupOption
            {
                Id = m.Id,
                Nom = m.Libelle
            }).ToListAsync();
            model.AvailableCorrespondants = await _context.Correspondants!.Select(c => new JconsultGC.Models.LookupOption
            {
                Id = c.Id,
                Nom = c.Nom
            }).ToListAsync();
            model.AvailableDossiers = await _context.DossierClassements!.Select(d => new JconsultGC.Models.LookupOption
            {
                Id = d.Id,
                Nom = d.Titre
            }).ToListAsync();
            model.AvailableTypeDossiers = await _context.TypeDossiers!.Select(t => new JconsultGC.Models.LookupOption
            {
                Id = t.Id,
                Nom = t.Libelle ?? "Sans libellé"
            }).ToListAsync();
        }

        private async Task<string> GenerateAndSaveCourrierPdfAsync(Courrier courrier)
        {
            // Load related data
            await LoadRelatedDataAsync(courrier);

            // Create a simple text-based PDF content
            var pdfContent = GenerateCourrierPdfContent(courrier);
            var pdfBytes = Encoding.UTF8.GetBytes(pdfContent);
            
            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "courriers");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Generate filename
            var fileName = $"courrier_{courrier.OrdreNumero}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save PDF file
            await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

            // Return relative path for database storage
            return $"/uploads/courriers/{fileName}";
        }

        private string GenerateCourrierPdfContent(Courrier courrier)
        {
            var content = new StringBuilder();
            
            // PDF Header
            content.AppendLine("%PDF-1.4");
            content.AppendLine("1 0 obj");
            content.AppendLine("<<");
            content.AppendLine("/Type /Catalog");
            content.AppendLine("/Pages 2 0 R");
            content.AppendLine(">>");
            content.AppendLine("endobj");
            
            // Simple PDF content with courrier data
            content.AppendLine("2 0 obj");
            content.AppendLine("<<");
            content.AppendLine("/Type /Pages");
            content.AppendLine("/Kids [3 0 R]");
            content.AppendLine("/Count 1");
            content.AppendLine(">>");
            content.AppendLine("endobj");
            
            // Page content
            content.AppendLine("3 0 obj");
            content.AppendLine("<<");
            content.AppendLine("/Type /Page");
            content.AppendLine("/Parent 2 0 R");
            content.AppendLine("/MediaBox [0 0 612 792]");
            content.AppendLine("/Contents 4 0 R");
            content.AppendLine(">>");
            content.AppendLine("endobj");
            
            // Text content
            var textContent = new StringBuilder();
            textContent.AppendLine("FICHE COURRIER ENTRANT");
            textContent.AppendLine("======================");
            textContent.AppendLine();
            textContent.AppendLine($"N° d'ordre: {courrier.OrdreNumero ?? "N/A"}");
            textContent.AppendLine($"N° de registre: {courrier.RegistreNumero ?? "N/A"}");
            textContent.AppendLine($"Référence: {courrier.ReferenceNumero ?? "N/A"}");
            textContent.AppendLine($"Date réception: {courrier.DateReception?.ToString("dd/MM/yyyy") ?? "N/A"}");
            textContent.AppendLine($"Heure réception: {courrier.HeureRecu?.ToString(@"hh\:mm") ?? "N/A"}");
            textContent.AppendLine();
            textContent.AppendLine("OBJET:");
            textContent.AppendLine("------");
            textContent.AppendLine(courrier.Objet ?? "Non spécifié");
            textContent.AppendLine();
            textContent.AppendLine("CLASSIFICATION:");
            textContent.AppendLine("---------------");
            textContent.AppendLine($"Catégorie: {courrier.CategorieCourrier?.Nom ?? "N/A"}");
            textContent.AppendLine($"Nature: {courrier.NatureCourrier?.Nom ?? "N/A"}");
            textContent.AppendLine($"Type dossier: {courrier.TypeDossier?.Libelle ?? "N/A"}");
            textContent.AppendLine($"Dossier classement: {courrier.DossierClassement?.Titre ?? "N/A"}");
            textContent.AppendLine();
            textContent.AppendLine("CORRESPONDANT ET SERVICE:");
            textContent.AppendLine("-------------------------");
            textContent.AppendLine($"Expéditeur: {courrier.Correspondant?.Nom ?? "N/A"}");
            textContent.AppendLine($"Mode envoi: {courrier.ModeEnvoi?.Libelle ?? "N/A"}");
            textContent.AppendLine($"Service concerné: {courrier.ServiceConcerne ?? "N/A"}");
            textContent.AppendLine($"Service destinataire: {courrier.Service?.Nom ?? "N/A"}");
            textContent.AppendLine();
            textContent.AppendLine("CARACTÉRISTIQUES:");
            textContent.AppendLine("-----------------");
            textContent.AppendLine($"Confidentialité: {GetConfidentialiteText(courrier.Confidentialite)}");
            textContent.AppendLine($"Priorité: {GetPrioriteText(courrier.Priorite)}");
            textContent.AppendLine($"Utilisateurs en copie: {courrier.UtilisateursEnCopie ?? "Aucun"}");
            textContent.AppendLine();
            textContent.AppendLine("MÉTADONNÉES:");
            textContent.AppendLine("------------");
            textContent.AppendLine($"Date enregistrement: {courrier.DateEnregistrement:dd/MM/yyyy HH:mm}");
            textContent.AppendLine($"Statut: {courrier.Statut ?? "N/A"}");
            if (courrier.CreatedBy != null)
            {
                textContent.AppendLine($"Indexé par: {courrier.CreatedBy.Nom} {courrier.CreatedBy.Prenom}");
            }
            
            // Convert text content to PDF stream
            var streamContent = Encoding.UTF8.GetBytes(textContent.ToString());
            
            content.AppendLine("4 0 obj");
            content.AppendLine("<<");
            content.AppendLine("/Length " + streamContent.Length);
            content.AppendLine(">>");
            content.AppendLine("stream");
            content.Append(textContent.ToString());
            content.AppendLine("endstream");
            content.AppendLine("endobj");
            
            // Cross-reference table
            content.AppendLine("xref");
            content.AppendLine("0 5");
            content.AppendLine("0000000000 65535 f ");
            content.AppendLine("0000000015 00000 n ");
            content.AppendLine("0000000074 00000 n ");
            content.AppendLine("0000000175 00000 n ");
            content.AppendLine("0000000300 00000 n ");
            
            // Trailer
            content.AppendLine("trailer");
            content.AppendLine("<<");
            content.AppendLine("/Size 5");
            content.AppendLine("/Root 1 0 R");
            content.AppendLine(">>");
            content.AppendLine("startxref");
            content.AppendLine("0");
            content.AppendLine("%%EOF");
            
            return content.ToString();
        }

        private async Task LoadRelatedDataAsync(Courrier courrier)
        {
            // Load related entities if not already loaded
            if (courrier.CategorieCourrier == null && courrier.CategorieCourrierId.HasValue)
            {
                courrier.CategorieCourrier = await _context.CategorieCourriers!
                    .FirstOrDefaultAsync(c => c.Id == courrier.CategorieCourrierId.Value);
            }

            if (courrier.NatureCourrier == null && courrier.NatureCourrierId.HasValue)
            {
                courrier.NatureCourrier = await _context.NatureCourriers!
                    .FirstOrDefaultAsync(n => n.Id == courrier.NatureCourrierId.Value);
            }

            if (courrier.Correspondant == null && courrier.CorrespondantId.HasValue)
            {
                courrier.Correspondant = await _context.Correspondants!
                    .FirstOrDefaultAsync(c => c.Id == courrier.CorrespondantId.Value);
            }

            if (courrier.Service == null && courrier.ServiceId.HasValue)
            {
                courrier.Service = await _context.Services!
                    .FirstOrDefaultAsync(s => s.Id == courrier.ServiceId.Value);
            }

            if (courrier.ModeEnvoi == null && courrier.ModeEnvoiId.HasValue)
            {
                courrier.ModeEnvoi = await _context.ModeEnvois!
                    .FirstOrDefaultAsync(m => m.Id == courrier.ModeEnvoiId.Value);
            }

            if (courrier.TypeDossier == null && courrier.TypeDossierId.HasValue)
            {
                courrier.TypeDossier = await _context.TypeDossiers!
                    .FirstOrDefaultAsync(t => t.Id == courrier.TypeDossierId.Value);
            }

            if (courrier.DossierClassement == null && courrier.DossierClassementId.HasValue)
            {
                courrier.DossierClassement = await _context.DossierClassements!
                    .FirstOrDefaultAsync(d => d.Id == courrier.DossierClassementId.Value);
            }

            if (courrier.CreatedBy == null && !string.IsNullOrEmpty(courrier.CreatedById))
            {
                courrier.CreatedBy = await _context.Users!
                    .FirstOrDefaultAsync(u => u.Id == courrier.CreatedById);
            }
        }

        private string GetConfidentialiteText(ConfidentialiteLevel confidentialite)
        {
            return confidentialite switch
            {
                ConfidentialiteLevel.Public => "Public",
                ConfidentialiteLevel.Interne => "Interne",
                ConfidentialiteLevel.Confidentiel => "Confidentiel",
                ConfidentialiteLevel.TresConfidentiel => "Très confidentiel",
                _ => "Non spécifié"
            };
        }

        private string GetPrioriteText(PrioriteLevel priorite)
        {
            return priorite switch
            {
                PrioriteLevel.Basse => "Basse",
                PrioriteLevel.Normal => "Normal",
                PrioriteLevel.Haute => "Haute",
                PrioriteLevel.Urgent => "Urgent",
                _ => "Non spécifié"
            };
        }
    }
}
