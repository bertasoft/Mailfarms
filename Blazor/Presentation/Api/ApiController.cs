using System;
using System.Linq;
using Business.Entity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Web;
using Business.Code;
using CommonNetCore;
using CommonNetCore.GlobalExtension;
using CommonNetCore.Misc;
using MailFarms_SharedWeb.Code;
using MailFarmsBlazor.Code;
using Microsoft.AspNetCore.Components;
using MailFarms_SharedWeb.Entity;
using MailFarms_SharedWeb.Response;
using BlazorLibrary.Code;

namespace MailFarmsBlazor.Api
{
    /// <summary>
    /// Richieste esterne risponde a http://indirizzo/api
    /// </summary>
    public class ApiController : ControllerBase //, SharedWeb.Code.IResponse
    {
        #region Common

        [Inject]
        private RefreshService RefreshService { get; set; }

        public ApiController(RefreshService emailRefreshService)
        {
            RefreshService = emailRefreshService;
        }

        private void BrowserRefresh()
        {
            if (RefreshService == null)
                return;

            //lancio l'aggionamento di tutti i browser aperti
            var action = RefreshService.OnCodaRefreshCallback;

            action?.Invoke();
        }

        #endregion

        /// <summary>
        /// Usato da Jquery, che viene chiamato da JsRuntime.InvokeAsync, dentro _Host.cshtml
        /// </summary>
        [HttpGet]
        public string GetIP()
        {
            return Request.HttpContext.Connection.RemoteIpAddress.ToString();
        }

        /// <summary>
        /// Usato da Jquery, che viene chiamato da JsRuntime.InvokeAsync, dentro _Host.cshtml
        /// </summary>
        [HttpGet]
        public string GetUserAgent()
        {
            return Request.Headers["User-Agent"];
        }

        /// <summary>
        /// Metodo di verifica lanciato dai client per capire se il servizio è installato e risponde ai controlli
        /// </summary>
        [HttpGet]
        public string Ping()
        {
            return "true";
        }

        #region Windows Service

        /// <summary>
        /// Chiamato dai windows service, che voglio sapere se sono attivi
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task GetAttivo()
        {
            var indirizzoIp = await ApiUtility.ReadStringContent(Request).ConfigureAwait(false);

            var server = Server.GetItem(indirizzoIp);

            if (server == null)
            {
                await ApiUtility.SerializeAndSend("false", Response).ConfigureAwait(false);
                return;
            }

            await ApiUtility.SerializeAndSend(server.Attivo.ToString(), Response).ConfigureAwait(false);
        }

        /// <summary>
        /// Viene chiamato dai windows service, mi invia lo stato della mail (inviata, errata, prossimo tentativo, log, ecc...)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task SegnalaStato()
        {
            var stato = await ApiUtility.ReadArrayContent<MailFarms_SharedService.Entity.Stato>(Request).ConfigureAwait(false);

            if (stato == null)
            {
                await ApiUtility.SerializeAndSend(new MailFarms_SharedService.Response.ResponseBoolAvviso()
                {
                    Avviso = string.Empty,
                    Result = true
                }, Response).ConfigureAwait(false);

                return;
            }

            var email = Email.GetItem(stato.UniqueIdentifier);

            if (email == null)
            {
                ManagerLog.Error("ApiController.cs - SegnalaStatoAsync: UniqueIdentifier non esiste " + stato.UniqueIdentifier);

                //non blocco la cancellazione sul service, ma loggo
                await ApiUtility.SerializeAndSend(new MailFarms_SharedService.Response.ResponseBoolAvviso()
                {
                    Avviso = string.Empty,
                    Result = true
                }, Response).ConfigureAwait(false);

                return;
            }

            email.DataProssimoTentativo = stato.DataProssimoTentativo;
            email.DataUltimoTentativo = stato.DataProssimoTentativo;
            email.NumeroTentativi = stato.NumeroTentativi;
            email.Stato = (long)stato.Statoenum;
            email.StatusCode4xx5xx = stato.StatusCode4xx5xx;
            email.DataInvio = stato.DataInvio;

            if (!email.Save(out string avviso))
            {
                ManagerLog.Error("ApiController.cs - SegnalaStatoAsync: Email Save " + avviso);

                //non blocco la cancellazione sul service, ma loggo
                await ApiUtility.SerializeAndSend(new MailFarms_SharedService.Response.ResponseBoolAvviso()
                {
                    Avviso = string.Empty,
                    Result = true
                }, Response).ConfigureAwait(false);

                return;
            }

            var logRemoto = stato.Log;

            var logLocale = email.EmailLogCollection;

            foreach (var log in logRemoto)
            {
                if (logLocale.Any(p => p.UniqueIdentifier.Equals(log.UniqueIdentifier, StringComparison.Ordinal)))
                    continue;

                var nuovoLog = new EmailLog
                {
                    Email = email,
                    UniqueIdentifier = log.UniqueIdentifier,
                    Data = log.Data,
                    Testo = log.Testo
                };

                if (!nuovoLog.Save(out string avv))
                {
                    ManagerLog.Error("ApiController.cs - SegnalaStatoAsync: NuovoLog - Save " + avv);
                    continue;
                }
            }

            BrowserRefresh();

            if (email.Statoenum == Email.EmailStatoEnum.Inviata)
                ServerStatistiche.NuovaInviata(email.Server);

            if (email.Statoenum == Email.EmailStatoEnum.Errata)
                ServerStatistiche.NuovaErrata(email.Server);

            //non blocco la cancellazione sul service, ma loggo
            await ApiUtility.SerializeAndSend(new MailFarms_SharedService.Response.ResponseBoolAvviso()
            {
                Avviso = string.Empty,
                Result = true
            }, Response).ConfigureAwait(false);
        }

        #endregion

        #region Chiamate Web

        /// <summary>
        /// Chiamato da altri servizi esterni che vogliono inviare una nuova email
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task EmailNuovo()
        {
            var nuovaEmail = await ApiUtility.ReadArrayContent<EmailWeb>(Request).ConfigureAwait(false);

            if (nuovaEmail == null)
                return;

            if (nuovaEmail.UniqueIdentifier.IsNullOrEmpty())
                nuovaEmail.UniqueIdentifier = Guid.NewGuid().ToString();

            var email = Email.GetItem(nuovaEmail.UniqueIdentifier);

            //se per qualche motivo è già registrata
            if (email != null)
            {
                await ApiUtility.SerializeAndSend(new ResponseBoolAvviso()
                {
                    Avviso = string.Empty,
                    Result = true
                }, Response).ConfigureAwait(false);

                return;
            }

            email = new Email
            {
                MittenteEmail = nuovaEmail.MittenteEmail,
                Oggetto = nuovaEmail.Oggetto,
                Server = string.Empty,
                DataProssimoTentativo = DateTime.Now,
                StatusCode4xx5xx = string.Empty,
                DestinatarioEmail = nuovaEmail.DestinatarioEmail,
                DataArrivo = DateTime.Now,
                Contenuto = nuovaEmail.Contenuto,
                DataUltimoTentativo = DateTime.MinValue,
                DestinatarioDataRegistrazione = nuovaEmail.DestinatarioDataRegistrazione,
                DestinatarioNome = nuovaEmail.DestinatarioNome,
                MittenteNome = nuovaEmail.MittenteNome,
                Immediata = nuovaEmail.Immediata,
                RispondiA = nuovaEmail.RispondiA,
                Statoenum = Email.EmailStatoEnum.Coda,
                UniqueIdentifier = nuovaEmail.UniqueIdentifier,
                UrlEliminazione = nuovaEmail.UrlEliminazione ?? string.Empty,
                NumeroTentativi = 0,
            };

            var result = new ResponseBoolAvviso()
            {
                Avviso = string.Empty,
                Result = true
            };

            if (!email.Save(out string avviso))
            {
                result.Avviso = avviso;
                result.Result = false;

                ManagerLog.Error("ApiController.cs - EmailNuovo: " + avviso);

                await ApiUtility.SerializeAndSend(result, Response).ConfigureAwait(false);
                return;
            }

            var html = email.Contenuto;

            var hrefs = html.GetHref();

            foreach (var href in hrefs)
            {
                html = html.ReplaceInsensitive("href=\"" + href + "\"", "href=\"" + ManagerQueryString.GetEncodedLink(Settings.Config.WebPath + "/e/c?e=" + email.Id + "&u=" + HttpUtility.UrlEncode(href)) + "\"");
                html = html.ReplaceInsensitive("href=\'" + href + "\'", "href=\'" + ManagerQueryString.GetEncodedLink(Settings.Config.WebPath + "/e/c?e=" + email.Id + "&u=" + HttpUtility.UrlEncode(href)) + "\'");
            }

            if (!hrefs.Any())
                email.DataClick = DateTime.Parse("10/10/1900"); //la uso come indicatore se non ci sono link


            //lo inserisco alla fine perchè se la pagina non è caricata completamente
            //il metodo scrollTop di javascript non funziona
            var index = html.IndexOf("</body>", StringComparison.OrdinalIgnoreCase);

            if (index == -1)
                index = html.Length;

            html = html.Insert(index, "<img alt=\"\" height=\"1px\" width=\"1px\" src=\"" + ManagerQueryString.GetEncodedLink(Settings.Config.WebPath + "/e/p?e=" + email.Id) + "\">");

            email.Contenuto = html;
            email.Save();

            foreach (var allegati in nuovaEmail.Allegati)
            {
                avviso = await EmailAllegati.CaricaAllegato(email, allegati.NomeFile, allegati.Bytes);

                if (!avviso.IsNullOrEmpty())
                {
                    email.Delete();

                    ManagerLog.Error(Environment.StackTrace + ": " + avviso);

                    result.Avviso = avviso;
                    result.Result = false;

                    await ApiUtility.SerializeAndSend(result, Response).ConfigureAwait(false);
                    return;
                }
            }

            BrowserRefresh();

            await ApiUtility.SerializeAndSend(result, Response).ConfigureAwait(false);
        }


        /// <summary>
        /// Viene chiamato dai servizi esterni per sapere come si trova la mail (inviata, errata, in coda)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task EmailStato()
        {
            var guid = await ApiUtility.ReadStringContent(Request).ConfigureAwait(false);

            if (guid.IsNullOrEmpty())
                return;

            var email = Email.GetItem(guid);

            if (email == null)
            {
                ManagerLog.Error("ApiController.cs - EmailStato: UniqueIdentifier (" + guid + ") non è associato a nessuna email");

                await ApiUtility.SerializeAndSend(new EmailStato()
                {
                    Avviso = "UniqueIdentifier (" + guid + ") non è associato a nessuna email",
                }, Response).ConfigureAwait(false);

                return;
            }

            if (email.Statoenum == Email.EmailStatoEnum.Errata)
            {
                await ApiUtility.SerializeAndSend(new EmailStato
                {
                    DataVisualizzazione = email.DataVisualizzazione,
                    DataInvio = email.DataInvio,
                    DataClick = email.DataClick,
                    Errore = email.StatusCode4xx5xx
                }, Response).ConfigureAwait(false);

                return;
            }

            await ApiUtility.SerializeAndSend(new EmailStato
            {
                DataVisualizzazione = email.DataVisualizzazione,
                DataInvio = email.DataInvio,
                DataClick = email.DataClick,
                Errore = string.Empty
            }, Response).ConfigureAwait(false);
        }

        /// <summary>
        /// Chiamato da altri servizi esterni che vogliono inviare una nuova email
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task SmsNuovo()
        {
            var smsWeb = await ApiUtility.ReadArrayContent<SmsWeb>(Request).ConfigureAwait(false);

            if (smsWeb == null)
                return;

            var sms = Sms.GetItem(smsWeb.UniqueIdentifier);

            if (sms != null)
            {
                await ApiUtility.SerializeAndSend(new ResponseBoolAvviso()
                {
                    Avviso = string.Empty,
                    Result = true
                }, Response).ConfigureAwait(false);

                return;
            }

            sms = new Sms
            {
                Caratteri = smsWeb.Caratteri,
                Destinatario = smsWeb.Destinatario.Decode(),
                DataCoda = DateTime.Now,
                DataInvio = DateTime.MinValue,
                Mittente = smsWeb.Mittente.Decode(),
                Numero = smsWeb.Numero,
                NumeroMessaggi = smsWeb.NumeroMessaggi,
                Sistema = smsWeb.Sistema,
                Stato = 0,
                Testo = smsWeb.Testo.Decode(),
                UniqueIdentifier = smsWeb.UniqueIdentifier,
                MittenteSms = smsWeb.MittenteSms
            };

            var result = new ResponseBoolAvviso()
            {
                Avviso = string.Empty,
                Result = true
            };

            if (!Sms.Save(out string avviso, ref sms))
            {
                result.Avviso = avviso;
                result.Result = false;

                ManagerLog.Error("ApiController.cs - SmsNuovo: " + avviso);

                await ApiUtility.SerializeAndSend(result, Response).ConfigureAwait(false);
                return;
            }

            Engine.DistribuisciSms(); //non voglio aspettare

            BrowserRefresh();

            await ApiUtility.SerializeAndSend(result, Response).ConfigureAwait(false);
        }

        /// <summary>
        /// Viene chiamato dai servizi esterni per sapere come si trova la mail (inviata, errata, in coda)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task SmsStato()
        {
            var guid = await ApiUtility.ReadStringContent(Request).ConfigureAwait(false);

            if (guid.IsNullOrEmpty())
                return;

            var sms = Sms.GetItem(guid);

            if (sms == null)
            {
                ManagerLog.Error("ApiController.cs - SmsStato: UniqueIdentifier (" + guid + ") non è associato a nessun sms");

                await ApiUtility.SerializeAndSend(new SmsStato()
                {
                    Errore = string.Empty,
                    Avviso = "UniqueIdentifier (" + guid + ") non è associato a nessun sms",
                    Result = true,
                    Inviato = true,
                    DataInvio = DateTime.Now, //metto comunque una data, in questo modo evito che chi chiama caschi in loop, l'errore viene comunque loggato qua dentro
                }, Response).ConfigureAwait(false);

                return;
            }

            if (sms.Statoenum == Sms.SmsStatoEnum.Coda)
            {
                await ApiUtility.SerializeAndSend(new SmsStato
                {
                    Errore = sms.Errore,
                    DataInvio = DateTime.MinValue,
                    Inviato = false,
                    Result = true

                }, Response).ConfigureAwait(false);

                return;
            }

            await ApiUtility.SerializeAndSend(new SmsStato()
            {
                Errore = sms.Errore,
                DataInvio = sms.DataInvio,
                Inviato = true,
                Result = true

            }, Response).ConfigureAwait(false);
        }

        #endregion
    }
}