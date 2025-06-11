#region Using

using Business.Code;
using Business.Entity;
using System.Collections.Generic;
using CommonNetCore.Entity;

#endregion

namespace Business.Collection
{
	public class ServerCollection : EntityCollectionBase<Server, ServerCollection>
	{
		#region Auto Methods

		/// <summary>
		/// Filtra la collection 'ServerCollection'
		/// </summary>
		public static ServerCollection GetList(
			string ipPorta = null,
            bool? riceve = null,
			bool? attivo = null,
			Server serverStatisticheServer = null,
			System.DateTime? serverStatisticheData = null,
			System.DateTime? serverStatisticheDataStart = null,
			System.DateTime? serverStatisticheDataEnd = null,
			long? serverStatisticheInviate = null,
			long? serverStatisticheErrate = null,
			int item4Page = -1, long page = -1, string orderPredicate = null)
		{
			string wherePredicate;
			List<object> whereValues;

			GetQuery(out wherePredicate, out whereValues,
				ipPorta,
				riceve,
				attivo,
				serverStatisticheServer,
				serverStatisticheData,
				serverStatisticheDataStart,
				serverStatisticheDataEnd,
				serverStatisticheInviate,
				serverStatisticheErrate);

			return EntityCollectionBase<Server, ServerCollection>.GetList(item4Page, page, string.Join(" AND ", wherePredicate), whereValues.ToArray(), orderPredicate);
		}

		/// <summary>
		/// Prende il numero degli elementi filtrati della collection 'ServerCollection'
		/// </summary>
		public static long GetCount(
			string ipPorta = null,
            bool? riceve = null,
			bool? attivo = null,
			Server serverStatisticheServer = null,
			System.DateTime? serverStatisticheData = null,
			System.DateTime? serverStatisticheDataStart = null,
			System.DateTime? serverStatisticheDataEnd = null,
			long? serverStatisticheInviate = null,
			long? serverStatisticheErrate = null)
		{
			string wherePredicate;
			List<object> whereValues;

			GetQuery(out wherePredicate, out whereValues,
				ipPorta,
				riceve,
				attivo,
				serverStatisticheServer,
				serverStatisticheData,
				serverStatisticheDataStart,
				serverStatisticheDataEnd,
				serverStatisticheInviate,
				serverStatisticheErrate);

			return EntityCollectionBase<Server, ServerCollection>.GetCount(string.Join(" AND ", wherePredicate), whereValues.ToArray());
		}

		/// <summary>
		/// Prende il numero degli elementi filtrati della collection 'ServerCollection'
		/// </summary>
		public static void GetQuery(out string wherepredicate, out List<object> whereValues, 
			string ipPorta = null,
            bool? riceve = null,
			bool? attivo = null,
			Server serverStatisticheServer = null,
			System.DateTime? serverStatisticheData = null,
			System.DateTime? serverStatisticheDataStart = null,
			System.DateTime? serverStatisticheDataEnd = null,
			long? serverStatisticheInviate = null,
			long? serverStatisticheErrate = null)
		{
			var wherePredicate = new List<string>();
			whereValues = new List<object>();

			if (!string.IsNullOrEmpty(ipPorta))
			{
				wherePredicate.Add("IpPorta.Contains(@0)");
				whereValues.Add(ipPorta);
			}

            if (riceve != null)
                wherePredicate.Add("Riceve == " + riceve + "");

            if (attivo != null)
				wherePredicate.Add("Attivo == " + attivo + "");

			if (serverStatisticheServer != null)
				wherePredicate.Add("ServerStatistiche.IdServer == " + serverStatisticheServer.Id + "");

			if (serverStatisticheData != null)
				wherePredicate.Add("ServerStatistiche.Data == " + serverStatisticheData.Value);

			if (serverStatisticheDataStart != null)
				wherePredicate.Add("ServerStatistiche.Data >= " + serverStatisticheDataStart.Value.Date);

			if (serverStatisticheDataEnd != null)
				wherePredicate.Add("ServerStatistiche.Data < " + serverStatisticheDataEnd.Value.Date.AddDays(1));

			if (serverStatisticheInviate != null)
				wherePredicate.Add("ServerStatistiche.Inviate == " + serverStatisticheInviate + "");

			if (serverStatisticheErrate != null)
				wherePredicate.Add("ServerStatistiche.Errate == " + serverStatisticheErrate + "");


			wherepredicate = string.Join(" AND ", wherePredicate);
		}


		#endregion
	}
}
