#region Using

using System.Collections.Generic;
using Business.Entity;
using CommonNetCore.Entity;

#endregion

namespace Business.Collection
{
    public class ImpostazioniCollection : EntityCollectionBase<Impostazioni, ImpostazioniCollection>
    {
        #region Auto Methods

        /// <summary>
        ///     Filtra la collection 'ImpostazioniCollection'
        /// </summary>
        public static ImpostazioniCollection GetList(string nome = null, string valore = null, int item4Page = -1, long page = -1, string orderPredicate = null)
        {
            var wherePredicate = new List<string>();
            var whereValues = new List<object>();

            if (!string.IsNullOrEmpty(nome))
            {
                wherePredicate.Add("Nome.Contains(@0)");
                whereValues.Add(nome);
            }

            if (!string.IsNullOrEmpty(valore))
            {
                wherePredicate.Add("Valore.Contains(@1)");
                whereValues.Add(valore);
            }

            return EntityCollectionBase<Impostazioni, ImpostazioniCollection>.GetList(item4Page, page, string.Join(" AND ", wherePredicate), whereValues.ToArray(), orderPredicate);
        }

        /// <summary>
        ///     Prende il numero degli elementi filtrati della collection 'ImpostazioniCollection'
        /// </summary>
        public static long GetCount(string nome = null, string valore = null)
        {
            var wherePredicate = new List<string>();
            var whereValues = new List<object>();

            if (!string.IsNullOrEmpty(nome))
            {
                wherePredicate.Add("Nome.Contains(@0)");
                whereValues.Add(nome);
            }

            if (!string.IsNullOrEmpty(valore))
            {
                wherePredicate.Add("Valore.Contains(@1)");
                whereValues.Add(valore);
            }

            return EntityCollectionBase<Impostazioni, ImpostazioniCollection>.GetCount(string.Join(" AND ", wherePredicate), whereValues.ToArray());
        }

        #endregion
    }
}