using System.Collections.Generic;

namespace JconsultGC.Models
{
    public class IndexateurDashboardViewModel
    {
        public IEnumerable<CourrierHistory> RecentActions { get; set; } = new List<CourrierHistory>();
        public IEnumerable<Courrier> RecentCourriers { get; set; } = new List<Courrier>();
        public IEnumerable<Courrier> CourriersAValider { get; set; } = new List<Courrier>();

        // Statistics
        public int TotalCourriersIndexes { get; set; }
        public int CourriersTresPrioritaires { get; set; }
        public int CourriersEnAttente { get; set; }
        public Dictionary<string, int> CourriersByCategory { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> CourriersByStatus { get; set; } = new Dictionary<string, int>();
    }
}