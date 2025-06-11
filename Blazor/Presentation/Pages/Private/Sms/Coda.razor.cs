using System;
using System.Collections.Generic;
using System.Linq;
using BlazorLibrary.Component;
using Business.Code;
using Business.Entity;
using CommonNetCore.GlobalExtension;
using MailFarmsBlazor.Code;
using Microsoft.AspNetCore.Components;

namespace MailFarmsBlazor.Pages.Private.Sms
{
    public partial class Coda : IDisposable
    {
        [Inject]
        RefreshService RefreshService { get; set; }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (!firstRender)
                return;

            SetLinkOrder("Table", OrderBy, asc);

            RefreshService.OnCodaRefreshCallback += Refresh;
        }

        void IDisposable.Dispose()
        {
            RefreshService.OnCodaRefreshCallback -= Refresh;
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

        public void EliminaSmsClick(LinkButton linkButton)
        {
            if (!linkButton.ConfirmResponse)
                return;

            var sms = linkButton.Object as Business.Entity.Sms;

            if (sms == null)
            {
                AlertFail("null");
                return;
            }

            if (!Business.Entity.Sms.Delete(out Avviso, sms))
                AlertFail(Avviso);

            RefreshService.OnCodaRefreshCallback.Invoke();
        }

        public void FiltraTextChange()
        {
            var list = new List<string>();
            whereObj = new List<object>();

            if (!__TextBox_Testo.Value.IsNullOrEmpty())
            {
                list.Add("Testo.Contains(@0)");
                whereObj.Add(__TextBox_Testo.Value.Encode());
            }

            if (!__TextBox_Mittente.Value.IsNullOrEmpty())
            {
                list.Add("Mittente.Contains(@1)");
                whereObj.Add(__TextBox_Mittente.Value.Encode());
            }

            if (!__TextBox_Destinatario.Value.IsNullOrEmpty())
            {
                list.Add("Destinatario.Contains(@2)");
                whereObj.Add(__TextBox_Destinatario.Value.Encode());
            }

            if (!__TextBox_Numero.Value.IsNullOrEmpty())
            {
                list.Add("Numero.Contains(@3)");
                whereObj.Add(__TextBox_Numero.Value.Encode());
            }

            if (!__TextBox_NumeroMessaggi.Value.IsNullOrEmpty())
            {
                list.Add("NumeroMessaggi = @4");
                whereObj.Add(__TextBox_NumeroMessaggi.Value);
            }

            if (!__TextBox_Caratteri.Value.IsNullOrEmpty())
            {
                list.Add("Caratteri = @5");
                whereObj.Add(__TextBox_Caratteri.Value);
            }

            if (!__TextBox_Sistema.Value.IsNullOrEmpty())
            {
                list.Add("Sistema.Contains(@6)");
                whereObj.Add(__TextBox_Sistema.Value);
            }

            if (list.Any())
                wherePredicate = string.Join(" AND ", list.ToArray()) + " AND ";
            else
                wherePredicate = string.Empty;

            StateHasChanged();
        }

        public void FiltraResetClick()
        {
            __TextBox_Destinatario.Value = string.Empty;
            __TextBox_Mittente.Value = string.Empty;
            __TextBox_Numero.Value = string.Empty;
            __TextBox_Testo.Value = string.Empty;
            __TextBox_NumeroMessaggi.Value = string.Empty;
            __TextBox_Caratteri.Value = string.Empty;

            StateHasChanged();
        }
    }
}
