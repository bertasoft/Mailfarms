using MailFarmsBlazor.Code;
using Microsoft.AspNetCore.Http;
using System;

namespace CommonNetCore.Web
{
    /// <summary>
    ///     Metodi utilizzati dentro le pagine aspx
    /// </summary>
    public static class CommonPage
    {
        #region Querystring

        /// <summary>
        /// Ritorna l'id passato via querystring, se l'id non è corretto ritorna -1
        /// </summary>
        public static long GetQueryId(string queryName, HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(queryName))
                throw new ArgumentNullException(nameof(queryName));

            if (request.Query.TryGetValue(queryName, out var valore))
            {
                if (long.TryParse(valore, out var id))
                {
                    return id;
                }
            }

            return -1;
        }

        ///// <summary>
        /////     Ritorna l'id passato via querystring, se l'id non è corretto ritorna -1
        ///// </summary>
        //public static long GetQueryId(string queryName)
        //{
        //    return GetQueryId(queryName, HttpContext.Current.Request);
        //}

        /// <summary>
        ///     Ritorna la stringa passata via querystring
        /// </summary>
        //public static string GetQuery(string queryName)
        //{
        //    var req = HttpContext.Current.Request.GetDecodedQueryString();

        //    string valore;

        //    if (req.TryGetValue(queryName, out valore))
        //        return valore;

        //    if (string.IsNullOrEmpty(HttpContext.Current.Request.QueryString[queryName]))
        //        return string.Empty;

        //    return HttpContext.Current.Request.QueryString[queryName];
        //}

        ///// <summary>
        /////     Ritorna l'url senza la chiave/valore querystring specificato
        ///// </summary>
        //public static string RemoveQuery(string queryName)
        //{
        //    if (queryName.IsNullOrEmpty())
        //        return HttpContext.Current.Request.RawUrl;

        //    var qs = HttpContext.Current.Request.QueryString;

        //    if (qs.Count == 0)
        //        return NoQuerystring;

        //    var url = NoQuerystring;

        //    var connetor = "?";

        //    foreach (string q in qs)
        //    {
        //        if (string.Equals(q, queryName, StringComparison.InvariantCultureIgnoreCase))
        //            continue;

        //        url += connetor + q + "=" + qs[q];

        //        if (connetor == "?")
        //            connetor = "&";
        //    }

        //    return url;
        //}

        ///// <summary>
        /////     Aggiunge all'url corrente la chiave/valore specificata
        ///// </summary>
        //public static string AddQuery(string queryName, string value)
        //{
        //    if (queryName.IsNullOrEmpty() || value.IsNullOrEmpty())
        //        return HttpContext.Current.Request.RawUrl;

        //    var connetor = "?";

        //    var qs = HttpContext.Current.Request.QueryString;

        //    if (qs.Count > 0)
        //        connetor = "&";

        //    return HttpContext.Current.Request.RawUrl + connetor + queryName + "=" + value;
        //}

        //public static string NoQuerystringRelative
        //{
        //    get
        //    {
        //        var path = HttpContext.Current.Request.Url.PathAndQuery;

        //        var index = path.IndexOf('?');

        //        if (index == -1)
        //            return path;

        //        path = path.Remove(index);

        //        return path;
        //    }
        //}

        //public static string NoQuerystring
        //{
        //    get
        //    {
        //        var path = HttpContext.Current.Request.Url.AbsoluteUri;

        //        var index = path.IndexOf('?');

        //        if (index == -1)
        //            return path;

        //        path = path.Remove(index);

        //        return path;
        //    }
        //}

        #endregion
    }
}