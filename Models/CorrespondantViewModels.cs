using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models
{
    public class CorrespondantCreateViewModel
    {
        [Required(ErrorMessage = "Le type d'expéditeur est requis")]
        public RaisonSociale RaisonSociale { get; set; }

        [Required(ErrorMessage = "Le nom est requis")]
        [StringLength(100)]
        public string Nom { get; set; }

        [StringLength(10)]
        public string? Civilite { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Contact1 { get; set; }

        [StringLength(20)]
        public string? Contact2 { get; set; }

        [StringLength(20)]
        public string? Fax { get; set; }

        [StringLength(200)]
        public string? Adresse { get; set; }

        [StringLength(50)]
        public string? Ville { get; set; }

        [StringLength(10)]
        public string? CodePostal { get; set; }

        [StringLength(50)]
        public string? Pays { get; set; }

        [StringLength(100)]
        [Url]
        public string? SiteWeb { get; set; }
    }

    public class CorrespondantViewModel
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string? Civilite { get; set; }
        public string? Email { get; set; }
        public string? Contact1 { get; set; }
        public string? Ville { get; set; }
        public string? Pays { get; set; }
        public RaisonSociale RaisonSociale { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CorrespondantIndexViewModel
    {
        public IList<CorrespondantViewModel> Correspondants { get; set; } = new List<CorrespondantViewModel>();
        public string? SearchTerm { get; set; }
        public string? TypeFilter { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<RaisonSociale> AvailableTypes { get; set; } = new List<RaisonSociale>();
    }
}