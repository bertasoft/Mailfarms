#region Using

#endregion

using System;

namespace MailFarms_SharedService.Entity
{
    public class Allegati 
    {
        public string NomeFile { get; set; }
        public byte[] Bytes { get; set; }
    }
}