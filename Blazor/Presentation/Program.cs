using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommonNetCore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MailFarmsBlazor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CommonNetCore.ApplicationStart.Initialize();

#if DEBUG
            Settings.Config.WebPath = "http://localhost:8085";
            Settings.Config.Database.Path = @"C:\Progetti\Web\MailFarms\Blazor\Presentation\Public\Database\Database.db3";
#endif

            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseWebRoot(@".\");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    //quindi qua dentro attivo o meno cron su database o altro
                    CommonNetCore.ApplicationStart.Start(services);
                })
                .Build()
                .Run();
        }
    }
}
