using System;

namespace MailFarms_SharedService.Response
{
    /// <summary>
    ///     La risposta che si ricevere dall'api dopo una richiesta
    /// </summary>
    [Serializable]
    public class ResponseBoolAvviso
    {
        public string Avviso;

        public bool Result;
    }
}