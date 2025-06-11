using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using BlazorLibrary.Code;
using Business.Code;
using Business.Entity;
using CommonNetCore;
using CommonNetCore.GlobalExtension;
using CommonNetCore.Misc;
using MailFarms_SharedService.Entity;
using MailFarmsBlazor.Code;
using NPOI.HPSF;

namespace MailFarmsBlazor.Pages.Private.Server
{
    public partial class Editor : PageComponent
    {
        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (!firstRender)
                return;

            __TextBox_Mittente.Value = "info@doweb.srl";
            __TextBox_Destinatario.Value = "info@doweb.srl";
            __TextBox_Contenuto.Value = "Email di prova - Contenuto";
            __TextBox_Oggetto.Value = "Email di prova - Oggetto";

            Id =  GetQueryLong("ServerId");

            var server = Business.Entity.Server.GetItem(Id);

            if (server != null)
            {
                __TextBox_Helo.Value = server.Helo;
                __TextBox_IpPorta.Value = server.Ip;
                __CheckBox_Attivo.Checked = server.Attivo;
            }
            else
            {
                NuovoClick();
            }
        }

        public async Task SalvaClick()
        {
            var server = Business.Entity.Server.GetItem(Id);

            if (server == null)
                server = new Business.Entity.Server();

            server.Helo = __TextBox_Helo.Value;
            server.Attivo = __CheckBox_Attivo.Checked;
            server.Ip = __TextBox_IpPorta.Value;

            if (!Business.Entity.Server.Save(out Avviso, ref server))
            {
                AlertFail(Avviso);
                return;
            }

            await MailFarms_SharedService.Code.RequestWindowsService.AggiornaConfigurazione(server.Ip, "Helo", __TextBox_Helo.Value);

            Id = server.Id;

            AlertSuccess();
        }

        public void NuovoClick()
        {
            __CheckBox_Attivo.Checked = false;
            __TextBox_IpPorta.Value = string.Empty;
            __TextBox_Helo.Value = string.Empty;

            Id = 0;
        }

        public void InviaClick()
        {
            var email = new Business.Entity.Email
            {
                MittenteEmail = __TextBox_Mittente.Value,
                Oggetto = __TextBox_Oggetto.Value,
                Server = __TextBox_IpPorta.Value,
                DataProssimoTentativo = DateTime.Now,
                StatusCode4xx5xx = string.Empty,
                DestinatarioEmail = __TextBox_Destinatario.Value,
                DataArrivo = DateTime.Now,
                Contenuto = __TextBox_Destinatario.Value,
                DataUltimoTentativo = DateTime.MinValue,
                DestinatarioDataRegistrazione = DateTime.MinValue,
                DestinatarioNome = __TextBox_Destinatario.Value,
                MittenteNome = __TextBox_Mittente.Value,
                Immediata = true,
                RispondiA = __TextBox_Mittente.Value,
                Statoenum = Business.Entity.Email.EmailStatoEnum.Coda,
                UniqueIdentifier = Guid.NewGuid().ToString(),
                UrlEliminazione = string.Empty,
                NumeroTentativi = 0,
            };

            if (!email.Save(out string avviso))
            {
                AlertFail(avviso);
                return;
            }

            var html = email.Contenuto;

            var hrefs = html.GetHref();

            foreach (var href in hrefs)
            {
                html = html.ReplaceInsensitive("href=\"" + href + "\"", "href=\"" + ManagerQueryString.GetEncodedLink(Settings.Config.WebPath + "/e/c?e=" + email.Id + "&u=" + HttpUtility.UrlEncode(href)) + "\"");
                html = html.ReplaceInsensitive("href=\'" + href + "\'", "href=\'" + ManagerQueryString.GetEncodedLink(Settings.Config.WebPath + "/e/c?e=" + email.Id + "&u=" + HttpUtility.UrlEncode(href)) + "\'");
            }

            if (!hrefs.Any())
                email.DataClick = DateTime.Parse("10/10/1900"); //la uso come indicatore se non ci sono link

            //lo inserisco alla fine perchè se la pagina non è caricata completamente
            //il metodo scrollTop di javascript non funziona
            var index = html.IndexOf("</body>", StringComparison.OrdinalIgnoreCase);

            if (index == -1)
                index = html.Length;

            html = html.Insert(index, "<img alt=\"\" height=\"1px\" width=\"1px\" src=\"" + ManagerQueryString.GetEncodedLink(Settings.Config.WebPath + "/e/p?e=" + email.Id) + "\">");

            email.Contenuto = html;
            email.Save();

            Engine.EmailDaInviare.Add(email);

            AlertSuccess("Email inserita in coda");
        }
    }
}
