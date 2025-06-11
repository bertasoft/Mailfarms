using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MailFarms_SharedWeb.Code
{
    public static class HttpClientExtension
    {
        private static readonly HttpClient Client = new HttpClient() { Timeout = TimeSpan.FromMinutes(5) };

        //non copia nello stream locale, in memoria i byte che scarica
        public static async Task<HttpResponseMessage> PostNoMemory(string url, HttpContent content)
        {
            var maxRetry = 5;
            var delayCount = 0;

            Exception e = null;
                
            while (maxRetry > 0)
            {
                var success = false;

                try
                {
                    var requestMessage = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };

                    var message = await Client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                    success = true;                    
                    
                    return message;
                }
                catch (Exception ex)
                {
                    e = ex;
                }
                finally
                {
                    if (!success)
                        await Task.Delay(delayCount * 500).ConfigureAwait(false);
                }

                delayCount++;
                maxRetry--;
            }

            throw new Exception("POST: non riesco a collegarmi a " + url, e);
        }

        //non copia nello stream locale, in memoria i byte che scarica
        public static async Task<HttpResponseMessage> GetNoMemory(string url)
        {
            var maxRetry = 5;
            var delayCount = 0;

            Exception e = null;

            while (maxRetry > 0)
            {
                var success = false;

                try
                {
                    using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

                    var message = await Client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                    success = true;

                    return message;
                }
                catch (Exception ex)
                {
                    e = ex;
                }
                finally
                {
                    if (!success)
                        await Task.Delay(delayCount * 500).ConfigureAwait(false);
                }

                delayCount++;
                maxRetry--;
            }

            throw new Exception("GET: non riesco a collegarmi a " + url, e);
        }
    }
}
