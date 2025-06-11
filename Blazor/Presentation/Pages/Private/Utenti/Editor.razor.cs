using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonNetCore.GlobalExtension;
using MailFarmsBlazor.Code;

namespace MailFarmsBlazor.Pages.Private.Utenti
{
    public partial class Editor : PageComponent
    {
        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (!firstRender)
                return;

            Id = GetQueryLong("UtentiId");

            var utente = Business.Entity.Utenti.GetItem(Id);

            if (utente != null)
            {
                __TextBox_Nome.Value = utente.Nome.Decode();
                __TextBox_Cognome.Value = utente.Cognome.Decode();
                __TextBox_Email.Value = utente.Email;
                __CheckBox_Attivo.Checked = utente.Attivo;
                __CheckBox_Admin.Checked = utente.Admin;
            }
            else
            {
                NuovoClick();
            }
        }

        public void SalvaClick()
        {
            var utente = Business.Entity.Utenti.GetItem(Id);

            if (utente == null)
                utente = new Business.Entity.Utenti();

            utente.Attivo = __CheckBox_Attivo.Checked;
            utente.Admin = __CheckBox_Admin.Checked;
            utente.Nome = __TextBox_Nome.Value.Encode();
            utente.Cognome = __TextBox_Cognome.Value.Encode();
            utente.Email = __TextBox_Email.Value;

            if (!__TextBox_Password.Value.IsNullOrEmpty())
                utente.Password = __TextBox_Password.Value;

            if (!Business.Entity.Utenti.Save(out Avviso, ref utente))
            {
                AlertFail(Avviso);
                return;
            }

            Id = utente.Id;

            AlertSuccess();
        }

        public void NuovoClick()
        {
            __CheckBox_Attivo.Checked = false;
            __CheckBox_Admin.Checked = false;

            __TextBox_Nome.Value = string.Empty;
            __TextBox_Cognome.Value = string.Empty;
            __TextBox_Email.Value = string.Empty;
            __TextBox_Password.Value = string.Empty;

            Id = 0;
        }
    }
}
