using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Business.Code;
using Business.Collection;
using Business.Entity;
using CommonNetCore.GlobalExtension;
using CommonNetCore.Misc;
using MailFarms_SharedService.Entity;

namespace MailFarmsBlazor.Code
{
    public static class Engine
    {
        private static int distribuisciLock;
        private static int inviaLock;
        private static int smsLock;

        public static readonly ConcurrentBag<Email> EmailDaInviare = new();

        public static void DistribuisciEmail()
        {
            Task.Run(async () =>
            {
                if (Interlocked.CompareExchange(ref distribuisciLock, 1, 0) == 1)
                    return;

                try
                {
                    if (EmailCollection.GetCount(wherePredicate: "Server == ''") == 0)
                        return;

                    var servers = ServerCollection.GetList(riceve: true).ToArray();

                    //ip, email inserite
                    var serverEmailCount = new Dictionary<string, long>();

                    foreach (var server in servers)
                    {
                        var ping = await MailFarms_SharedService.Code.RequestWindowsService.PingAsync(server.Ip).ConfigureAwait(false);

                        if (!ping)
                            continue;

                        var inCoda = EmailCollection.GetCount(wherePredicate: "Server == '" + server.Ip + "' AND Stato = 0");

                        serverEmailCount.Add(server.Ip, server.Inviate + inCoda);
                    }

                    if (!servers.Any())
                        return;

                    var emailDaInviare = EmailCollection.GetList(wherePredicate: "Server == ''", orderPredicate: "Immediata DESC");

                    foreach (var email in emailDaInviare)
                    {
                        foreach (var server in serverEmailCount.OrderBy(p => p.Value))
                        {
                            var srv = Server.GetItem(server.Key);

                            //se la mail è bannata sul server processo quella dopo
                            if (ServerDominiBannati.DominioBannato(srv, email.DestinatarioEmailDominio))
                                continue;

                            //adesso assegno il server alla mail
                            email.Server = server.Key;
                            Email.Save(email);

                            serverEmailCount[server.Key]++;

                            EmailDaInviare.Add(email);

                            break;
                        }
                    }

                }
                catch (Exception ex)
                {
                    ManagerLog.Error(ex, "Email.DistribuisciEmail() " + ex.Message);
                }
                finally
                {
                    Interlocked.Exchange(ref distribuisciLock, 0);
                }
            });
        }

        public static void InserisciInCoda()
        {
            var daInviare = EmailCollection.GetList(wherePredicate: "Server != '' AND Stato = 0");

            foreach (var email in daInviare)
                EmailDaInviare.Add(email);
        }

        public static void InviaEmail()
        {
            Task.Run(async () =>
            {
                if (Interlocked.CompareExchange(ref inviaLock, 1, 0) == 1)
                    return;

                try
                {
                    while (EmailDaInviare.TryTake(out Email email))
                    {
                        try
                        {
                            var emailService = new EmailService
                            {
                                MittenteEmail = email.MittenteEmail,
                                DestinatarioEmail = email.DestinatarioEmail,
                                Oggetto = email.Oggetto,
                                DestinatarioNome = email.DestinatarioNome,
                                MittenteNome = email.MittenteNome,
                                UniqueIdentifier = email.UniqueIdentifier,
                                UrlEliminazione = email.UrlEliminazione,
                                RispondiA = email.RispondiA,
                                Immediata = email.Immediata,
                                DestinatarioDataRegistrazione = email.DestinatarioDataRegistrazione,
                                Contenuto = email.Contenuto,
                            };

                            var allegatiSource = email.EmailAllegatiCollection;

                            var allegatiDest = new List<Allegati>(allegatiSource.Count);

                            foreach (var allegato in allegatiSource)
                            {
                                if (!File.Exists(allegato.PercorsoDisco))
                                    continue;

                                try
                                {
                                    var bytes = await File.ReadAllBytesAsync(allegato.PercorsoDisco).ConfigureAwait(false);

                                    bytes = await bytes.DecompressAsync().ConfigureAwait(false);

                                    allegatiDest.Add(new Allegati()
                                    {
                                        Bytes = bytes,
                                        NomeFile = allegato.NomeFile
                                    });
                                }
                                catch //nel caso ci siano problemi con il file
                                {

                                }
                            }

                            emailService.Allegati = allegatiDest.ToArray();

//#if RELEASE
                            //invio al windows service la mail da inviare
                            var serviceResult = await MailFarms_SharedService.Code.RequestWindowsService.NuovaEmail(emailService, email.Server).ConfigureAwait(false);

                            if (!serviceResult.Result)
                                ManagerLog.Error("Engine.InviaEmail() - NuovaEmail: " + serviceResult.Avviso);
//#endif

                        }
                        catch (Exception ex)
                        {
                            ManagerLog.Error(ex, "Engine.InviaEmail(), " + email.DestinatarioEmail + ", " + email.Server);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ManagerLog.Error(ex, "Engine.InviaEmail()");
                }
                finally
                {
                    Interlocked.Exchange(ref inviaLock, 0);
                }
            });
        }

        public static void DistribuisciSms()
        {
            Task.Run(() =>
            {
                if (Interlocked.CompareExchange(ref smsLock, 1, 0) == 1)
                    return;

                try
                {
                    var smss = SmsCollection.GetList(wherePredicate: "Stato = 0", orderPredicate: "DataCoda ASC");

                    for (var index = 0; index < smss.Count; index++)
                    {
                        var sms = smss[index];

                        if (!ManagerSkebby.InviaSMS(out string avviso, sms))
                            sms.Errore = avviso;

                        sms.DataInvio = DateTime.Now;
                        sms.Stato = 1;

                        if (!Sms.Save(out avviso, ref sms))
                            ManagerLog.Error("Sms.DistribuisciSms(): " + avviso);
                    }
                }
                catch (Exception ex)
                {
                    ManagerLog.Error(ex, "Sms.DistribuisciSms() " + ex.Message);
                }
                finally
                {
                    Interlocked.Exchange(ref smsLock, 0);
                }
            });
        }
    }
}
