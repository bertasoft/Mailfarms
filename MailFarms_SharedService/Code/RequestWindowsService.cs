using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CommonNetCore.Misc;
using MailFarms_SharedService.Entity;
using MailFarms_SharedService.Response;

namespace MailFarms_SharedService.Code
{
    /// <summary>
    /// Richieste fatte ai Windows Service
    /// </summary>
    public class RequestWindowsService
    {
        #region Common

        private static readonly HttpClient client = new HttpClient();
        private static readonly HttpClient fastClient = new HttpClient();

        public RequestWindowsService()
        {
            client.Timeout = TimeSpan.FromSeconds(10);
            fastClient.Timeout = TimeSpan.FromSeconds(1);
        }

        #endregion

        /// <summary>
        /// Risponde WindowsService.Business.Api.ServiceController
        /// </summary>
        public static async Task<long> InCoda(string ip)
        {
            var obj = ApiUtility.Serialize(ip);

            try
            {
                var result = await HttpClientExtension.PostNoMemory("http://" + ip + ":1000/api/InCoda", new ByteArrayContent(obj.Bytes, 0, obj.Size)).ConfigureAwait(false);

                return await ApiUtility.GetRequest<long>(result.Content).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ManagerLog.Error(ex, "InCoda(" + ip + ")");

                return -1;
            }
            finally
            {
                Pool<byte>.SpaceReturn(obj.Bytes);
            }
        }

        /// <summary>
        /// Risponde WindowsService.Business.Api.ServiceController
        /// </summary>
        public static async Task<ResponseBoolAvviso> NuovaEmail(EmailService email, string ip)
        {
            var obj = ApiUtility.Serialize(email);

            try
            {
                var result = await HttpClientExtension.PostNoMemory("http://" + ip + ":1000/api/NuovaEmail", new ByteArrayContent(obj.Bytes, 0, obj.Size)).ConfigureAwait(false);

                return await ApiUtility.GetRequest<ResponseBoolAvviso>(result.Content).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ManagerLog.Error(ex, "NuovaEmail(email, " + ip + ")");

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

        public static async Task AggiornaConfigurazione(string ip, string impostazione, string valore)
        {
            var obj = ApiUtility.Serialize(new Tuple<string, string>(impostazione, valore));

            try
            {
                await HttpClientExtension.PostNoMemory("http://" + ip + ":1000/api/AggiornaConfigurazione", new ByteArrayContent(obj.Bytes, 0, obj.Size)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ManagerLog.Error(ex, "AggiornaConfigurazione(" + ip + ", " + impostazione + ", " + valore + ")");
            }
            finally
            {
                Pool<byte>.SpaceReturn(obj.Bytes);
            }
        }

        public static async Task<bool> PingAsync(string ip)
        {
            try
            {
                var result = await fastClient.GetStringAsync("http://" + ip + ":1000/api/Ping").ConfigureAwait(false);

                return result.IndexOf("true", StringComparison.OrdinalIgnoreCase) != -1;
            }
            catch (Exception ex)
            {
                ManagerLog.Error(ex, "Ping(" + ip + ")");
            }

            return false;
        }

        public static bool Ping(string ip)
        {
            try
            {
                var result = string.Empty;

                Task.Run(async () =>
                {
                    result = await fastClient.GetStringAsync("http://" + ip + ":1000/api/Ping");

                }).Wait(1000);

                return result.IndexOf("true", StringComparison.OrdinalIgnoreCase) != -1;
            }
            catch (Exception ex)
            {
                ManagerLog.Error(ex, "Ping(" + ip + ")");
            }

            return false;

        }

        /// <summary>
        /// Aggiorna lo stato di attivo sul service - un service processa la coda solo se è attivo
        /// </summary>
        public static async Task Attivo(string ip, bool attivo)
        {
            try
            {
                await HttpClientExtension.PostNoMemory("http://" + ip + ":1000/api/Attivo", new StringContent(attivo.ToString())).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ManagerLog.Error(ex, "Attivo(" + ip + ", " + attivo + ")");
            }
        }

        /// <summary>
        /// Aggiorna l'indirizzo ip sul service - deve sapere qual'è il suo ip per identificarlo quando mi invia richieste
        /// </summary>
        public static async Task<ResponseBoolAvviso> AggiornaIndirizzoIp(string ip)
        {
            try
            {
                var result = await HttpClientExtension.PostNoMemory("http://" + ip + ":1000/api/AggiornaIndirizzoIp", new StringContent(ip)).ConfigureAwait(false);

                return await ApiUtility.GetRequest<ResponseBoolAvviso>(result.Content).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new ResponseBoolAvviso
                {
                    Avviso = "Impossibile contattare il service: " + ex.Message,
                    Result = false
                };
            }
        }

        #region FileLog

        public static async Task<string[]> GetLogFileNameList(string ip)
        {
            try
            {
                var result = await HttpClientExtension.PostNoMemory("http://" + ip + ":1000/api/LogFileNameList", null).ConfigureAwait(false);

                return await ApiUtility.GetRequest<string[]>(result.Content).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ManagerLog.Error(ex, "GetLogFileNameList(" + ip + ")");

                return Array.Empty<string>();
            }
        }

        public static async Task<string> LogFileNameContent(string ip, string fileName)
        {
            try
            {
                var result = await HttpClientExtension.PostNoMemory("http://" + ip + ":1000/api/LogFileNameContent", new StringContent(fileName, Encoding.UTF8)).ConfigureAwait(false);

                return await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ManagerLog.Error(ex, "LogFileNameContent(" + ip + ", " + fileName + ")");

                return string.Empty;
            }
        }

        public static async Task<string> LogFileNameDelete(string ip, string fileName)
        {
            try
            {
                var result = await HttpClientExtension.PostNoMemory("http://" + ip + ":1000/api/LogFileNameDelete", new StringContent(fileName, Encoding.UTF8)).ConfigureAwait(false);

                return await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ManagerLog.Error(ex, "LogFileNameDelete(" + ip + ", " + fileName + ")");

                return string.Empty;
            }
        }

        public static async Task LogFileNameDeleteAll(string ip)
        {
            try
            {
                await HttpClientExtension.PostNoMemory("http://" + ip + ":1000/api/LogFileNameDeleteAll", null).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ManagerLog.Error(ex, "LogFileNameDeleteAll(" + ip + ")");
            }
        }

        #endregion
    }
}


