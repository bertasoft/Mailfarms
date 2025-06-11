using System;

namespace MailFarms_SharedWeb.Entity
{
    public class EmailStato
    {
        /// <summary>
        /// errore sulla comunicazione tra servizi o altro non legato alla mail
        /// </summary>
        public string Avviso { get; set; }

        public DateTime DataInvio { get; set; }
        
        public DateTime DataVisualizzazione { get; set; }
        
        public DateTime DataClick { get; set; }
        
        /// <summary>
        /// errore durante l'invio
        /// </summary>
        public string Errore { get; set; }
    }
}
