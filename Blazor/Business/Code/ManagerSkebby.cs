using System;
using System.Net;
using System.Text;
using Business.Entity;
using CommonNetCore.Misc;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
#pragma warning disable 414

namespace Business.Code
{
    public static class ManagerSkebby
    {
        //PER PRENDERE user_key USARE QUESTO, LOGIN E PASSWORD SONO QUELLI USATI PER ACCEDERE AL SITO
        //SCARICARE CURL PER WINDOWS
        //curl -XGET "https://api.skebby.it/API/v1.0/REST/token?username=LOGIN&password=PASSWORD" -H "Content-Type: application/json"

        private static readonly string Baseurl = Impostazioni.GetValore(Impostazioni.ImpostazioniEnum.SkebbyApiUrl);
        //private static string MESSAGE_HIGH_QUALITY = "GP";
        private static readonly string MessageMediumQuality = "TI";
        //private static readonly string MESSAGE_LOW_QUALITY = "SI";

        public static bool InviaSMS(out string avviso, Sms sms)
        {
            var sendSmsRequest = new SendSMS
            {
                message = sms.Testo,
                message_type = MessageMediumQuality,
                recipient = new[] { sms.Numero.Replace(" ", "") },
                sender = sms.MittenteSms
            };

            avviso = string.Empty;

            var tentativi = 3;

            Exception exception = null;

            while (tentativi > 0)
            {
                tentativi--;

                try
                {
                    //(WebException) System.Net.WebException: The request was aborted: Could not create SSL/TLS secure channel. 
                    //at System.Net.WebClient.UploadDataInternal(Uri address, String method, Byte[] data, WebRequest& request) at 
                    //System.Net.WebClient.UploadString(Uri address, String method, String data) at
                    //Business.Manager.ManagerSkebby.InviaSMS(String text, String numeroTelefono)
                    using (var wb = new WebClient())
                    {
                        wb.Encoding = Encoding.UTF8;
                        wb.Headers.Set(HttpRequestHeader.ContentType, "application/json; charset=utf-8");
                        wb.Headers.Add("user_key", Impostazioni.GetValore(Impostazioni.ImpostazioniEnum.SkebbyUserKey));
                        wb.Headers.Add("Access_token", Impostazioni.GetValore(Impostazioni.ImpostazioniEnum.SkebbyAccessToken));

                        var json = JsonConvert.SerializeObject(sendSmsRequest, Formatting.None);

#if !DEBUG
                        var sentSmsBodyResponse = wb.UploadString(Baseurl + "sms", "POST", json);

                        var response = JsonConvert.DeserializeObject<SMSSent>(sentSmsBodyResponse);

                        if (response == null)
                            continue;

                        if (!response.result.Equals("OK", StringComparison.OrdinalIgnoreCase))
                        {
                            ManagerLog.Error(sentSmsBodyResponse);
                            avviso = response.result;
                            return false;
                        }
#endif
                        
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }

            if (exception != null)
                avviso = exception.Message;

            ManagerLog.Error(exception, sms.Testo);

            return false;
        }

        private class SendSMS
        {
            /* The message body */
            public string message;

            /** The message type */
            public string message_type;

            /** The sender Alias (TPOA) */
            public string sender;

            /** Postpone the SMS message sending to the specified date */
#pragma warning disable 169
            public DateTime? scheduled_delivery_time = null;
#pragma warning restore 169

            /** The list of recipients */
            public string[] recipient;

            /** Should the API return the remaining credits? */
#pragma warning disable 169
            public bool returnCredits = false;
#pragma warning restore 169
        }

        private class SMSSent
        {
#pragma warning disable 649
            public string result = string.Empty;
#pragma warning restore 649

#pragma warning disable 169
            public string order_id = string.Empty;
#pragma warning restore 169

#pragma warning disable 169
            public int total_sent = 0;
#pragma warning restore 169

#pragma warning disable 169
            public int remaining_credits = 0;
#pragma warning restore 169
        }
    }
}
