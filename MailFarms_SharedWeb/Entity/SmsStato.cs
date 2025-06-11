using System;
using MailFarms_SharedWeb.Response;

namespace MailFarms_SharedWeb.Entity
{
    public class SmsStato : ResponseBoolAvviso
    {
        public SmsStato()
        {
            Errore = string.Empty;
        }

        public bool Inviato { get; set; }
        public DateTime DataInvio { get; set; }
        public string Errore { get; set; }
    }
}
