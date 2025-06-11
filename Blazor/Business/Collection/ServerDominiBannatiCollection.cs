#region Using

using Business.Entity;
using System.Collections.Generic;
using CommonNetCore.Entity;

#endregion

namespace Business.Collection
{
	public class ServerDominiBannatiCollection : EntityCollectionBase<ServerDominiBannati, ServerDominiBannatiCollection>
	{
		#region Auto Methods

		/// <summary>
		/// Filtra la collection 'DominiBannatiCollection'
		/// </summary>
		public static ServerDominiBannatiCollection GetList(string dominio = null, int item4Page = -1, long page = -1, string orderPredicate = null)
		{
			var wherePredicate = new List<string>();
			var whereValues = new List<object>();

			if (!string.IsNullOrEmpty(dominio))
			{
				wherePredicate.Add("Dominio.Contains(@0)");
				whereValues.Add(dominio);
			}

			return EntityCollectionBase<ServerDominiBannati, ServerDominiBannatiCollection>.GetList(item4Page, page, string.Join(" AND ", wherePredicate), whereValues.ToArray(), orderPredicate);
		}

		/// <summary>
		/// Prende il numero degli elementi filtrati della collection 'DominiBannatiCollection'
		/// </summary>
		public static long GetCount(string dominio = null)
		{
			var wherePredicate = new List<string>();
			var whereValues = new List<object>();

			if (!string.IsNullOrEmpty(dominio))
			{
				wherePredicate.Add("Dominio.Contains(@0)");
				whereValues.Add(dominio);
			}

			return EntityCollectionBase<ServerDominiBannati, ServerDominiBannatiCollection>.GetCount(string.Join(" AND ", wherePredicate), whereValues.ToArray());
		}

		#endregion
	}
}
