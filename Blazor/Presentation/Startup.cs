using System.Collections.Generic;
using System.IO;
using System.Threading;
using BlazorLibrary.Code;
using BlazorLibrary.Middleware;
using Business.Code;
using Business.Entity;
using MailFarmsBlazor.Code;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MailFarmsBlazor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private Timer Timer;

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options => options.EnableEndpointRouting = false);

            services.AddResponseCaching();

            services.AddResponseCompression(p => { p.EnableForHttps = true; } );

            services.AddRazorPages();            
         
            services.AddServerSideBlazor();

            services.AddScoped<AlertService>();
            services.AddScoped<LayoutService>();
            services.AddScoped<SessionDictionary>();

            services.AddSingleton<RefreshService>(new RefreshService());

            new CronDay();

            new CronMinute();

            Engine.InserisciInCoda();

            Timer = new Timer(sender =>
            {
                Engine.DistribuisciEmail();
                Engine.InviaEmail();
                Engine.DistribuisciSms();
            }, null, 1000, 1000);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //app.UseMiddleware<MiddlewareRedirect>();

            app.UseMiddleware<MiddlewareException>();

            //https://docs.microsoft.com/it-it/aspnet/core/fundamentals/middleware/index?view=aspnetcore-3.1#use-run-and-map
            //https://docs.microsoft.com/it-it/aspnet/core/migration/http-modules?view=aspnetcore-3.1

            if (!env.IsDevelopment())
                app.UseHttpsRedirection();

            app.UseMvc(
                    routes =>
                    {
                        routes.MapRoute("EmailController", "e/{action}", new { controller = "Email" });
                        routes.MapRoute("GetController", "download/{action}/{*id}", new {controller = "Download"});
                        routes.MapRoute("ApiController", "{controller}/{action=Ping}");
                    }
                );

            app.UseResponseCaching();

            app.UseResponseCompression();

            app.UseDefaultFiles(new DefaultFilesOptions
            {
                DefaultFileNames = new List<string> { "/" }
            });

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
