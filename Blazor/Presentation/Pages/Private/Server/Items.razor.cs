using Microsoft.AspNetCore.Components;
using Business.Entity;
using BlazorLibrary.Component;
using BlazorLibrary.Code;
using MailFarmsBlazor.Code;

namespace MailFarmsBlazor.Pages.Private.Server
{
    public partial class Items : PageComponent
    {
        public void RiceveOnChangeCallback(CheckBox sender)
        {
            var server = sender.Object as Business.Entity.Server;

            if (server == null)
            {
                AlertFail("null");
                return;
            }

            server.Riceve = sender.Checked;

            if (!server.Save(out Avviso))
                AlertFail(Avviso);
        }

        public void AttivoOnChangeCallback(CheckBox sender)
        {
            var server = sender.Object as Business.Entity.Server;

            if (server == null)
            {
                AlertFail("null");
                return;
            }

            server.Attivo = sender.Checked;

            if (!server.Save(out Avviso))
                AlertFail(Avviso);
        }

        public void EliminaServerClick(LinkButton sender)
        {
            if (!sender.ConfirmResponse)
                return;

            var server = sender.Object as Business.Entity.Server;

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
