using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Business.Collection;
using Business.Entity;
using CommonNetCore;
using CommonNetCore.GlobalExtension;
using MailFarms_SharedService.Code;
using MailFarms_SharedService.Entity;
using MailFarms_SharedService.Response;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SmtpRelayer
{
    /// <summary>
    /// Richieste esterne risponde a http://indirizzo/api
    /// </summary>
    public class ApiController : ControllerBase
    {
        [Inject]
        private AlertInjection AlertService { get; set; }

        public ApiController(AlertInjection alertService)
        {
            AlertService = alertService;
        }

        /// <summary>
        /// mailfarms.com, la web app, controlla se sono attivo
        /// </summary>
        [HttpGet]
        public string Ping()
        {
            return "true";
        }

        /// <summary>
        /// mailfarms.com, la web app, controlla se sono attivo
        /// </summary>
        [HttpGet]
        public string Info()
        {
            var codeBase = Assembly.GetExecutingAssembly().Location;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        #region FileLog

        /// <summary>
        /// ritorna a mailfarms.com l'elenco dei file di log
        /// </summary>
        [HttpPost]
        public async Task LogFileNameList()
        {
            var list = new List<string>();

            foreach (var file in Directory.EnumerateFiles(Settings.PathLog))
                list.Add(file.FileNameFromPath());

            list.Reverse();

            await ApiUtility.SerializeAndSend(list, Response).ConfigureAwait(false);
        }

        /// <summary>
        /// chiamato da mailfarms.com, ritorna il contenuto del file di log
        /// </summary>
        [HttpPost]
        public async Task LogFileNameContent()
        {
            var filename = await ApiUtility.ReadStringContent(Request).ConfigureAwait(false);

            var path = Settings.PathLog + "\\" + filename;

            if (!System.IO.File.Exists(path))
                return;

            var testo = await System.IO.File.ReadAllTextAsync(path).ConfigureAwait(false);

            await ApiUtility.SerializeAndSend(testo, Response).ConfigureAwait(false);
        }

        /// <summary>
        /// chiamato da mailfarms.com, elimina il singolo file di log
        /// </summary>
        [HttpPost]
        public async Task LogFileNameDelete()
        {
            var filename = await ApiUtility.ReadStringContent(Request).ConfigureAwait(false);

            var path = Settings.PathLog + "\\" + filename;

            if (!System.IO.File.Exists(path))
                return;

            System.IO.File.Delete(path);
        }

        /// <summary>
        /// chiamato da mailfarms.com, elimina tutti i file di log
        /// </summary>
        [HttpPost]
        public void LogFileNameDeleteAll()
        {
            foreach (var file in Directory.EnumerateFiles(Settings.PathLog))
                System.IO.File.Delete(file);
        }

        #endregion

        /// <summary>
        /// mailfarms.com, la web app, mi invia una nuova email da inviare
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task NuovaEmail()
        {
            var email = await ApiUtility.ReadArrayContent<EmailService>(Request).ConfigureAwait(false);

            var avviso = string.Empty;

            if (email.MittenteNome.IsNullOrEmpty())
                avviso += "Email.CheckEmail: Mittente è stato chiamato con un nome vuoto";

            if (!email.MittenteEmail.IsEmail())
                avviso += "Email.CheckEmail: Mittente è stato chiamato con una email non valida: " + email.MittenteEmail;

            if (!email.RispondiA.IsNullOrEmpty() && !email.RispondiA.IsEmail())
                avviso += "Email.CheckEmail: RispondiA è stato chiamato con una email non valida: " + email.RispondiA;

            if (email.DestinatarioNome.IsNullOrEmpty())
                avviso += "Email.CheckEmail: Destinatario è stato chiamato con un nome vuoto";

            if (!email.DestinatarioEmail.IsEmail())
                avviso += "Email.CheckEmail: Destinatario è stato chiamato con una email non valida: " + email.DestinatarioEmail;

            if (email.UniqueIdentifier.IsNullOrEmpty())
                avviso += "Email.CheckEmail: UniqueIdentifier è vuoto";

            if (email.Oggetto.IsNullOrEmpty())
                avviso += "Email.CheckEmail: L'oggetto è vuoto: " + email.DestinatarioEmail;

            if (email.Contenuto.IsNullOrEmpty())
                avviso += "Email.CheckEmail: Il contenuto è vuoto";

            if (email.Allegati != null && (email.Allegati.Any(p => p.Bytes == null || p.Bytes.Length == 0) || email.Allegati.Any(p => p.NomeFile.IsNullOrEmpty())))
                avviso += "Email.CheckEmail: Ci sono degli allegati con nome vuoto o dimensione zero";

            if (!avviso.IsNullOrEmpty())
            {
                await ApiUtility.SerializeAndSend(new ResponseBoolAvviso()
                {
                    Avviso = avviso,
                    Result = false
                }, Response).ConfigureAwait(false);

                return;
            }

            var nuovaEmail = new Email
            {
                RispondiA = email.RispondiA,
                Immediata = email.Immediata,
                DestinatarioEmail = email.DestinatarioEmail,
                DestinatarioNome = email.DestinatarioNome,
                DestinatarioDataRegistrazione = email.DestinatarioDataRegistrazione,
                MittenteEmail = email.MittenteEmail,
                MittenteNome = email.MittenteNome,
                Contenuto = email.Contenuto,
                Oggetto = email.Oggetto,
                UniqueIdentifier = email.UniqueIdentifier,
                DataArrivo = DateTime.Now,
                DataProssimoTentativo = DateTime.Now,
                DataUltimoTentativo = DateTime.Now,
                NumeroTentativi = 0,
                Stato = -1, //perchè gli allegati non erano pronti e partiva immediatamente
                UrlEliminazione = email.UrlEliminazione,
                StatusCode4xx5xx = string.Empty,
            };

            if (!nuovaEmail.Save(out avviso))
            {
                await ApiUtility.SerializeAndSend(new ResponseBoolAvviso()
                {
                    Avviso = avviso,
                    Result = false
                }, Response).ConfigureAwait(false);
            }

            if (email.Allegati != null)
            {
                foreach (var allegato in email.Allegati)
                {
                    if (!string.IsNullOrEmpty(await EmailAllegati.CaricaAllegato(nuovaEmail, allegato.NomeFile, allegato.Bytes)))
                    {
                        nuovaEmail.Delete();

                        await ApiUtility.SerializeAndSend(new ResponseBoolAvviso()
                        {
                            Avviso = avviso,
                            Result = false
                        }, Response).ConfigureAwait(false);

                        return;
                    }
                }
            }

            //perchè gli allegati non erano pronti e partiva immediatamente
            nuovaEmail.Stato = 0;
            nuovaEmail.Save();

            var valore = Impostazioni.GetItem(Impostazioni.ImpostazioniEnum.EmailTotaliDaInizio).Valore.ToLong();

            valore++;

            Impostazioni.Save(Impostazioni.ImpostazioniEnum.EmailTotaliDaInizio, valore.ToString());

            Relayer.AddEmail(nuovaEmail);

            //avviso che è arrivata una nuova email e di processarla immediatamente
            var action = AlertService.OnNewEmailCallback;
#pragma warning disable 4014
            Task.Run(() =>
#pragma warning restore 4014
            {
                action?.Invoke();
            });

            await ApiUtility.SerializeAndSend(new ResponseBoolAvviso()
            {
                Avviso = string.Empty,
                Result = true
            }, Response).ConfigureAwait(false);
        }

        /// <summary>
        /// mailfarms.com, la web app, mi invia la configurazione
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task AggiornaConfigurazione()
        {
            var tuple = await ApiUtility.ReadArrayContent<Tuple<string, string>>(Request).ConfigureAwait(false);

            if (!Enum.TryParse(tuple.Item1, out Impostazioni.ImpostazioniEnum chiave))
                return;

            var impostazione = Impostazioni.GetItem(chiave);

            impostazione.Valore = tuple.Item2;

            Impostazioni.Save(impostazione);
        }

        /// <summary>
        /// mailfarms.com, la web app, mi chiede quante email ho in coda
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task InCoda()
        {
            await ApiUtility.SerializeAndSend(EmailCollection.GetCount(wherePredicate: "Stato == 0"), Response).ConfigureAwait(false);
        }

        /// <summary>
        /// mailfarms.com, attivo o meno il servizio nel processare o meno la mail
        /// </summary>
        [HttpPost]
        public async Task Attivo()
        {
            var request = await ApiUtility.ReadStringContent(Request).ConfigureAwait(false);

            Worker.Attivo = request.ToBool();
        }

        /// <summary>
        /// chiamata a mano, attiva o meno la traccia
        /// </summary>
        [HttpGet]
        public async Task List()
        {
            var sb = new StringBuilder();

            sb.Append("<style>body{font-size:10px; font-family:tahoma} table { font-size:10px; } table tr, table td { text-align:left; }</style>");

            foreach (var runner in Program.DominiRunner)
                sb.Append(runner.Key + " (" + runner.Value.EmailDaInviare.Count + ")<br>");

            foreach (var runner in Program.DominiRunner)
            {
                sb.Append("<table style=\"width:100%\">");

                sb.Append("<tr>");

                sb.Append("<td style=\"vertical-align:top\">" + runner.Key + " (" + runner.Value.EmailDaInviare.Count + ")</td>");

                sb.Append("<td><table style=\"width:100%\">");

                foreach (var keyValue in runner.Value.EmailDaInviare.OrderByDescending(p => p.Value.Immediata).ThenBy(p => p.Value.DataProssimoTentativo))
                {
                    var email = keyValue.Value;

                    sb.Append("<tr>" +
                              "<th style=\"width:250px\">Uniq.</th>" +
                              "<th style=\"width:200px\">Mitt.</th>" +
                              "<th style=\"width:200px\">Dest.</th>" +
                              "<th style=\"width:200px\">Ult.Tent.</th>" +
                              "<th>Pross.Tent.</th>" +
                              "</tr>");

                    sb.Append("<tr>");

                    sb.Append("<td>" + email.UniqueIdentifier + "</td>");
                    sb.Append("<td>" + email.MittenteEmail + "</td>");
                    sb.Append("<td>" + email.DestinatarioEmail + "</td>");
                    sb.Append("<td>" + email.DataUltimoTentativo + "</td>");
                    sb.Append("<td>" + email.DataProssimoTentativo + "</td>");

                    sb.Append("</tr>");
                }

                sb.Append("</table></td>");

                sb.Append("</tr>");

                sb.Append("</table>");
            }

            await ApiUtility.SerializeAndSend(sb.ToString(), Response).ConfigureAwait(false);
        }

        /// <summary>
        /// chiamata a mano, attiva o meno la traccia
        /// </summary>
        [HttpGet]
        public async Task Trace()
        {
            Worker.Trace = !Worker.Trace;

            await ApiUtility.SerializeAndSend(Worker.Trace, Response).ConfigureAwait(false);
        }

        [HttpPost]
        public async Task AggiornaIndirizzoIp()
        {
            var ip = await ApiUtility.ReadStringContent(Request).ConfigureAwait(false);

            var response = new ResponseBoolAvviso();

            response.Result = Impostazioni.Save(Impostazioni.ImpostazioniEnum.IndirizzoIp, ip);

            if (!response.Result)
                response.Avviso = "E' avvenuto un errore durante il salvataggio";

            await ApiUtility.SerializeAndSend(response, Response).ConfigureAwait(false);
        }
    }
}
