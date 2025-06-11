#region Using

using Business.Code;
using Business.Entity;
using System.Collections.Generic;
using CommonNetCore.Entity;

#endregion

namespace Business.Collection
{
	public class UtentiCollection : EntityCollectionBase<Utenti, UtentiCollection>
	{
		#region Auto Methods

		/// <summary>
		/// Filtra la collection 'UtentiCollection'
		/// </summary>
		public static UtentiCollection GetList(
			string nome = null,
			string cognome = null,
			string email = null,
			string password = null,
			bool? attivo = null,
			int item4Page = -1, long page = -1, string orderPredicate = null)
		{
			string wherePredicate;
			List<object> whereValues;

			GetQuery(out wherePredicate, out whereValues,
				nome,
				cognome,
				email,
				password,
				attivo);

			return EntityCollectionBase<Utenti, UtentiCollection>.GetList(item4Page, page, string.Join(" AND ", wherePredicate), whereValues.ToArray(), orderPredicate);
		}

		/// <summary>
		/// Prende il numero degli elementi filtrati della collection 'UtentiCollection'
		/// </summary>
		public static long GetCount(
			string nome = null,
			string cognome = null,
			string email = null,
			string password = null,
			bool? attivo = null)
		{
			string wherePredicate;
			List<object> whereValues;

			GetQuery(out wherePredicate, out whereValues,
				nome,
				cognome,
				email,
				password,
				attivo);

			return EntityCollectionBase<Utenti, UtentiCollection>.GetCount(string.Join(" AND ", wherePredicate), whereValues.ToArray());
		}

		/// <summary>
		/// Prende il numero degli elementi filtrati della collection 'UtentiCollection'
		/// </summary>
		public static void GetQuery(out string wherepredicate, out List<object> whereValues, 
			string nome = null,
			string cognome = null,
			string email = null,
			string password = null,
			bool? attivo = null)
		{
			var wherePredicate = new List<string>();
			whereValues = new List<object>();

			if (!string.IsNullOrEmpty(nome))
			{
				wherePredicate.Add("Nome.Contains(@0)");
				whereValues.Add(nome);
			}

			if (!string.IsNullOrEmpty(cognome))
			{
				wherePredicate.Add("Cognome.Contains(@1)");
				whereValues.Add(cognome);
			}

			if (!string.IsNullOrEmpty(email))
			{
				wherePredicate.Add("Email.Contains(@2)");
				whereValues.Add(email);
			}

			if (!string.IsNullOrEmpty(password))
			{
				wherePredicate.Add("Password.Contains(@3)");
				whereValues.Add(password);
			}

			if (attivo != null)
				wherePredicate.Add("Attivo == " + attivo + "");


			wherepredicate = string.Join(" AND ", wherePredicate);
		}


		#endregion
	}
}
