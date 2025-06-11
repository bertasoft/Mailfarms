#region Using

using Business.Code;
using Business.Entity;
using System.Collections.Generic;
using CommonNetCore.Entity;

#endregion

namespace Business.Collection
{
	public class ServerStatisticheCollection : EntityCollectionBase<ServerStatistiche, ServerStatisticheCollection>
	{
		#region Auto Methods

		/// <summary>
		/// Filtra la collection 'ServerStatisticheCollection'
		/// </summary>
		public static ServerStatisticheCollection GetList(
			Server server = null,
			System.DateTime? data = null,
			System.DateTime? dataStart = null,
			System.DateTime? dataEnd = null,
			long? inviate = null,
			long? errate = null,
			string serverIpPorta = null,
			bool? serverAttivo = null,
			int item4Page = -1, long page = -1, string orderPredicate = null)
		{
			string wherePredicate;
			List<object> whereValues;

			GetQuery(out wherePredicate, out whereValues,
				server,
				data,
				dataStart,
				dataEnd,
				inviate,
				errate,
				serverIpPorta,
				serverAttivo);

			return EntityCollectionBase<ServerStatistiche, ServerStatisticheCollection>.GetList(item4Page, page, string.Join(" AND ", wherePredicate), whereValues.ToArray(), orderPredicate);
		}

		/// <summary>
		/// Prende il numero degli elementi filtrati della collection 'ServerStatisticheCollection'
		/// </summary>
		public static long GetCount(
			Server server = null,
			System.DateTime? data = null,
			System.DateTime? dataStart = null,
			System.DateTime? dataEnd = null,
			long? inviate = null,
			long? errate = null,
			string serverIpPorta = null,
			bool? serverAttivo = null)
		{
			string wherePredicate;
			List<object> whereValues;

			GetQuery(out wherePredicate, out whereValues,
				server,
				data,
				dataStart,
				dataEnd,
				inviate,
				errate,
				serverIpPorta,
				serverAttivo);

			return EntityCollectionBase<ServerStatistiche, ServerStatisticheCollection>.GetCount(string.Join(" AND ", wherePredicate), whereValues.ToArray());
		}

		/// <summary>
		/// Prende il numero degli elementi filtrati della collection 'ServerStatisticheCollection'
		/// </summary>
		public static void GetQuery(out string wherepredicate, out List<object> whereValues, 
			Server server = null,
			System.DateTime? data = null,
			System.DateTime? dataStart = null,
			System.DateTime? dataEnd = null,
			long? inviate = null,
			long? errate = null,
			string serverIpPorta = null,
			bool? serverAttivo = null)
		{
			var wherePredicate = new List<string>();
			whereValues = new List<object>();

			if (server != null)
				wherePredicate.Add("IdServer == " + server.Id + "");

			if (data != null)
				wherePredicate.Add("Data == " + data.Value);

			if (dataStart != null)
				wherePredicate.Add("Data >= " + dataStart.Value.Date);

			if (dataEnd != null)
				wherePredicate.Add("Data < " + dataEnd.Value.Date.AddDays(1));

			if (inviate != null)
				wherePredicate.Add("Inviate == " + inviate + "");

			if (errate != null)
				wherePredicate.Add("Errate == " + errate + "");

			if (!string.IsNullOrEmpty(serverIpPorta))
			{
				wherePredicate.Add("Server.IpPorta.Contains(@0)");
				whereValues.Add(serverIpPorta);
			}

			if (serverAttivo != null)
				wherePredicate.Add("Server.Attivo == " + serverAttivo + "");


			wherepredicate = string.Join(" AND ", wherePredicate);
		}


		#endregion
	}
}
