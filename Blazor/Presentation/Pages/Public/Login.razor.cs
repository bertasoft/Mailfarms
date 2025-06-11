using System;
using System.Threading.Tasks;
using System.Web;
using BlazorLibrary.Component;
using Business.Entity;
using CommonNetCore;
using CommonNetCore.GlobalExtension;
using Hanssens.Net;
using MailFarmsBlazor.Code;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MailFarmsBlazor.Pages.Public
{

    public partial class Login : PageComponent
    {
        [Inject]
        private SessionDictionary sessionDictionary { get; set; }

        [Inject]
        private IJSRuntime JsRuntime { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (!firstRender)
                return;

            var email = await JsRuntime.InvokeAsync<string>("blazorExtensions.CookieGet", "email");

            if (string.IsNullOrEmpty(email))
                return;

            var utente = Business.Entity.Utenti.GetItem(email);

            if (utente == null || !utente.Attivo)
                return;

            var indirizzoIp = await JsRuntime.InvokeAsync<string>("blazorExtensions.GetIP");

            if (!Utenti.Accedi(out Avviso, email, utente.Password, indirizzoIp))
                return;

            sessionDictionary.AddReplace("email", email);

            var url = HttpUtility.UrlDecode(GetQueryString("ReturnUrl"));

            if (!string.IsNullOrEmpty(url))
            {
                navigationManager.NavigateTo(url);
                return;
            }

            navigationManager.NavigateTo("/private");
        }

        public async Task AccediClick()
        {
            var email = __TextBox_Email.Value.Trim();
            var passw = __TextBox_Password.Value.Trim();

            var indirizzoIp = await JsRuntime.InvokeAsync<string>("blazorExtensions.GetIP").ConfigureAwait(false);
            var userAgent = await JsRuntime.InvokeAsync<string>("blazorExtensions.GetUserAgent").ConfigureAwait(false);

            if (indirizzoIp == null)
                indirizzoIp = "null";

            if (!Utenti.Accedi(out Avviso, email, passw, indirizzoIp))
            {
                BloccoIp.BloccaIp(indirizzoIp, userAgent);

                BloccoAccesso.BloccaEmail(email);

                AlertFail(Avviso);

                return;
            }

            sessionDictionary.AddReplace("email", email);

            await JsRuntime.InvokeVoidAsync("blazorExtensions.CookieSet", "email", email).ConfigureAwait(false);

            var url = HttpUtility.UrlDecode(GetQueryString("ReturnUrl"));

            if (!string.IsNullOrEmpty(url))
            {
                navigationManager.NavigateTo(url);
                return;
            }

            navigationManager.NavigateTo("/private");
        }
    }
}
