using BlazorLibrary.Code;
using Business.Entity;
using CommonNetCore;
using CommonNetCore.GlobalExtension;

namespace Business.Code
{
    public static class ManagerTagReplace
    {
        public enum RuoliEnum
        {
            Amministratore
        }

        /// <summary>
        ///     Sostituisce la chiave i campi con i valori
        /// </summary>
        public static string Replace(this string text, Utenti utente)
        {
            if (utente == null)
                return text;

            text = text
                .Replace("[ID]", utente.Id.ToString())
                .Replace("[NOME]", utente.Nome)
                .Replace("[COGNOME]", utente.Cognome)
                .Replace("[EMAIL]", utente.Email)
                .Replace("[PASSWORD]", utente.Password)
                .Replace("[URLREIMPOSTAPASSWORD]", ManagerQueryString.GetEncodedLink(Settings.Config.WebPath + "/password?UtentiId=" + utente.Id));

            return text;
        }

        /// <summary>
        ///     Replace comune per tutte le email
        /// </summary>
        public static string CommonReplace(this string text, bool replacebr = true)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            text = text.ConvertUrlsToLinks();

            text = text.Replace("\"", "'"); //nei tooltip o in qualsiasi posto dove vado ad inserire la stringa dinamica verificare che non vengano usati apici singoli

            if (replacebr)
            {
                text = text.Replace("\n\r", "<br>");
                text = text.Replace("\n", "<br>");
            }

            return text;
        }
    }
}