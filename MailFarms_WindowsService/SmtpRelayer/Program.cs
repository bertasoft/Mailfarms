using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CommonNetCore;
using CommonNetCore.Database;
using CommonNetCore.Misc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.EventLog;

namespace SmtpRelayer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Imposto la cultura per la formattazione dei decimali sui numeri e per le date
            var culture = CultureInfo.CreateSpecificCulture("IT-it");

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            //dove gira il windows service, altrimenti mi prende c:\windows\system32
            var path = Settings.AssemblyDirectory;

            ApplicationStart.Initialize(path);

            #region Se non esiste il database, lo creo

            if (!File.Exists(Settings.Config.Database.Path))
            {
                var fileQuery = Settings.AssemblyDirectory + "\\SQLBuildDatabase.txt";

                if (!File.Exists(fileQuery))
                {
                    ManagerLog.Error("Il file '" + fileQuery + "' non esiste");
                    return;
                }

                var fi = new FileInfo(Settings.Config.Database.Path);

                Directory.CreateDirectory(fi.Directory.FullName);

                var query = File.ReadAllText(fileQuery);

                ManagerConnection.ExecuteCommand(query);
            }

            #endregion

            #region Recupero i messaggi di errore che bloccano il dominio temporaneamente

            var fileTemporaryBlock = Settings.AssemblyDirectory + "\\TemporaryBlock.txt";

            //costruisco se assente
            if (!File.Exists(fileTemporaryBlock))
                ManagerLog.Error("Non trovo il file " + fileTemporaryBlock);
            else
            {
                _temporaryMessage = File.ReadAllLines(fileTemporaryBlock);

                if (_temporaryMessage.Any())
                    ManagerLog.Warn("Letti " + _temporaryMessage.Length + " avvisi di blocco temporaneo dominio: " + string.Join(", ", _temporaryMessage));
            }

            #endregion

            #region Recupero i messaggi di errore che meritano altri tentativi

            var file5xxTo4xx_txt = Settings.AssemblyDirectory + "\\5xxTo4xx.txt";

            if (!File.Exists(file5xxTo4xx_txt))
            {
                ManagerLog.Error("Non trovo il file " + file5xxTo4xx_txt);
            }
            else
            {
                _5xxTo4xx = File.ReadAllLines(file5xxTo4xx_txt);

                if (_5xxTo4xx.Any())
                    ManagerLog.Warn("Letti " + _5xxTo4xx.Length + " errori 5xx da trasformare in ritentativi: " + string.Join(", ", _5xxTo4xx));
            }

            #endregion

            #region Recupero i messaggi di avviso che indicano che non ha senso ritentare

            var file4xxTo5xx_txt = Settings.AssemblyDirectory + "\\4xxTo5xx.txt";

            if (!File.Exists(file4xxTo5xx_txt))
            {
                ManagerLog.Error("Non trovo il file " + file4xxTo5xx_txt);
            }
            else
            {
                _4xxTo5xx = File.ReadAllLines(file4xxTo5xx_txt);

                if (_4xxTo5xx.Any())
                    ManagerLog.Warn("Letti " + _4xxTo5xx.Length + " errori 4xx che non ha senso ritentare: " + string.Join(", ", _4xxTo5xx));
            }

            #endregion


            ApplicationStart.TempEnabled = false;
            ApplicationStart.CronLaunch = false;

            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(serverOptions =>
                        {
                            //https://docs.microsoft.com/it-it/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-3.1
                            serverOptions.Limits.MaxConcurrentConnections = null;
                            serverOptions.Limits.MaxConcurrentUpgradedConnections = null;
                            serverOptions.Limits.MaxRequestBodySize = null;
                            serverOptions.Limits.MinRequestBodyDataRate = new MinDataRate(bytesPerSecond: 1024, gracePeriod: TimeSpan.FromSeconds(3));
                            serverOptions.Limits.MinResponseDataRate = new MinDataRate(bytesPerSecond: 1024, gracePeriod: TimeSpan.FromSeconds(3));
                            serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
                            serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(20);
                            serverOptions.AddServerHeader = false;
                            serverOptions.ListenAnyIP(1000);
                        })
                        .UseStartup<Startup>();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<EventLogSettings>(config =>
                    {
                        config.LogName = "SmtpRelayer";
                        config.SourceName = "SmtpRelayer";
                    });

                    //Attivo cron su database e il resto, metto qua per dependency injection
                    ApplicationStart.Start(services);

                    services.AddHostedService<Worker>();
                })
                .UseWindowsService()
                .Build()
                .Run();
        }

        internal static void Trace(string info)
        {
            if (Worker.Trace)
                ManagerLog.Trace(info);
        }

        internal static Dictionary<string, Runner> DominiRunner = new();

        /// <summary>
        /// Blocca l'invio delle email di un dominio particolare, es.: tiscali risponde rallenta, blocco tutte le mail che vanno a tiscali per un po'
        /// Questo array contiene tutti i messaggi che sono inviati dal server quando ci avvisa di rallentare
        /// </summary>
        internal static string[] _temporaryMessage = Array.Empty<string>();
        
        internal static string[] _4xxTo5xx = Array.Empty<string>();

        /// <summary>
        /// Ci sono degli errori temporanei che non sono 404 ma sono come i 404, ovvero errori temporanei di cui vale la pena
        /// riprovare l'invio, questo array contiene tutti questi errori
        /// </summary>
        internal static string[] _5xxTo4xx = Array.Empty<string>();
    }
}
