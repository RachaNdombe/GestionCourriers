using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models
{
    public class CategorieCourrierViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Libelle { get; set; }
        public string? Description { get; set; }
        public bool Actif { get; set; }
        public int Ordre { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CategorieCourrierIndexViewModel
    {
        public IList<CategorieCourrierViewModel> CategoriesCourrier { get; set; } = new List<CategorieCourrierViewModel>();
        public string? SearchTerm { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public class CreateCategorieCourrierViewModel
    {
        [Required(ErrorMessage = "Le code est requis")]
        [StringLength(20)]
        public string Code { get; set; }

        [Required(ErrorMessage = "Le libellé est requis")]
        [StringLength(100)]
        public string Libelle { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool Actif { get; set; } = true;

        public int Ordre { get; set; }
    }
}