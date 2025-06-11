using System;
using Business.Entity;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommonNetCore.GlobalExtension;
using CommonNetCore.Misc;
using MailFarms_SharedService.Entity;
using SmtpRelayer.Smtp;
using System.Collections.Concurrent;

namespace SmtpRelayer
{
    public class Runner
    {
        private DateTime dataBlocco = DateTime.MinValue;

        public readonly ConcurrentDictionary<string, Email> EmailDaInviare = new(StringComparer.Ordinal);

        public bool Running => EmailDaInviare.Any();

        public CancellationTokenSource CancellationTokenSource = new();

        readonly Stopwatch _sw = new();

        public string StatoStr(Email email, string str)
        {
            if (email == null || EmailDaInviare == null || _sw == null)
                return "(email null) " + str;

            return "(" + email.DestinatarioEmail + ", tot: " + EmailDaInviare.Count + ", ms.: " + _sw.ElapsedMilliseconds + ") " + str;
        }

        public Runner(string domain)
        {
            var numeroMassimoTentativi = Impostazioni.GetValore(Impostazioni.ImpostazioniEnum.EmailNumeroMassimoTentativi).ToInt();

            if (numeroMassimoTentativi < 1)
                numeroMassimoTentativi = 1;

            Task.Run(async () =>
            {
                while (!CancellationTokenSource.IsCancellationRequested)
                {
                    Program.Trace("principale, dominio: " + domain + ", attivo: " + Worker.Attivo);

                    while (Worker.Attivo && EmailDaInviare.Any())
                    {
                        try
                        {
                            Program.Trace("interno, dominio: " + domain + ", tot: " + EmailDaInviare.Count);

                            _sw.Restart();

                            var keyValue = EmailDaInviare.OrderByDescending(p => p.Value.Immediata).ThenBy(p => p.Value.DataProssimoTentativo).FirstOrDefault();

                            var email = keyValue.Value;

                            email = Email.GetItem(email.Id);

                            if (email is not { Statoenum: Stato.StatoEnum.Coda })
                            {
                                Program.Trace(StatoStr(email, "rimuovo la mail"));
                                EmailDaInviare.TryRemove(keyValue.Key, out _);
                                continue;
                            }

                            Program.Trace(StatoStr(email, "processo la mail"));

                            var now = DateTime.Now;

                            string avviso;

                            if (email.DataProssimoTentativo > now)
                            {
                                email.DataUltimoTentativo = now;

                                if (!email.Save(out avviso))
                                    ManagerLog.Error("Runner(" + email.Id + "): " + avviso);

                                EmailDaInviare[email.UniqueIdentifier] = email; //per aggiornare la data prossimo tentativo che serve per l'ordinamento

                                Program.Trace(StatoStr(email, "email bloccata fino al " + email.DataProssimoTentativo));
                                break; //aspetto un secondo
                            }

                            if (dataBlocco > now)
                            {
                                email.DataProssimoTentativo = dataBlocco;
                                email.DataUltimoTentativo = now;

                                if (!email.Save(out avviso))
                                    ManagerLog.Error("Runner(" + email.Id + "): " + avviso);

                                EmailDaInviare[email.UniqueIdentifier] = email; //per aggiornare la data prossimo tentativo che serve per l'ordinamento

                                Program.Trace(StatoStr(email, "dominio bloccato fino al " + dataBlocco));

                                //il dominio è ancora bloccato passo alla mail dopo che ha sempre lo stesso dominio ma aggiorno la sua data di prossimo tentativo
                                continue;
                            }

                            email.DataUltimoTentativo = now;
                            email.NumeroTentativi++;

                            if (!email.Save(out avviso))
                                ManagerLog.Error("Runner(" + email.Id + "): " + avviso);

                            Program.Trace(StatoStr(email, "invio la mail"));

                            var task = Sender.InviaEmail(email, domain);

                            using var timeoutCancellationTokenSource = new CancellationTokenSource();

                            //aspetto massimo due minuti
                            var completedTask = await Task.WhenAny(task, Task.Delay(TimeSpan.FromMinutes(2), timeoutCancellationTokenSource.Token)).ConfigureAwait(false);

                            if (completedTask != task)
                            {
                                email.DataProssimoTentativo = now.AddMinutes(Impostazioni.GetValore(Impostazioni.ImpostazioniEnum.EmailTentativoSuccessivoMinuti).ToInt());

                                if (!email.Save(out avviso))
                                    ManagerLog.Error("Runner(" + email.Id + "): " + avviso);

                                EmailDaInviare[email.UniqueIdentifier] = email; //per aggiornare la data prossimo tentativo che serve per l'ordinamento

                                Program.Trace(StatoStr(email, "Tentativo andato in timeout"));

                                //il dominio è ancora bloccato passo alla mail dopo che ha sempre lo stesso dominio ma aggiorno la sua data di prossimo tentativo
                                continue;
                            }

                            timeoutCancellationTokenSource.Cancel();

                            var tuple = await task.ConfigureAwait(false);

                            Program.Trace(StatoStr(email, "send concluso"));

                            avviso = tuple.Item2;

                            //prendo la nuova email perchè il campo email.UltimoStatusCode è stato aggiornato dal logger
                            email = Email.GetItem(email.Id);

                            if (!tuple.Item1)
                            {
                                Program.Trace(StatoStr(email, "mail non inviata"));

                                //se non c'è nessun log
                                if (!email.Log.Any())
                                {
                                    var log = new EmailLog
                                    {
                                        UniqueIdentifier = Guid.NewGuid().ToString(),
                                        Data = now,
                                        Email = email,
                                        Testo = !avviso.IsNullOrEmpty() ? avviso : "Impossibile inviare la mail"
                                    };

                                    if (!EmailLog.Save(out string avvisoInt, ref log))
                                        ManagerLog.Warn("Main.cs - Log.Save - '" + avvisoInt + "'");
                                }

                                //se non si collegato o non ci sono MX o altri errori non legati ai comandi SMTP
                                if (email.StatusCode4xx5xx.IsNullOrEmpty())
                                    email.StatusCode4xx5xx = avviso;

                                var isLocked = false;

                                //controllo se la risposta dell'smtp è dentro una delle frasi preimpostate
                                //blocco l'invio di tutte le mail a questo dominio per un pò
                                foreach (var message in Program._temporaryMessage)
                                {
                                    if (email.StatusCode4xx5xx.IndexOf(message, StringComparison.OrdinalIgnoreCase) != -1 || avviso.IndexOf(message, StringComparison.OrdinalIgnoreCase) != -1)
                                    {
                                        dataBlocco = now.AddMinutes(Impostazioni.GetValore(Impostazioni.ImpostazioniEnum.DominioTentativoSuccessivoMinuti).ToInt());

                                        if (email.StatusCode4xx5xx.IndexOf(message, StringComparison.OrdinalIgnoreCase) != -1)
                                            ManagerLog.Warn("Per " + email.DestinatarioEmail + ",  l'smtp mi ha risposto '" + email.StatusCode4xx5xx + "' che contiene la frase '" + message + "', blocco tutte le mail del dominio fino a " + dataBlocco);
                                        else
                                            ManagerLog.Warn("Per " + email.DestinatarioEmail + ",  l'smtp mi ha risposto '" + avviso + "' che contiene la frase '" + message + "', blocco tutte le mail del dominio fino a " + dataBlocco);

                                        isLocked = true;

                                        break;
                                    }
                                }

                                if (isLocked)
                                {
                                    email.DataProssimoTentativo = now.AddMinutes(Impostazioni.GetValore(Impostazioni.ImpostazioniEnum.EmailTentativoSuccessivoMinuti).ToInt());

                                    EmailDaInviare[email.UniqueIdentifier] = email; //per aggiornare la data prossimo tentativo che serve per l'ordinamento

                                    Program.Trace(StatoStr(email, "posticipo invio 1, " + email.DataProssimoTentativo));

                                    if (!email.Save(out avviso))
                                        ManagerLog.Error("Main.ControllaCoda: " + avviso);

                                    break;
                                }

                                var is404 = email.StatusCode4xx5xx.StartsWith("4");

                                if (is404)
                                {
                                    //se è un errore 400 ma non ha senso riprovare
                                    foreach (var message in Program._4xxTo5xx)
                                    {
                                        if (email.StatusCode4xx5xx.IndexOf(message, StringComparison.OrdinalIgnoreCase) == -1 && avviso.IndexOf(message, StringComparison.OrdinalIgnoreCase) == -1)
                                            continue;

                                        is404 = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    //se è un errore 500 ma in realtà dovrei comunque riprovare, controllo se l'avviso dice così
                                    foreach (var message in Program._5xxTo4xx)
                                    {
                                        if (email.StatusCode4xx5xx.IndexOf(message, StringComparison.OrdinalIgnoreCase) == -1 && avviso.IndexOf(message, StringComparison.OrdinalIgnoreCase) == -1)
                                            continue;

                                        is404 = true;
                                        break;
                                    }
                                }


                                if (email.NumeroTentativi >= numeroMassimoTentativi || !is404)
                                {
                                    email.Statoenum = Stato.StatoEnum.Errata;

                                    ManagerLog.Trace(StatoStr(email, "email errata"));
                                }
                                else
                                {
                                    email.DataProssimoTentativo = now.AddMinutes(Impostazioni.GetValore(Impostazioni.ImpostazioniEnum.EmailTentativoSuccessivoMinuti).ToInt());

                                    EmailDaInviare[email.UniqueIdentifier] = email; //per aggiornare la data prossimo tentativo che serve per l'ordinamento

                                    ManagerLog.Trace(StatoStr(email, "posticipo invio 2, " + email.DataProssimoTentativo));
                                }
                            }
                            else
                            {
                                email.StatusCode4xx5xx = string.Empty;
                                email.Statoenum = Stato.StatoEnum.Inviata;

                                ManagerLog.Trace(StatoStr(email, "inviata"));
                            }

                            if (!email.Save(out avviso))
                                ManagerLog.Error("Main.ControllaCoda: " + avviso);
                        }
                        catch (Exception ex)
                        {
                            ManagerLog.Error(ex, "interno, dominio");
                        }
                    }

                    //evito 100% cpu
                    await Task.Delay(1000, CancellationTokenSource.Token).ConfigureAwait(false); //aspetto 1 s
                }
            }, CancellationTokenSource.Token);
        }

        public void NuovaEmail(Email email)
        {
            if (email == null)
                return;

            if (EmailDaInviare.ContainsKey(email.UniqueIdentifier))
                return;

            EmailDaInviare.TryAdd(email.UniqueIdentifier, email);

            Program.Trace("NuovaEmail, mittente " + email.MittenteEmail + ", destinatario " + email.DestinatarioEmail + ", " + email.UniqueIdentifier + ",  tot: " + EmailDaInviare.Count + ", CancellationRequest: " + CancellationTokenSource.Token.IsCancellationRequested);
        }
    }
}
