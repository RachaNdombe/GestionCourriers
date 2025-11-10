using JconsultGC.Models;
using System.ComponentModel.DataAnnotations;

namespace JconsultGC.ViewModels
{
    public class CourrierViewModel
    {
        public int Id { get; set; }
        public string NumeroOrdre { get; set; } = string.Empty;
        public string NumeroRegistre { get; set; } = string.Empty;
        public string Objet { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public TypeCourrier TypeCourrier { get; set; }
        public StatutCourrier Statut { get; set; }
        public Priorite Priorite { get; set; }
        public string? ServiceConcerne { get; set; }
        public string? Correspondant { get; set; }
        public string? CategorieCourrier { get; set; }
        public string? NatureCourrier { get; set; }
        public string? DossierClassement { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DateReception { get; set; }
        public DateTime? DateEnvoi { get; set; }
        public bool Viseur { get; set; }
        public bool Validation { get; set; }
        public bool ValidationSaisie { get; set; }
        public bool EstArchive { get; set; }
    }

    public class CourrierIndexViewModel
    {
        public List<CourrierViewModel> Courriers { get; set; } = new();
        public string SearchTerm { get; set; } = string.Empty;
        public string TypeFilter { get; set; } = string.Empty;
        public string StatutFilter { get; set; } = string.Empty;
        public string ServiceFilter { get; set; } = string.Empty;
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<TypeCourrier> AvailableTypes { get; set; } = new();
        public List<StatutCourrier> AvailableStatuts { get; set; } = new();
        public List<Priorite> AvailablePriorites { get; set; } = new();
        public List<ServiceOption> AvailableServices { get; set; } = new();
    }

    public class CourrierDetailsViewModel
    {
        public int Id { get; set; }
        public string NumeroOrdre { get; set; } = string.Empty;
        public string NumeroRegistre { get; set; } = string.Empty;
        public string Objet { get; set; } = string.Empty;
        public string ReferenceCourrier { get; set; } = string.Empty;
        public TypeCourrier TypeCourrier { get; set; }
        public StatutCourrier Statut { get; set; }
        public Priorite Priorite { get; set; }
        public NiveauConfidentialite NiveauConfidentialite { get; set; }
        public DateTime? DateReception { get; set; }
        public DateTime? DateEnvoi { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool ValidationSaisie { get; set; }
        public bool Validation { get; set; }
        public bool EstArchive { get; set; }
        public string? Viseur { get; set; }
        public string? ServiceConcerneNom { get; set; }
        public string? CorrespondantNom { get; set; }
        public string NatureCourrierNom { get; set; } = string.Empty;
        public string DossierClassementNom { get; set; } = string.Empty;
        public string? CreatedBy { get; set; }
        public int? CorrespondantId { get; set; }
        public string? Correspondant { get; set; }
        public string? CorrespondantEmail { get; set; }
        public string? CorrespondantRaisonSociale { get; set; }
        public string? CorrespondantContact1 { get; set; }
        public string? CorrespondantContact2 { get; set; }
        public string? CorrespondantAdresse { get; set; }
        public List<PieceJointeViewModel> PiecesJointes { get; set; } = new();
        public List<SuivieCourrierViewModel> Historique { get; set; } = new();
        public List<AnnotationCourrierViewModel> Notes { get; set; } = new();
        public List<DiffusionCourrierViewModel> ListeDiffusion { get; set; } = new();
    }

    public class CreateCourrierViewModel
    {
        public TypeCourrier Type { get; set; }
        public TypeCourrier TypeCourrier { get; set; }
        public StatutCourrier Statut { get; set; }
        public StatutCourrier StatutCourrier { get; set; }
        public Priorite Priorite { get; set; }
        public NiveauConfidentialite NiveauConfidentialite { get; set; }
        public DateTime DateReception { get; set; }
        public List<ServiceOption> AvailableServices { get; set; } = new();
        public List<LookupOption> AvailableCategories { get; set; } = new();
        public List<LookupOption> AvailableNatures { get; set; } = new();
        public List<LookupOption> AvailableModesEnvoi { get; set; } = new();
        public List<LookupOption> AvailableCorrespondants { get; set; } = new();
        public List<LookupOption> AvailableDossiers { get; set; } = new();
    }

    public class PieceJointeViewModel
    {
        public int Id { get; set; }
        public string NomFichier { get; set; } = string.Empty;
        public string CheminFichier { get; set; } = string.Empty;
        public long TailleFichier { get; set; }
        public string TypeMime { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class SuivieCourrierViewModel
    {
        public int Id { get; set; }
        public string Observations { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string UserSuiviNom { get; set; } = string.Empty;
        public string UserSuiviEmail { get; set; } = string.Empty;
    }

    public class AnnotationCourrierViewModel
    {
        public int Id { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedByNom { get; set; } = string.Empty;
    }

    public class DiffusionCourrierViewModel
    {
        public int Id { get; set; }
        public string ServiceNom { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedByNom { get; set; } = string.Empty;
        public string? Commentaire { get; set; }
    }

    public class ProcessObservationRequest
    {
        public List<int> CourrierIds { get; set; } = new();
        public string Observation { get; set; } = string.Empty;
    }

    public class ViserCourriersRequest
    {
        public List<int> CourrierIds { get; set; } = new();
        public string Observation { get; set; } = string.Empty;
    }

    public class RepondreCourrierRequest
    {
        public List<int> CourrierIds { get; set; } = new();
        public int CourrierReponseId { get; set; }
        public string Observation { get; set; } = string.Empty;
        public bool Cloturer { get; set; }
    }
}