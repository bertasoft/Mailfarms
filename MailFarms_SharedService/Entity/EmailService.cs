using System;

namespace MailFarms_SharedService.Entity
{
    /// <summary>
    /// La mail che viene inviata da MailFarms.com ai vari windows service
    /// </summary>
    public class EmailService
    {
        public EmailService()
        {
            UniqueIdentifier = string.Empty;
            MittenteEmail = string.Empty;
            MittenteNome = string.Empty;
            DestinatarioEmail = string.Empty;
            DestinatarioNome = string.Empty;
            UrlEliminazione = string.Empty;
            Oggetto = string.Empty;
            Contenuto = string.Empty;
            RispondiA = string.Empty;
            Allegati = Array.Empty<Allegati>();
        }

        public string UniqueIdentifier { get; set; }
        public string MittenteEmail { get; set; }
        public string MittenteNome { get; set; }
        public string DestinatarioEmail { get; set; }
        public string DestinatarioNome { get; set; }
        public DateTime DestinatarioDataRegistrazione { get; set; }
        public string UrlEliminazione { get; set; }
        public string Oggetto { get; set; }
        public string Contenuto { get; set; }
        public string RispondiA { get; set; }
        public bool Immediata { get; set; }
        public Allegati[] Allegati { get; set; }
    }
}
