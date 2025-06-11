using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.Entity;
using BlazorLibrary.Component;

namespace MailFarmsBlazor.Pages.Private.Utenti
{
    public partial class Items
    {
        public void AttivoOnChangeCallback(CheckBox sender)
        {
            var server = sender.Object as Business.Entity.Utenti;

            if (server == null)
            {
                AlertFail("null");
                return;
            }

            server.Attivo = sender.Checked;

            if (!server.Save(out Avviso))
                AlertFail(Avviso);
        }

        public void AdminOnChangeCallback(CheckBox sender)
        {
            var server = sender.Object as Business.Entity.Utenti;

            if (server == null)
            {
                AlertFail("null");
                return;
            }

            server.Admin = sender.Checked;

            if (!server.Save(out Avviso))
                AlertFail(Avviso);
        }

        public void EliminaUtenteClick(LinkButton sender)
        {
            if (!sender.ConfirmResponse)
                return;

            var server = sender.Object as Business.Entity.Utenti;

            if (server == null)
            {
                AlertFail("null");
                return;
            }

            if (!server.Delete(out Avviso))
                AlertFail(Avviso);
        }
    }
}
