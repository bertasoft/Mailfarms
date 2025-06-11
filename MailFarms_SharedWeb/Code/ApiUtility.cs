using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MailFarms_SharedWeb.Code
{
    public static class ApiUtility
    {
        public const string Ok = "Ok";

        private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {         
            Culture = new System.Globalization.CultureInfo("it-IT"),
        };

        public class ArrayReturn
        {
            public byte[] Bytes;
            public int Size;
        }

        public static ArrayReturn Serialize<T>(T obj)
        {
            var json = JsonConvert.SerializeObject(obj, serializerSettings);

            var bytes = Pool<byte>.SpaceGet(Encoding.UTF8.GetMaxByteCount(json.Length));

            var size = Encoding.UTF8.GetBytes(json, 0, json.Length, bytes, 0);

            return new ArrayReturn
            {
                Bytes = bytes,
                Size = size
            };
        }

        public static async Task<T> GetRequest<T>(HttpContent httpContent)
        {
            var json = await httpContent.ReadAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<T>(json, serializerSettings);
        }

        /// <summary>
        /// per inviare la risposta in json
        /// </summary>
        public static async Task SerializeAndSend(string str, HttpResponse response)
        {
            var bytes = Pool<byte>.SpaceGet(Encoding.UTF8.GetMaxByteCount(str.Length));

            var size = Encoding.UTF8.GetBytes(str, 0, str.Length, bytes, 0);

            await response.Body.WriteAsync(bytes, 0, size).ConfigureAwait(false);

            Pool<byte>.SpaceReturn(bytes);
        }

        /// <summary>
        /// per inviare la risposta in json
        /// </summary>
        public static async Task SerializeAndSend<T>(T obj, HttpResponse response)
        {
            var json = JsonConvert.SerializeObject(obj, serializerSettings);

            var bytes = Pool<byte>.SpaceGet(Encoding.UTF8.GetMaxByteCount(json.Length));

            var size = Encoding.UTF8.GetBytes(json, 0, json.Length, bytes, 0);

            await response.Body.WriteAsync(bytes, 0, size).ConfigureAwait(false);

            Pool<byte>.SpaceReturn(bytes);
        }

        /// <summary>
        /// Per leggere la richiesta se inviata in byte con post
        /// </summary>
        public static async Task<T> ReadArrayContent<T>(HttpRequest request)
        {
            using var textReader = new StreamReader(request.Body, Encoding.UTF8);

            var json = await textReader.ReadToEndAsync().ConfigureAwait(false);

            var obj = JsonConvert.DeserializeObject<T>(json, serializerSettings);

            return obj;
        }

        /// <summary>
        /// per leggere la richiesta se inviata con StringContent, se ricevuta da WebForms
        /// </summary>
        public static async Task<string> ReadStringContent(HttpRequest request)
        {
            string stringContent = await new StreamReader(request.Body).ReadToEndAsync().ConfigureAwait(false);

            return stringContent;
        }
    }
}
