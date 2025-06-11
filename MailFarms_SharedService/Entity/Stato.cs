using System;

namespace MailFarms_SharedService.Entity
{
    /// <summary>
    /// La risposta che viene inviata dai windows service a mailfarms per aggiornare lo stato di una email
    /// </summary>
    public class Stato
    {
        public enum StatoEnum
        {
            Salvataggio = -1,
            Coda = 0,
            Inviata = 1,
            Errata = 2
        }

        public StatoLog[] Log { get; set; }
        public string UniqueIdentifier { get; set; }
        public string StatusCode4xx5xx { get; set; }
        public DateTime DataInvio { get; set; }
        public DateTime DataUltimoTentativo { get; set; }
        public DateTime DataProssimoTentativo { get; set; }
        public long NumeroTentativi { get; set; }
        public StatoEnum Statoenum { get; set; }
    }
}
