#region Using

using System.Collections.Generic;
using Business.Entity;
using CommonNetCore.Entity;

#endregion

namespace Business.Collection
{
    public class EmailLogCollection : EntityCollectionBase<EmailLog, EmailLogCollection>
    {
        #region Auto Methods

        /// <summary>
        ///     Filtra la collection 'LogCollection'
        /// </summary>
        public static EmailLogCollection GetList(System.DateTime? data = null, string testo = null, int item4Page = -1, long page = -1, string orderPredicate = null)
        {
            var wherePredicate = new List<string>();
            var whereValues = new List<object>();

            if (data != null)
            {
                wherePredicate.Add("Data >= " + data.Value.Date + "");
                wherePredicate.Add("Data <= " + data.Value.Date.AddDays(1).AddMilliseconds(-1) + "");
            }

            if (!string.IsNullOrEmpty(testo))
            {
                wherePredicate.Add("Testo.Contains(@0)");
                whereValues.Add(testo);
            }

            return EntityCollectionBase<EmailLog, EmailLogCollection>.GetList(item4Page, page, string.Join(" AND ", wherePredicate), whereValues.ToArray(), orderPredicate);
        }

        /// <summary>
        ///     Prende il numero degli elementi filtrati della collection 'LogCollection'
        /// </summary>
        public static long GetCount(System.DateTime? giorno = null, string testo = null)
        {
            var wherePredicate = new List<string>();
            var whereValues = new List<object>();

            if (giorno != null)
            {
                wherePredicate.Add("Data >= " + giorno.Value.Date + "");
                wherePredicate.Add("Data <= " + giorno.Value.Date.AddDays(1).AddMilliseconds(-1) + "");
            }

            if (!string.IsNullOrEmpty(testo))
            {
                wherePredicate.Add("Testo.Contains(@0)");
                whereValues.Add(testo);
            }

            return EntityCollectionBase<EmailLog, EmailLogCollection>.GetCount(string.Join(" AND ", wherePredicate), whereValues.ToArray());
        }

        #endregion
    }
}