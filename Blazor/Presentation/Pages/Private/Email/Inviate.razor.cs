using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorLibrary.Component;
using Business.Code;
using CommonNetCore.GlobalExtension;
using MailFarmsBlazor.Code;
using Business.Entity;
using Hanssens.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MailFarmsBlazor.Pages.Private.Email
{
    public partial class Inviate : IDisposable
    {
        [Inject]
        RefreshService EmailRefreshService { get; set; }

        [Inject]
        private IJSRuntime JsRuntime { get; set; }

        [Inject]
        private LayoutService layoutService { get; set; }

        protected override void OnInitialized()
        {
            using var storage = new LocalStorage();

            if (storage.Exists("inviatePager"))
                elementiPerPagina = storage.Get<int>("inviatePager");

            layoutService.OnTitlePageCallback.InvokeAsync("Email - Inviate");

            OnFancy = new EventCallback<LinkButton>(null, (Action<LinkButton>)(p =>
            {
                var email = (Business.Entity.Email)p.Object;
                JsRuntime.InvokeVoidAsync("blazorExtensions.FancyBox", CommonNetCore.Settings.Config.WebPath + "/private/email/popupanteprima?EmailId=" + email.Id);
                StateHasChanged();
            }));

            OnFancyLog = new EventCallback<LinkButton>(null, (Action<LinkButton>)(p =>
            {
                var email = (Business.Entity.Email)p.Object;
                JsRuntime.InvokeVoidAsync("blazorExtensions.FancyBox", CommonNetCore.Settings.Config.WebPath + "/private/email/popuplog?EmailId=" + email.Id);
                StateHasChanged();
            }));
        }

        public void CambiaPagina(Tuple<long, int> paginaElementi)
        {
            paginaCorrente = paginaElementi.Item1;
            elementiPerPagina = paginaElementi.Item2;

            using var storage = new LocalStorage();
            storage.Store("inviatePager", elementiPerPagina);

            //StateHasChanged();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (!firstRender)
                return;

            SetLinkOrder("Table", OrderBy, asc);

            EmailRefreshService.OnCodaRefreshCallback += Refresh;
        }

        void IDisposable.Dispose()
        {
            EmailRefreshService.OnCodaRefreshCallback -= Refresh;
        }

        private void Refresh()
        {
            InvokeAsync(StateHasChanged);
        }

        public void OrdinamentoClick(LinkOrder linkOrder)
        {
            OrderBy = linkOrder.OrderColumn;

            if (linkOrder.OrderCurrent == OrderBy)
                asc = !asc;

            SetLinkOrder("Table", OrderBy, asc);

            StateHasChanged();
        }

        public void EliminaMailClick(LinkButton linkButton)
        {
            if (!linkButton.ConfirmResponse)
                return;

            var email = linkButton.Object as Business.Entity.Email;

            if (email == null)
            {
                AlertFail("null");
                return;
            }

            if (!email.Delete(out Avviso))
                AlertFail(Avviso);
        }

        public void FiltraTextChange()
        {
            var list = new List<string>();
            whereObj = new List<object>();

            if (!__TextBox_Oggetto.Value.IsNullOrEmpty())
            {
                list.Add("Oggetto.Contains(@0)");
                whereObj.Add(__TextBox_Oggetto.Value.Encode());
            }

            if (!__TextBox_Mittente.Value.IsNullOrEmpty())
            {
                list.Add("MittenteEmail.Contains(@1)");
                whereObj.Add(__TextBox_Mittente.Value.Encode());
            }

            if (!__TextBox_Destinatario.Value.IsNullOrEmpty())
            {
                list.Add("DestinatarioEmail.Contains(@2)");
                whereObj.Add(__TextBox_Destinatario.Value.Encode());
            }

            if (!__TextBox_Server.Value.IsNullOrEmpty())
            {
                list.Add("Server.Contains(@3)");
                whereObj.Add(__TextBox_Server.Value.Encode());
            }

            if (!__TextBox_UniqueIdentifier.Value.IsNullOrEmpty())
            {
                list.Add("UniqueIdentifier.Contains(@4)");
                whereObj.Add(__TextBox_UniqueIdentifier.Value.Encode());
            }

            if (list.Any())
                wherePredicate = string.Join(" AND ", list.ToArray()) + " AND ";
            else
                wherePredicate = string.Empty;

            StateHasChanged();
        }

        public void FiltraResetClick()
        {
            __TextBox_Server.Value = string.Empty;
            __TextBox_Mittente.Value = string.Empty;
            __TextBox_Oggetto.Value = string.Empty;
            __TextBox_Server.Value = string.Empty;
            __TextBox_UniqueIdentifier.Value = string.Empty;

            StateHasChanged();
        }
    }
}
