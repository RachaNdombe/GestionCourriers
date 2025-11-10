using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models
{
    public enum TypeCourrier
    {
        Entrant = 1,
        Sortant = 2,
        Interne = 3
    }

    public enum StatutCourrier
    {
        Brouillon = 0,
        EnCours = 1,
        Traite = 2,
        Refuse = 3,
        Cloture = 4,
        Vise = 5,
        Envoye = 6,
        Archive = 7,
        Classe = 8,
        cloture = 9,
        vise = 10,
        envoye = 11,
        classer = 12,
        refuse = 13
    }

    public enum Priorite
    {
        Basse = 0,
        Normale = 1,
        Haute = 2,
        Urgente = 3
    }

    public enum NiveauConfidentialite
    {
        Public = 0,
        Interne = 1,
        Confidentiel = 2,
        TresConfidentiel = 3
    }

    public enum TypeTransmission
    {
        Rediriger = 1,
        EnvoyerPourESignature = 2,
        ViserCourrier = 3,
        TraiterCourrier = 4,
        CloturerCourrier = 5,
        EnvoyerPourEnvoi = 6
    }

    public enum StatutTraiter
    {
        NonTraite = 0,
        Traite = 1,
        Refuse = 2,
        Transfere = 3,
        Cloture = 4
    }
}