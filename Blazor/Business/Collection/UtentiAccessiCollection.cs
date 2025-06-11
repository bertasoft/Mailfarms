#region Using

using System.Collections.Generic;
using Business.Code;
using Business.Entity;
using CommonNetCore.Entity;

#endregion

namespace Business.Collection
{
    public class UtentiAccessiCollection : EntityCollectionBase<UtentiAccessi, UtentiAccessiCollection>
    {
        #region Auto Methods

        /// <summary>
        ///     Filtra la collection 'UtentiAccessiCollection'
        /// </summary>
        public static UtentiAccessiCollection GetList(
            System.DateTime? dataAccessoInizio = null, System.DateTime? dataAccessoFine = null,
            string nome = null, string cognome = null, string email = null,
            Utenti utenti = null, string ipaccesso = null,
            int item4Page = -1, long page = -1, string orderPredicate = null
            )
        {
            var wherePredicate = new List<string>();
            var whereValues = new List<object>();

            if (dataAccessoInizio != null)
                wherePredicate.Add("DataAccesso >= " + dataAccessoInizio + "");

            if (dataAccessoFine != null)
                wherePredicate.Add("DataAccesso <= " + dataAccessoFine.Value.Date.AddDays(1).AddMilliseconds(-1) + "");

            if (!string.IsNullOrEmpty(nome))
            {
                wherePredicate.Add("Utenti.Nome.Contains(@0)");
                whereValues.Add(nome);
            }

            if (!string.IsNullOrEmpty(cognome))
            {
                wherePredicate.Add("Utenti.Cognome.Contains(@1)");
                whereValues.Add(cognome);
            }

            if (!string.IsNullOrEmpty(email))
            {
                wherePredicate.Add("Utenti.Email.Contains(@2)");
                whereValues.Add(email);
            }

            if (!string.IsNullOrEmpty(ipaccesso))
            {
                wherePredicate.Add("IpAccesso.Contains(@3)");
                whereValues.Add(ipaccesso);
            }

            if (utenti != null)
                wherePredicate.Add("IdUtenti == " + utenti.Id + "");

            return EntityCollectionBase<UtentiAccessi, UtentiAccessiCollection>.GetList(item4Page, page, string.Join(" AND ", wherePredicate), whereValues.ToArray(), orderPredicate);
        }

        /// <summary>
        ///     Prende il numero degli elementi filtrati della collection 'UtentiAccessiCollection'
        /// </summary>
        public static long GetCount(System.DateTime? dataAccessoInizio = null, System.DateTime? dataAccessoFine = null,
            string nome = null, string cognome = null, string email = null,
            Utenti utenti = null, string ipaccesso = null)
        {
            var wherePredicate = new List<string>();
            var whereValues = new List<object>();

            if (dataAccessoInizio != null)
                wherePredicate.Add("DataAccesso >= " + dataAccessoInizio + "");

            if (dataAccessoFine != null)
                wherePredicate.Add("DataAccesso <= " + dataAccessoFine.Value.Date.AddDays(1).AddMilliseconds(-1) + "");

            if (!string.IsNullOrEmpty(nome))
            {
                wherePredicate.Add("Utenti.Nome.Contains(@0)");
                whereValues.Add(nome);
            }

            if (!string.IsNullOrEmpty(cognome))
            {
                wherePredicate.Add("Utenti.Cognome.Contains(@1)");
                whereValues.Add(cognome);
            }

            if (!string.IsNullOrEmpty(email))
            {
                wherePredicate.Add("Utenti.Email.Contains(@2)");
                whereValues.Add(email);
            }

            if (!string.IsNullOrEmpty(ipaccesso))
            {
                wherePredicate.Add("IpAccesso.Contains(@3)");
                whereValues.Add(ipaccesso);
            }

            if (utenti != null)
                wherePredicate.Add("IdUtenti == " + utenti.Id + "");

            return EntityCollectionBase<UtentiAccessi, UtentiAccessiCollection>.GetCount(string.Join(" AND ", wherePredicate), whereValues.ToArray());
        }

        #endregion
    }
}