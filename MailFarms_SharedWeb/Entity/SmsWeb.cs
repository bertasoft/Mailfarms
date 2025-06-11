using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailFarms_SharedWeb.Entity
{
    /// <summary>
    /// L'sms che viene inviato dai siti esterni a mailfarms.com
    /// </summary>
    public class SmsWeb
    {
        public string UniqueIdentifier { get; set; }
        public string Numero { get; set; }
        public string Testo { get; set; }
        public string MittenteSms { get; set; }
        public string Mittente { get; set; }
        public string Destinatario { get; set; }
        public long Caratteri { get; set; }
        public long NumeroMessaggi { get; set; }
        public string Sistema { get; set; }

        public SmsWeb()
        {
            UniqueIdentifier = string.Empty;
            Testo = string.Empty;
            MittenteSms = string.Empty;
            Mittente = string.Empty;
            Destinatario = string.Empty;
        }

        public static long SmsCreditiNecessari(string testo)
        {
            if (string.IsNullOrEmpty(testo))
                return 0;

            var caratteri = SmsCaratteri(testo);
            
            if (caratteri <= 160)
                return 1;

            var sms = caratteri / 153;

            if (caratteri % 153 != 0)
                sms++;

            return sms;
        }

        public static long SmsCaratteri(string testo)
        {
            if (string.IsNullOrEmpty(testo))
                return 0;

            var str  = GSMConverter.StringToGSMHexString(testo, false);

            return str.Length / 2;
        }

        // Data/info taken from http://en.wikipedia.org/wiki/GSM_03.38
        public static class GSMConverter
        {
            // The index of the character in the string represents the index
            // of the character in the respective character set

            // Basic Character Set
            private const string BASIC_SET =
                    "@£$¥èéùìòÇ\nØø\rÅåΔ_ΦΓΛΩΠΨΣΘΞ\x1bÆæßÉ !\"#¤%&'()*+,-./0123456789:;<=>?" +
                    "¡ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÑÜ§¿abcdefghijklmnopqrstuvwxyzäöñüà";

            // Basic Character Set Extension 
            private const string EXTENSION_SET =
                    "````````````````````^```````````````````{}`````\\````````````[~]`" +
                    "|````````````````````````````````````€``````````````````````````";

            // If the character is in the extension set, it must be preceded
            // with an 'ESC' character whose index is '27' in the Basic Character Set
            private const int ESC_INDEX = 27;

            public static string StringToGSMHexString(string text, bool delimitWithDash = true)
            {
                // Replace \r\n with \r to reduce character count
                text = text.Replace(Environment.NewLine, "\r");

                // Use this list to store the index of the character in 
                // the basic/extension character sets
                var indicies = new List<int>(text.Length * 2);

                foreach (var c in text)
                {
                    int index = BASIC_SET.IndexOf(c);
                    if (index != -1)
                    {
                        indicies.Add(index);
                        continue;
                    }

                    index = EXTENSION_SET.IndexOf(c);
                    if (index != -1)
                    {
                        // Add the 'ESC' character index before adding 
                        // the extension character index
                        indicies.Add(ESC_INDEX);
                        indicies.Add(index);
                        continue;
                    }
                }

                // Convert indicies to 2-digit hex
                var hex = indicies.Select(i => i.ToString("X2")).ToArray();

                string delimiter = delimitWithDash ? "-" : "";

                // Delimit output
                string delimited = string.Join(delimiter, hex);
                return delimited;
            }
        }
    }

}

