using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorLibrary.Component;

namespace MailFarmsBlazor.Pages.Private.Server
{
    public partial class Configurazione
    {
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (!firstRender)
                return;

            __DropDownList_Selezione.Values = from p in Enum.GetNames(typeof(Business.Entity.ServerImpostazioni.ServerImpostazioniEnum)).OrderBy(p => p) select new Select.Item(p);

            CaricaImpostazione();
        }

        protected void DropDownListSelezioneSelectIndexChanged()
        {
            CaricaImpostazione();
        }

        protected void SalvaClick()
        {
            var impostazione = Business.Entity.ServerImpostazioni.GetItem((Business.Entity.ServerImpostazioni.ServerImpostazioniEnum)Enum.Parse(typeof(Business.Entity.ServerImpostazioni.ServerImpostazioniEnum), __DropDownList_Selezione.SelectedItem().Value));

            impostazione.Valore = __TextBox_Valore.Value;

            if (!Business.Entity.ServerImpostazioni.Save(out Avviso, ref impostazione))
            {
                AlertFail(Avviso);
                return;
            }

            AlertSuccess();
        }

        private void CaricaImpostazione()
        {
            var impostazione = Business.Entity.ServerImpostazioni.GetItem((Business.Entity.ServerImpostazioni.ServerImpostazioniEnum)Enum.Parse(typeof(Business.Entity.ServerImpostazioni.ServerImpostazioniEnum), __DropDownList_Selezione.SelectedItem().Value));

            __TextBox_Valore.Value = impostazione.Valore;

            StateHasChanged();
        }
    }
}
