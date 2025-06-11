#region Using

using System.Collections.Generic;
using Business.Code;
using Business.Entity;
using CommonNetCore.Entity;

#endregion

namespace Business.Collection
{
    public class EtichetteCollection : EntityCollectionBase<Etichette, EtichetteCollection>
    {
        #region Auto Methods

        /// <summary>
        ///     Filtra la collection 'EtichetteCollection'
        /// </summary>
        public static EtichetteCollection GetList(string nome = null, int item4Page = -1, long page = -1, string orderPredicate = null)
        {
            var wherePredicate = new List<string>();
            var whereValues = new List<object>();

            if (!string.IsNullOrEmpty(nome))
            {
                wherePredicate.Add("Nome.Contains(@0)");
                whereValues.Add(nome);
            }

            return EntityCollectionBase<Etichette, EtichetteCollection>.GetList(item4Page, page, string.Join(" AND ", wherePredicate), whereValues.ToArray(), orderPredicate);
        }

        /// <summary>
        ///     Prende il numero degli elementi filtrati della collection 'EtichetteCollection'
        /// </summary>
        public static long GetCount(string nome = null)
        {
            var wherePredicate = new List<string>();
            var whereValues = new List<object>();

            if (!string.IsNullOrEmpty(nome))
            {
                wherePredicate.Add("Nome.Contains(@0)");
                whereValues.Add(nome);
            }

            return EntityCollectionBase<Etichette, EtichetteCollection>.GetCount(string.Join(" AND ", wherePredicate), whereValues.ToArray());
        }

        #endregion
    }
}