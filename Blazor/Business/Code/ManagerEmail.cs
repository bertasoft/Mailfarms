using System.Collections.Generic;
using System.IO;
using Business.Entity;
using CommonNetCore;
using CommonNetCore.GlobalExtension;
using CommonNetCore.Misc;

namespace Business.Code
{
    public class ManagerEmail
    {
        private static string _template;

        private static string Template
        {
            get
            {
                if (_template.IsNullOrEmpty())
                    _template = File.ReadAllText(Settings.DiskPath + @"\Layout\EmailTemplate.html");

                return _template;
            }
        }

        public static bool InviaEmail(string mittente, string destinatario, string oggetto, string contenuto)
        {
            if (!mittente.IsEmail() || !destinatario.IsEmail())
            {
                ManagerLog.Warn("ManagerEmail.InviaEmail è stato chiamato con una email non valida: " + mittente + ", " + destinatario);
                return false;
            }

            contenuto = contenuto.Replace("[OGGETTO]", oggetto);

            var destinatari = new List<CommonNetCore.Misc.ManagerEmail.Destinatari>();
            destinatari.Add(new CommonNetCore.Misc.ManagerEmail.Destinatari(destinatario, destinatario));

            return CommonNetCore.Misc.ManagerEmail.InviaEmail(out _, "DOWEB.SRL", mittente, destinatari, "DOWEB.SRL - " + oggetto.Decode().StripTagsCharArray(), contenuto);
        }

        public static void InviaEmail(string email, string oggetto, string contenuto)
        {
            InviaEmail(Settings.Config.Email.Sender, email, oggetto, contenuto.Replace("[OGGETTO]", oggetto));
        }

        private static string TemplateEmail(string contenuto)
        {
            contenuto = Template.Replace("[CONTENUTO]", contenuto.CommonReplace());
            contenuto = contenuto.Replace("[URL]", Settings.Config.WebPath);
            contenuto = contenuto.Replace("[EMAILFOOTER]", Etichette.GetValore(Etichette.EtichetteEnum.EmailFooter).CommonReplace());

            return contenuto;
        }

        public static void RecuperaPassword(Utenti utente)
        {
            if (utente == null)
                return;

            var oggetto = Etichette.GetValore(Etichette.EtichetteEnum.EmailRecuperaDatiAccessoOggetto);
            var contenuto = TemplateEmail(Etichette.GetValore(Etichette.EtichetteEnum.EmailRecuperaDatiAccessoContenuto));

            contenuto = contenuto.Replace(utente);
            oggetto = oggetto.Replace(utente);

            InviaEmail(utente.Email, oggetto, contenuto);
        }

        /// <summary>
        ///     Invia l'email contenente il link per sbloccare l'accesso al sito
        /// </summary>
        internal static void SbloccaAccesso(Utenti utenti = null)
        {
            if (utenti != null)
            {
                var oggetto = Etichette.GetValore(Etichette.EtichetteEnum.EmailAccessoBloccatoOggetto)
                    .Replace(utenti);

                var contenuto = TemplateEmail(Etichette.GetValore(Etichette.EtichetteEnum.EmailAccessoBloccatoContenuto))
                    .Replace(utenti);

                InviaEmail(utenti.Email, oggetto, contenuto);
            }
        }
    }
}