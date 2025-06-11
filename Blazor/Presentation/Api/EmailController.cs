using System;
using System.Threading.Tasks;
using System.Web;
using BlazorLibrary.Code;
using Business.Code;
using Business.Entity;
using CommonNetCore.GlobalExtension;
using MailFarmsBlazor.Code;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;

namespace MailFarmsBlazor.Api
{
    /// <summary>
    /// Risponde a /e/c oppure /e/p
    /// </summary>
    public class EmailController : ControllerBase
    {
        [Inject]
        private RefreshService RefreshService { get; set; }

        public EmailController(RefreshService emailRefreshService)
        {
            RefreshService = emailRefreshService;
        }

        /// <summary>
        /// Risponde al pixel per la visualizzazione
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public FileContentResult P()
        {
            //Gif traparente
            var fileGif = File(Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAAAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw=="), "image/gif");

            var id = Request.GetQueryLong("e");

            var email = Email.GetItem(id);

            if (email == null)
                return fileGif;

            if (email.DataVisualizzazione != DateTime.MinValue)
                return fileGif;
            
            email.DataVisualizzazione = DateTime.Now;
            email.Save();

            //lancio l'aggionamento di tutti i browser aperti, aggiorno la data di visualizzazione della email
            var action = RefreshService.OnCodaRefreshCallback;
#pragma warning disable 4014
            Task.Run(() =>
#pragma warning restore 4014
            {
                action?.Invoke();
            });

            return fileGif;
        }

        /// <summary>
        /// Traccia il click
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public RedirectResult C()
        {
            var id = Request.GetQueryLong("e");
            var url = HttpUtility.UrlDecode(Request.GetQueryString("u"));

            if (string.IsNullOrEmpty(url))
                return Redirect("https://doweb.srl");

            var email = Email.GetItem(id);

            if (email == null)
                return Redirect(url);

            if (email.DataVisualizzazione == DateTime.MinValue)
                email.DataVisualizzazione = DateTime.Now;
            
            email.DataClick = DateTime.Now;            
            
            email.Save();

            //lancio l'aggionamento di tutti i browser aperti, aggiorno la data di click della email
            var action = RefreshService.OnCodaRefreshCallback;
#pragma warning disable 4014
            Task.Run(() =>
#pragma warning restore 4014
            {
                action?.Invoke();
            });

            return Redirect(url);
        }
    }
}