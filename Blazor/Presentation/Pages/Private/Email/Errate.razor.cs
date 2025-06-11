using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorLibrary.Component;
using Business.Code;
using CommonNetCore.GlobalExtension;
using MailFarmsBlazor.Code;
using Microsoft.AspNetCore.Components;
using Business.Entity;

namespace MailFarmsBlazor.Pages.Private.Email
{
    public partial class Errate : IDisposable
    {
        [Inject]
        RefreshService EmailRefreshService { get; set; }

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

        public void RinviaEmailClick(MyButton myButton)
        {
            if (!myButton.ConfirmResponse)
                return;

            var email = myButton.Object as Business.Entity.Email;

            if (email == null)
            {
                AlertFail("null");
                return;
            }

            Business.Entity.Email.RinviaEmail(email);

            AlertSuccess("Inserita in coda");
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

            if (!__TextBox_Errore.Value.IsNullOrEmpty())
            {
                list.Add("StatusCode4xx5xx.Contains(@4)");
                whereObj.Add(__TextBox_Errore.Value.Encode());
            }

            if (!__TextBox_UniqueIdentifier.Value.IsNullOrEmpty())
            {
                list.Add("UniqueIdentifier.Contains(@5)");
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
            __TextBox_Errore.Value = string.Empty;
            __TextBox_UniqueIdentifier.Value = string.Empty;

            StateHasChanged();
        }

        private void InviaClick(MyButton myButton)
        {
            if (!myButton.ConfirmResponse)
                return;

            var emails = Business.Collection.EmailCollection.GetList(wherePredicate: wherePredicate + "Stato = 2", whereValues: whereObj.ToArray());

            foreach (var email in emails)
                Business.Entity.Email.RinviaEmail(email);

            AlertSuccess("Inserite in coda");
        }
    }
}
