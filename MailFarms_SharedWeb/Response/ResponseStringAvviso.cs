using System;

namespace MailFarms_SharedWeb.Response
{
    /// <summary>
    ///     La risposta che si ricevere dall'api dopo una richiesta
    /// </summary>
    [Serializable]
    public class ResponseStringAvviso
    {
        public string String;

        public string Avviso;
    }
}