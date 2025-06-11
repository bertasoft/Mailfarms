using System;

namespace MailFarms_SharedService.Entity
{
    public class StatoLog
    {
        public string UniqueIdentifier { get; set; }

        public DateTime Data { get; set; }

        public string Testo { get; set; }
    }
}
