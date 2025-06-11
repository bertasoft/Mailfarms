using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MailFarms_SharedWeb.Entity;
using MailFarms_SharedWeb.Response;

namespace MailFarms_SharedWeb.Code
{
    /// <summary>
    /// Richieste fatte dagli applicativi esterni
    /// </summary>
    public class Request
    {

#if DEBUG
        private static readonly string Url = "http://localhost:8085/api/";
        //private static readonly string Url = "https://mailfarms.com/api/";
#else
        private static readonly string Url = "https://mailfarms.com/api/";
#endif        

        public static async Task<ResponseBoolAvviso> EmailNuovo(EmailWeb email)
        {
            var obj = ApiUtility.Serialize(email);

            try
            {
                var result = await HttpClientExtension.PostNoMemory(Url + "EmailNuovo", new ByteArrayContent(obj.Bytes, 0, obj.Size)).ConfigureAwait(false);

                return await ApiUtility.GetRequest<ResponseBoolAvviso>(result.Content).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new ResponseBoolAvviso()
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

        public static async Task<ResponseBoolAvviso> SmsNuovo(SmsWeb sms)
        {
            var obj = ApiUtility.Serialize(sms);

            try
            {
                var result = await HttpClientExtension.PostNoMemory(Url + "SmsNuovo", new ByteArrayContent(obj.Bytes, 0, obj.Size)).ConfigureAwait(false);

                return await ApiUtility.GetRequest<ResponseBoolAvviso>(result.Content).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new ResponseBoolAvviso()
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

        public static async Task<ResponseBoolAvviso> SmsNuovo(
            string numeroTelefonoDestinazione, 
            string testoMessaggio,
            string mittenteSms, //la procedura che invia il messaggio, il metodo (solo informativo)
            string mittente, //il nome di chi invia il messaggio, il processo (solo informativo)
            string destinatario, //a chi viene inviato il messaggio (solo informativo)
            string mittenteSistema, //chi appare come mittente negli SMS (deve essere verificato dal gateway)
            string uniqueIdentifier = null)
        {
            if (string.IsNullOrEmpty(uniqueIdentifier))
                uniqueIdentifier = Guid.NewGuid().ToString();

            SmsWeb.SmsCaratteri(testoMessaggio);
            SmsWeb.SmsCreditiNecessari(testoMessaggio);

            var obj = ApiUtility.Serialize(new SmsWeb()
            {
                Testo = testoMessaggio,
                Caratteri = SmsWeb.SmsCaratteri(testoMessaggio),
                NumeroMessaggi = SmsWeb.SmsCreditiNecessari(testoMessaggio),
                Destinatario = destinatario,
                Mittente = mittente,
                MittenteSms = mittenteSms,
                Sistema = mittenteSistema,
                Numero = numeroTelefonoDestinazione,
                UniqueIdentifier = uniqueIdentifier
            });

            try
            {
                var result = await HttpClientExtension.PostNoMemory(Url + "SmsNuovo", new ByteArrayContent(obj.Bytes, 0, obj.Size)).ConfigureAwait(false);

                return await ApiUtility.GetRequest<ResponseBoolAvviso>(result.Content).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new ResponseBoolAvviso()
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

        public static async Task<SmsStato> SmsStato(string guid)
        {
            try
            {
                var result = await HttpClientExtension.PostNoMemory(Url + "SmsStato", new StringContent(guid, Encoding.UTF8)).ConfigureAwait(false);

                return await ApiUtility.GetRequest<SmsStato>(result.Content).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                return new SmsStato
                {
                    Errore = string.Empty,
                    Avviso = e.Message,
                    Result = false
                };
            }
        }

        public static async Task<EmailStato> EmailStato(string guid)
        {
            try
            {
                var result = await HttpClientExtension.PostNoMemory(Url + "EmailStato", new StringContent(guid, Encoding.UTF8)).ConfigureAwait(false);

                return await ApiUtility.GetRequest<EmailStato>(result.Content).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                return new EmailStato
                {
                    Errore = e.Message,
                    Avviso = string.Empty
                };
            }
        }

        /// <summary>
        /// Se mailfarms.com è online
        /// </summary>
        public static async Task<bool> Ping()
        {
            try
            {
                var result = await HttpClientExtension.GetNoMemory(Url + "Ping").ConfigureAwait(false);

                var response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

                return response.IndexOf("true", StringComparison.OrdinalIgnoreCase) != -1;
            }
            catch
            {

            }

            return false;
        }
    }
}
