using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Business.Collection;
using Business.Entity;
using CommonNetCore;
using CommonNetCore.GlobalExtension;
using CommonNetCore.Misc;
using MailFarms_SharedService.Code;
using MailFarms_SharedService.Entity;
using Microsoft.Extensions.Hosting;
using SmtpRelayer.Smtp;

namespace SmtpRelayer
{
    public class Worker : BackgroundService
    {
        //[Inject]
        //private AlertInjection AlertService { get; set; }

        public Worker()
        {
            //AlertService = alertService;

            ////quando arrivano delle email le invio immediatamente, viene chiamato dal controller ApiController
            //AlertService.OnNewEmailCallback = () =>
            //{
            //    Task.Run(SendEmail);
            //};

#if DEBUG
            Settings.Config.Database.Path = @"C:\Progetti\Web\MailFarms\MailFarms_WindowsService\SmtpRelayer\Public\Database\Database.db3";
#endif

            ManagerLog.Warn("Il mio IP: " + Impostazioni.GetValore(Impostazioni.ImpostazioniEnum.IndirizzoIp));

#if DEBUG
            Attivo = true;
#endif

            ManagerLog.Warn("Attivo: " + Attivo);
        }

        private static DateTime _ultimoAggiornamento = DateTime.MinValue;

        /// <summary>
        /// Solo se questa variabile è attiva, viene processata la coda
        /// </summary>
        public static bool Attivo = false;

        /// <summary>
        /// Impostata a mano tramite get, se è attiva traccia i movimenti
        /// </summary>
        public static bool Trace = false;

        /// <summary>
        /// Mette in processo le email che sono in coda all'avvio
        /// </summary>
        private static readonly Timer TimerServizioAttivo = new Timer(async _ =>
        {
            try
            {
                Attivo = await RequestWebApp.GetAttivo(Impostazioni.GetValore(Impostazioni.ImpostazioniEnum.IndirizzoIp));
            }
#if !DEBUG
            catch (Exception ex)
            {
                ManagerLog.Error(ex, "TimerServizioAttivo");
            }
#endif            

        }, null, 1000, 5000);


        static object incodatoreStatus = new object();

        /// <summary>
        /// Mette in processo le email che sono in coda all'avvio
        /// </summary>
        private static readonly Timer TimerIncodatore = new Timer(_ =>
        {
            if (!Monitor.TryEnter(incodatoreStatus))
                return;

            try
            {
                var emailCoda = EmailCollection.GetList(wherePredicate: "Stato = 0");

                foreach (var email in emailCoda)
                    Relayer.AddEmail(email);
            }
#if !DEBUG
                catch (Exception ex)
                {
                    ManagerLog.Error(ex, "TimerIncodatore");
                }
#endif
            finally
            {
                Monitor.Exit(incodatoreStatus);
            }

        }, null, 0, 500);

        static int aggioratoreStatus;

        private static readonly Timer TimerAggiornatore = new Timer(_ =>
        {
            Task.Run(async () =>
            {
                if (Interlocked.CompareExchange(ref aggioratoreStatus, 1, 0) == 1)
                    return;

                try
                {
                    //se mailfarms è giu per qualche motivo
                    if (!await RequestWebApp.Ping().ConfigureAwait(false))
                        return;

                    var dataAggiornamento = DateTime.Now.AddMinutes(-10);

                    var updateTime = false;

                    var emailDaSegnalare = EmailCollection.GetList(wherePredicate: "Stato != -1 AND DataUltimoTentativo >= " + _ultimoAggiornamento);

                    foreach (var email in emailDaSegnalare)
                    {
                        updateTime = true;

                        var stato = new Stato
                        {
                            UniqueIdentifier = email.UniqueIdentifier,
                            StatusCode4xx5xx = email.StatusCode4xx5xx,
                            DataUltimoTentativo = email.DataUltimoTentativo,
                            DataProssimoTentativo = email.DataProssimoTentativo,
                            NumeroTentativi = email.NumeroTentativi,
                            Statoenum = email.Statoenum,
                            Log = email.Log.Select(p => new StatoLog
                            {
                                UniqueIdentifier = p.UniqueIdentifier,
                                Data = p.Data,
                                Testo = p.Testo
                            }).ToArray(),
                            DataInvio = email.Statoenum == Stato.StatoEnum.Inviata ? email.DataUltimoTentativo : DateTime.MinValue
                        };

                        var response = await RequestWebApp.SegnalaStato(stato).ConfigureAwait(false);

                        //se per qualche motivo mi risponde male interrompo
                        if (!response.Result)
                            return;

                        //se inviata o errata la elimino
                        if (email.Statoenum != Stato.StatoEnum.Coda)
                        {
                            if (!email.Delete(out string avviso))
                                ManagerLog.Error(Environment.StackTrace + " - EmailDelete: " + avviso);
                        }
                    }

                    if (updateTime)
                        _ultimoAggiornamento = dataAggiornamento;
                }
#if !DEBUG
                catch (Exception ex)
                {
                    ManagerLog.Error(ex, "TimerAggiornatore");
                }
#endif
                finally
                {
                    Interlocked.Exchange(ref aggioratoreStatus, 0);
                }
            });

        }, null, 0, 500);

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ManagerLog.Warn("Servizio avviato");

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            ManagerLog.Warn("Servizio fermato");

            return Task.CompletedTask;
        }
    }
}
