using System.ComponentModel.DataAnnotations;
using JconsultGC.Models;

namespace JconsultGC.ViewModels
{
    public class AnnotationsListViewModel
    {
        public int CourrierId { get; set; }
        public string NumeroOrdre { get; set; } = string.Empty;
        public string Objet { get; set; } = string.Empty;
        public List<AnnotationViewModel> Annotations { get; set; } = new();
        public CreateAnnotationViewModel CreateAnnotation { get; set; } = new();
    }

    public class AnnotationViewModel
    {
        public int Id { get; set; }
        public int CourrierId { get; set; }
        public string Note { get; set; } = string.Empty;
        public string UserNoteNom { get; set; } = string.Empty;
        public string? UsersCopieNom { get; set; }
        public DateTime DateNote { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAnnotationViewModel
    {
        public int CourrierId { get; set; }

        [Required(ErrorMessage = "Le contenu de l'annotation est requis")]
        [StringLength(1000, ErrorMessage = "L'annotation ne peut pas dépasser 1000 caractères")]
        public string Note { get; set; } = string.Empty;

        public string? UsersCopieId { get; set; }
        public List<LookupOption> AvailableUsers { get; set; } = new();
    }
}