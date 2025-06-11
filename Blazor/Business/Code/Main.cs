using Business.Collection;
using Business.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Business.Code
{

    ///// <summary>
    /////     SERVER: Questa classe risponde al client o invia risposte ai client connessi
    ///// </summary>
    //public static class Main
    //{
    //    private static readonly object LockObject = new object();

    //    /// <summary>
    //    /// Blocca l'invio delle email di un dominio particolare, es.: tiscali risponde rallenta, blocco tutte le mail che vanno a tiscali per un po'
    //    /// Questo array contiene tutti i messaggi che sono inviati dal server quando ci avvisa di rallentare
    //    /// </summary>
    //    private static string[] TemporaryMessage = new string[0];

    //    /// <summary>
    //    /// Ci sono degli errori temporanei che non sono 404 ma sono come i 404, ovvero errori temporanei di cui vale la pena
    //    /// riprovare l'invio, questo array contiene tutti questi errori
    //    /// </summary>
    //    private static string[] ErrorTo404 = new string[0];

    //    //i domini temporaneamente bloccati
    //    private static readonly Dictionary<string, DateTime> TemporaryLocked = new Dictionary<string, DateTime>(StringComparer.Ordinal);

    //    public static void Start()
    //    {
    //        ApplicationStart.Initialize();

    //        #region Se non esiste il database, lo creo

    //        if (!File.Exists(Settings.Config.Database.Path))
    //        {
    //            //in debug costruisco il ogni volta
    //            var fileQuery = Settings.AssemblyDirectory + "\\SQLBuildDatabase.txt";

    //            if (!File.Exists(fileQuery))
    //            {
    //                ManagerLog.Error("Il file '" + fileQuery + "' non esiste");
    //                return;
    //            }

    //            var fi = new FileInfo(Common.Settings.Config.Database.Path);

    //            Directory.CreateDirectory(fi.Directory.FullName);

    //            var query = File.ReadAllText(fileQuery);

    //            ManagerConnection.ExecuteCommand(query);
    //        }

    //        #endregion

    //        #region Recupero i messaggi di errore che bloccano il dominio temporaneamente

    //        var fileTemporaryBlock = Settings.AssemblyDirectory + "\\TemporaryBlock.txt";

    //        //costruisco se assente
    //        if (!File.Exists(fileTemporaryBlock))
    //            File.WriteAllText(fileTemporaryBlock, string.Empty);

    //        TemporaryMessage = File.ReadAllLines(fileTemporaryBlock);

    //        if (TemporaryMessage.Any())
    //            ManagerLog.Warn("Letti " + TemporaryMessage.Length + " avvisi di blocco temporaneo dominio: " + string.Join(", ", TemporaryMessage));

    //        #endregion

    //        #region Recupero i messaggi di errore che sono temporanei e meritano altri tentativi

    //        var file404Block = Settings.AssemblyDirectory + "\\ErrorTo404.txt";

    //        //costruisco se assente
    //        if (!File.Exists(file404Block))
    //            File.WriteAllText(file404Block, string.Empty);

    //        ErrorTo404 = File.ReadAllLines(file404Block);

    //        if (ErrorTo404.Any())
    //            ManagerLog.Warn("Letti " + ErrorTo404.Length + " avvisi di blocco temporaneo email: " + string.Join(", ", ErrorTo404));

    //        #endregion

    //        ApplicationStart.TempEnabled = false;

    //        //Gestione database e cron
    //        ApplicationStart.Start();

    //        try
    //        {
    //            //Avvio il server OWIN per le chiamate remote webapi
    //            WebApp.Start<WebApiStartup>(url: "http://*:1100/");
    //        }
    //        catch
    //        {
    //            var username = Environment.GetEnvironmentVariable("USERNAME");
    //            var userdomain = Environment.GetEnvironmentVariable("USERDOMAIN");

    //            var err = "You need to run the following command: netsh http add urlacl url=http://*:1100/ user=" + userdomain + "\\" + username + " listen=yes";
    //            Console.WriteLine(err);
    //            ManagerLog.Error(err);
    //            throw;
    //        }

    //        var tempoAttesa = 1000 * 3; //3 secondi

    //        var timer = new Timer(new TimerCallback(ControllaCoda));
    //        timer.Change(0, tempoAttesa);
    //    }

    //    public static void Stop()
    //    {

    //    }

    //    private static void ControllaCoda(object state)
    //    {
    //        if (Monitor.TryEnter(LockObject))
    //        {
    //            try
    //            {
    //                var numeroMassimoTentativi = Impostazioni.GetValore(ImpostazioniEnum.EmailNumeroMassimoTentativi).ToInt();

    //                if (numeroMassimoTentativi < 1)
    //                    numeroMassimoTentativi = 1;

    //                var contatore = 0;

    //                var emailCoda = EmailCollection.GetList(wherePredicate: "Stato = 0 AND DataProssimoTentativo < " + DateTime.Now, orderPredicate: "Immediata DESC, DataProssimoTentativo ASC");

    //                foreach (var email in emailCoda)
    //                {
    //                    var dominioEmail = SmtpRelayerApi.Email.GetDomain(email.DestinatarioEmail);

    //                    if (TemporaryLocked.TryGetValue(dominioEmail, out DateTime dataBlocco))
    //                    {
    //                        if (dataBlocco < DateTime.Now)
    //                        {
    //                            //il blocco del dominio è scaduto, posso inviargli email
    //                            TemporaryLocked.Remove(dominioEmail);
    //                        }
    //                        else
    //                        {
    //                            //il dominio è ancora bloccato
    //                            continue;
    //                        }
    //                    }

    //                    Console.WriteLine(email.DestinatarioEmail + ": Inizio l'invio...");

    //                    var emailInviata = Sender.InviaEmail(out var avviso, Sender.PreparaMail(email));

    //                    //prendo la nuova email perchè il campo email.UltimoStatusCode è stato aggiornato dal logger
    //                    //e se salvo direttamente email del foreach lo perdo

    //                    var updatedEmail = Email.GetItem(email.Id);

    //                    updatedEmail.NumeroTentativi++;
    //                    updatedEmail.DataUltimoTentativo = DateTime.Now;

    //                    if (!emailInviata)
    //                    {
    //                        //se è un errore lo loggo
    //                        if (!avviso.IsNullOrEmpty())
    //                        {
    //                            var log = new EmailLog
    //                            {
    //                                Data = DateTime.Now,
    //                                Email = updatedEmail,
    //                                Testo = avviso
    //                            };

    //                            if (!log.Save(out string avvisoInt))
    //                                ManagerLog.Warn("Main.cs - Log.Save - '" + avvisoInt + "'");
    //                        }

    //                        //se non si collegato o non ci sono MX o altri errori non legati ai comandi SMTP
    //                        if (updatedEmail.StatusCode4xx5xx.IsNullOrEmpty())
    //                            updatedEmail.StatusCode4xx5xx = avviso;

    //                        var isLocked = false;

    //                        //controllo se la risposta dell'smtp è dentro una delle frasi preimpostate
    //                        //blocco l'invio di tutte le mail a questo dominio per un pò
    //                        foreach (var message in TemporaryMessage)
    //                        {
    //                            if (updatedEmail.StatusCode4xx5xx.IndexOf(message, StringComparison.OrdinalIgnoreCase) != -1 ||
    //                                avviso.IndexOf(message, StringComparison.OrdinalIgnoreCase) != -1)
    //                            {
    //                                if (!TemporaryLocked.TryGetValue(dominioEmail, out DateTime dataRegistrata))
    //                                {
    //                                    var dataSblocco = DateTime.Now.AddMinutes(Impostazioni.GetValore(ImpostazioniEnum.DominioTentativoSuccessivoMinuti).ToInt());

    //                                    if (updatedEmail.StatusCode4xx5xx.IndexOf(message, StringComparison.OrdinalIgnoreCase) != -1)
    //                                        ManagerLog.Warn("L'smtp '" + dominioEmail + "' mi ha detto '" + updatedEmail.StatusCode4xx5xx + "' che contiene la frase '" + message + "', blocco tutte le mail del dominio fino a " + dataSblocco);
    //                                    else
    //                                        ManagerLog.Warn("L'smtp '" + dominioEmail + "' mi ha detto '" + avviso + "' che contiene la frase '" + message + "', blocco tutte le mail del dominio fino a " + dataSblocco);

    //                                    TemporaryLocked.Add(dominioEmail, dataSblocco);
    //                                }
    //                                else
    //                                {
    //                                    ManagerLog.Warn("L'smtp '" + dominioEmail + "' mi ha detto '" + updatedEmail.StatusCode4xx5xx + "' che contiene la frase '" + message + "', il dominio è già inserito nella blacklist fino a " + dataRegistrata);
    //                                }

    //                                isLocked = true;

    //                                break;
    //                            }
    //                        }

    //                        if (isLocked)
    //                            continue;

    //                        var is404 = updatedEmail.StatusCode4xx5xx.StartsWith("4");

    //                        //se è un errore 500 ma in realtà dovrei comunque riprovare, controllo se l'avviso dice così
    //                        if (!is404)
    //                        {
    //                            foreach (var message in ErrorTo404)
    //                            {
    //                                if (updatedEmail.StatusCode4xx5xx.IndexOf(message, StringComparison.OrdinalIgnoreCase) == -1 &&
    //                                    avviso.IndexOf(message, StringComparison.OrdinalIgnoreCase) == -1)
    //                                    continue;

    //                                is404 = true;
    //                                break;
    //                            }
    //                        }

    //                        if (updatedEmail.NumeroTentativi < numeroMassimoTentativi && is404)
    //                        {
    //                            updatedEmail.DataProssimoTentativo = DateTime.Now.AddMinutes(Impostazioni.GetValore(ImpostazioniEnum.EmailTentativoSuccessivoMinuti).ToInt());
    //                        }
    //                        else
    //                        {
    //                            updatedEmail.Statoenum = SmtpRelayerApi.Email.EmailStatoEnum.Errata;
    //                        }
    //                    }
    //                    else
    //                    {
    //                        contatore++;
    //                        updatedEmail.Statoenum = SmtpRelayerApi.Email.EmailStatoEnum.Inviata;
    //                    }

    //                    if (!updatedEmail.Save(out avviso))
    //                        ManagerLog.Error("Main.ControllaCoda: " + avviso);
    //                }

    //                if (contatore > 0)
    //                    ManagerLog.Warn("Email inviate: " + contatore);

    //                contatore = 0;

    //                //cancello le email messe in coda più del tempo stabilito
    //                emailCoda = EmailCollection.GetList(wherePredicate: "DataUltimoTentativo < " + DateTime.Now.AddDays(-Impostazioni.GetValore(ImpostazioniEnum.ScadenzaGiorni).ToInt()), orderPredicate: "DataProssimoTentativo ASC");

    //                foreach (var email in emailCoda)
    //                {
    //                    if (!email.Delete(out string avviso))
    //                    {
    //                        ManagerLog.Error(avviso);
    //                        continue;
    //                    }

    //                    contatore++;
    //                }

    //                if (contatore > 0)
    //                    ManagerLog.Warn("Email vecchie eliminate: " + contatore);
    //            }
    //            finally
    //            {
    //                Monitor.Exit(LockObject);
    //            }
    //        }
    //    }
    //}
}