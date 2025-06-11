#region

using System;
using System.Collections.Generic;
using System.Linq;
using CommonNetCore.GlobalExtension;
using MailFarmsBlazor.Code;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

#endregion

namespace CommonNetCore.Web
{
    public static class CryptDecryptQueryString
    {
        public static NameValue[] GetDecodedQueryString(this HttpRequest request)
        {
            var url = request.GetDisplayUrl();

            var index = url.IndexOf('?');

            if (index == -1)
                return null;

            url = url.Substring(index + 1);

            return GetDecodedQueryString(url);
        }

        public static NameValue[] GetDecodedQueryString(this string codedQueryString)
        {
            codedQueryString = codedQueryString.Base64Decode();

            if (string.IsNullOrEmpty(codedQueryString))
                return null;

            var dictionary = new List<NameValue>();

            var arrMsgs = codedQueryString.Split('&');

            foreach (var arrMsg in arrMsgs)
            {
                var arrIndMsg = arrMsg.Split('='); //Get the Name

                var key = arrIndMsg[0];

                var values = string.Empty;

                if (arrIndMsg.Length > 1 && !string.IsNullOrEmpty(arrIndMsg[1]))
                    values = arrIndMsg[1];

                dictionary.Add(new NameValue(key, values));
            }

            return dictionary.ToArray();
        }

        /// <summary>
        ///     Da un link e un dictionary restituisce un link criptato base 64 non interpretabile
        /// </summary>
        public static string GetEncodedLink(string link, params NameValue[] nameValues)
        {
            var url = string.Empty;

            for (var i = 0; i < nameValues.Length; i++)
                url += nameValues[i].Name + "=" + nameValues[i].Value + "&";

            if (url.Length == 0)
                return link;

            url = url.Remove(url.Length - 1); //Tolgo &

            return link + "?" + url.Base64Encode();
        }

        /// <summary>
        ///     Prende un link e lo cripta
        /// </summary>
        public static string GetEncodedLink(string link)
        {
            var index = link.IndexOf('?');

            if (index == -1)
                return link;

            var querystring = link.Substring(index + 1);

            link = link.Remove(index + 1);

            return link + querystring.Base64Encode();
        }
    }

    [Serializable]
    public class NameValue
    {
        public string Name;
        public string Value;

        public NameValue(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }

    public static class NameValueExtension
    {
        public static bool TryGetValue(this List<NameValue> nameValues, string name, out string value)
        {
            if (nameValues.Any(p => p.Name == name))
            {
                value = nameValues.First(p => p.Name == name).Value;
                return true;
            }

            value = null;
            return false;
        }

        public static bool TryGetValue(this NameValue[] nameValues, string name, out string value)
        {
            if (nameValues != null && nameValues.Any(p => p.Name == name))
            {
                value = nameValues.First(p => p.Name == name).Value;
                return true;
            }

            value = null;
            return false;
        }
    }
}