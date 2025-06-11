using System;
using System.Web;
using Business.Code;
using Business.Collection;
using CommonNetCore.Entity;
using CommonNetCore.Entity.Attribute;
using CommonNetCore.Entity.Validation.Attribute;
using CommonNetCore.GlobalExtension;
using Microsoft.AspNetCore.Http;

namespace Business.Entity
{
    /// <summary>
    ///     Sono gli ip bloccati nella registrazione per editori e inserzionisti, vengono resettati ogni giorno
    /// </summary>
    [DatabaseTable]
    public class BloccoIp : EntityBase<BloccoIp>
    {
        #region Enums

        public enum KeyColumnsEnum
        {
            Id,
            Ip
        }

        #endregion

        #region Constructors

        public BloccoIp()
        {
            Ip = string.Empty;
            UserAgent = string.Empty;
        }

        #endregion

        #region Fields

        [DatabaseColumn]
        [StringNotNullOrEmpty(ErrorMessage = "Assegnare il valore alla proprietà Ip")]
        [EntityColumnUnique(false, ErrorMessage = "L'ip specificato è già presente")]
        public string Ip { get; set; }

        [DatabaseColumn]
        [ObjectNull(ErrorMessage = "Assegnare il valore alla proprietà UserAgent")]
        public string UserAgent { get; set; }

        [DatabaseColumn]
        public DateTime Data { get; set; }

        [DatabaseColumn]
        public int Tentativi { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Prende un oggetto del tipo Utenti da un ip
        /// </summary>
        public static BloccoIp GetItem(string ip)
        {
            return GetItem("Ip", ip);
        }

        /// <summary>
        ///     Salva o aggiorna un oggetto del tipo SpamIp da una colonna di chiave univoca
        /// </summary>
        /// <param name="avviso">Ritorna eventuali avvisi di errore</param>
        /// <param name="spamip">L'oggetto da eliminare</param>
        /// <returns>Ritorna true se l'operazione ha avuto successo, altrimenti false</returns>
        public static bool Save(out string avviso, ref BloccoIp spamip)
        {
            return EntityBase<BloccoIp>.Save(out avviso, ref spamip);
        }

        /// <summary>
        ///     Elimina un oggetto del tipo SpamIp
        /// </summary>
        /// <param name="avviso">Ritorna eventuali avvisi di errore</param>
        /// <param name="spamip">L'oggetto da eliminare</param>
        /// <returns>Ritorna true se l'operazione ha avuto successo, altrimenti false</returns>
        public new static bool Delete(out string avviso, BloccoIp spamip)
        {
            return EntityBase<BloccoIp>.Delete(out avviso, spamip);
        }

        /// <summary>
        ///     Inserisce l'ip nell'elenco bloccati
        /// </summary>
        public static void BloccaIp(string ip, string userAgent = null)
        {
            var spamIp = GetItem(ip);

            if (spamIp == null)
                spamIp = new BloccoIp();

            spamIp.UserAgent = userAgent;

            if (spamIp.UserAgent == null)
                spamIp.UserAgent = "N/A";

            spamIp.Data = DateTime.Now;
            spamIp.Tentativi++;
            spamIp.Ip = ip;

            Save(spamIp);
        }

        /// <summary>
        ///     Se l'ip è bloccato restituisce true, un ip può fare massimo 30 tentativi in mezz'ora,
        ///     l'ip viene sbloccato dopo mezz'ora dall'ultimo tentativo
        /// </summary>
        public static bool IpIsLocked(string ipAddress)
        {
            if (ipAddress.IsNullOrEmpty())
                return false;

            var bloccoIp = GetItem(ipAddress);

            if (bloccoIp != null && bloccoIp.Tentativi > 30)
                return true;

            return false;
        }

        /// <summary>
        ///     Elimina gli ip sbloccabili, che hanno superato la mezz'ora di blocco
        /// </summary>
        public static void ResetSbloccabili()
        {
            foreach (var sbloccabile in BloccoIpCollection.GetListSbloccabili())
                Delete(out _, sbloccabile);
        }

        public static void SbloccaIp(string ipAddress)
        {
            if (ipAddress.IsNullOrEmpty())
                return;

            var bloccoAccesso = GetItem(ipAddress);

            Delete(out _, bloccoAccesso);
        }

        #endregion

        #region Extension Methods

        public static class SpamIpExtension
        {
        }

        #endregion
    }
}