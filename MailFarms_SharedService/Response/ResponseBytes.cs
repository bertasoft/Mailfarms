using System;

namespace MailFarms_SharedService.Response
{
    /// <summary>
    ///     La risposta che si ricevere dall'api dopo una richiesta
    /// </summary>
    [Serializable]
    public class ResponseBytes
    {
        public byte[] Bytes;
    }
}