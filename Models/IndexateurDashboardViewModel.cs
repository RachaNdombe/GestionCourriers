using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models
{
    public class IndexateurDashboardViewModel
    {
        public int TotalCourriersIndexes { get; set; }
        public int CourriersTresPrioritaires { get; set; }
        public int CourriersEnAttente { get; set; }
        public int ActionsAujourdhui { get; set; }
        public int CourriersASaisir { get; set; }
        public List<CourrierHistory>? RecentActions { get; set; }
        public List<Courrier>? CourriersAValider { get; set; }
        public List<Courrier>? RecentCourriers { get; set; }
        public Dictionary<string, int>? CourriersByCategory { get; set; }
        public Dictionary<string, int>? CourriersByStatus { get; set; }
    }
}