using System.Threading.Tasks;
using CommonNetCore.GlobalExtension;
using Microsoft.AspNetCore.Mvc;

namespace MailFarmsBlazor.Api
{
    public class DownloadController : ControllerBase
    {
        [HttpGet]
        public async Task<FileContentResult> EmailAllegato()
        {
            var id = Request.RouteValues["id"].ToString();

            var allegato = Business.Entity.EmailAllegati.GetItem(id.ToLong());

            if (allegato == null)
                return null;

            if (!System.IO.File.Exists(allegato.PercorsoDisco))
                return null;

            var bytes = await System.IO.File.ReadAllBytesAsync(allegato.PercorsoDisco);

            bytes = await bytes.DecompressAsync();

            return File(bytes, "application/octet-stream", allegato.NomeFile);
        }
    }
}