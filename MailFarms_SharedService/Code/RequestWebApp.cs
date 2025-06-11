using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using CommonNetCore.GlobalExtension;
using CommonNetCore.Misc;
using MailFarms_SharedService.Entity;
using MailFarms_SharedService.Response;

namespace MailFarms_SharedService.Code
{
    /// <summary>
    /// Richieste fatte al sito mailfarms.com
    /// </summary>
    public class RequestWebApp
    {
        #region Common

#if DEBUG
        private static string Url = "http://localhost:8085/api/";
#else
        private static string Url = "https://mailfarms.com/api/";
#endif

        private static readonly HttpClient client = new HttpClient();

        public RequestWebApp()
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        }

        #endregion

        /// <summary>
        /// Risponde WindowsService.Business.Api.ServiceController
        /// </summary>
        public static async Task<ResponseBoolAvviso> SegnalaStato(Stato stato)
        {
            var obj = ApiUtility.Serialize(stato);

            try
            {
                var result = await HttpClientExtension.PostNoMemory(Url + "SegnalaStato", new ByteArrayContent(obj.Bytes, 0, obj.Size)).ConfigureAwait(false);

                return await ApiUtility.GetRequest<ResponseBoolAvviso>(result.Content).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ManagerLog.Error(ex, "SegnalaStato(stato)");

                return new ResponseBoolAvviso
                {
                    Avviso = ex.Message,
                    Result = false
                };
            }
            finally
            {
                Pool<byte>.SpaceReturn(obj.Bytes);
            }
        }

        /// <summary>
        /// Se mailfarms.com è online
        /// </summary>
        public static async Task<bool> Ping()
        {
            try
            {
                var response = await client.GetStringAsync(Url + "Ping").ConfigureAwait(false);

                return response.IndexOf("true", StringComparison.OrdinalIgnoreCase) != -1;
            }
            catch 
            {

            }

            return false;
        }

        /// <summary>
        /// Mi dice se sono attivo e posso processare la mia coda, risponde WindowsService.Business.Api.ServiceController
        /// </summary>
        public static async Task<bool> GetAttivo(string ilMioIp)
        {
            try
            {
                var result = await HttpClientExtension.PostNoMemory(Url + "GetAttivo", new StringContent(ilMioIp, Encoding.UTF8)).ConfigureAwait(false);

                var obj = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

                return obj.ToBool();
            }
            catch (Exception ex)
            {
                ManagerLog.Error(ex, "GetAttivo(" + ilMioIp + ")");

                return false;
            }
        }
    }
}


