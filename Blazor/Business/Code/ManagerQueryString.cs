#region

using System;
using System.Collections.Generic;
using System.Linq;
using CommonNetCore.GlobalExtension;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

#endregion

namespace BlazorLibrary.Code
{
    public static class ManagerQueryString
    {
        public static long GetQueryStringId(this HttpRequest request)
        {
            var token = GetTokenFromQueryString(request);

            var item = token?.FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));

            return item?.Value.ToLong() ?? 0;
        }

        public static long GetQueryStringId(this string url)
        {
            var token = GetTokenFromQueryString(url);

            var item = token?.FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));

            return item?.Value.ToLong() ?? 0;
        }

        public static long GetQueryLong(this HttpRequest request, string key)
        {
            var token = GetTokenFromQueryString(request);

            var item = token?.FirstOrDefault(p => p.Name.Equals(key, StringComparison.OrdinalIgnoreCase));

            return item?.Value.ToLong() ?? 0;
        }

        public static long GetQueryLong(this string url, string key)
        {
            var token = GetTokenFromQueryString(url);

            var item = token?.FirstOrDefault(p => p.Name.Equals(key, StringComparison.OrdinalIgnoreCase));

            return item?.Value.ToLong() ?? 0;
        }

        public static long GetQueryLong(this Uri url, string key)
        {
            if (url == null)
                return 0;

            return url.ToString().GetQueryLong(key);
        }

        public static string GetQueryString(this HttpRequest request, string key)
        {
            var token = GetTokenFromQueryString(request);

            var item = token?.FirstOrDefault(p => p.Name.Equals(key, StringComparison.OrdinalIgnoreCase));

            return item?.Value ?? string.Empty;
        }

        public static string GetQueryString(this string url, string key)
        {
            var token = GetTokenFromQueryString(url);

            var item = token?.FirstOrDefault(p => p.Name.Equals(key, StringComparison.OrdinalIgnoreCase));

            return item?.Value ?? string.Empty;
        }

        public static string GetQueryString(this Uri url, string key)
        {
            if (url == null)
                return null;

            return url.ToString().GetQueryString(key);
        }

        public static NameValue[] GetTokenFromQueryString(this HttpRequest request)
        {
            var url = request.GetDisplayUrl();

            return GetTokenFromQueryString(url);
        }

        public static NameValue[] GetTokenFromQueryString(this string url)
        {
            if (url == null)
                return null;

            var index = url.IndexOf('?', StringComparison.Ordinal);

            if (index == -1)
                return null;

            url = url.Substring(index + 1);

            var decript = url.Base64Decode();

            if (!string.IsNullOrEmpty(decript))
                url = decript;

            var dictionary = new List<NameValue>();

            var arrMsgs = url.Split('&');

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
            if (link.IsNullOrEmpty())
                return link;

            var index = link.IndexOf('?', StringComparison.Ordinal);

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
        internal string Name;
        internal string Value;

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