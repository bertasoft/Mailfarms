#region Using

using System;
using Business.Code;
using Business.Collection;
using CommonNetCore.Entity;
using CommonNetCore.Entity.Attribute;
using CommonNetCore.Entity.Validation.Attribute;

#endregion

namespace Business.Entity
{
	[DatabaseTable]
    public class ServerStatistiche : EntityBase<ServerStatistiche>
	{
		#region Enum

		#endregion

		#region Constructors

		public ServerStatistiche()
		{

		}

		#endregion

		#region Auto Properties

        [EntityColumnUniqueComposite("ServerData", ErrorMessage = "Server e data sono già presenti")]
		[DatabaseColumn]
		[ExternalReferences(typeof(ServerCollection), "Server")]
		private long IdServer { get; set; }

		[ObjectNull(ErrorMessage = "Il campo 'Server' non può rimanere vuoto")]
		public Server Server
		{
			get => Server.GetItem(IdServer);
            set => IdServer = value?.Id ?? 0;
        }

		/// <summary>
		/// Il giorno
		/// </summary>
		[EntityColumnUniqueComposite("ServerData", ErrorMessage = "Server e data sono già presenti")]
        [DatabaseColumn]
		public System.DateTime Data { get; set; }

        [DatabaseColumn]
		public long Inviate { get; set; }

		[DatabaseColumn]
		public long Errate { get; set; }

		#endregion

		#region Auto Methods

        public static ServerStatistiche GetItem(Server server, DateTime dateTime)
        {
            if (server == null)
                return null;

            return GetItem("IdServer", server.Id, "Data", dateTime);
        }

        #endregion

		#region Properties

	
		#endregion

		#region Methods

		/// <summary>
		/// Elimina un oggetto del tipo 'ServerStatistiche'
		/// </summary>
		public static bool Delete(ServerStatistiche serverstatistiche)
		{
            return Delete(out _, serverstatistiche);
		}

		/// <summary>
		/// Elimina un oggetto del tipo 'ServerStatistiche'
		/// </summary>
		public new static bool Delete(out string avviso, ServerStatistiche serverstatistiche)
		{
			return EntityBase<ServerStatistiche>.Delete(out avviso, serverstatistiche);
		}

		/// <summary>
		/// Salva o aggiorna un oggetto del tipo 'ServerStatistiche'
		/// </summary>
		public static bool Save(out string avviso, ref ServerStatistiche serverstatistiche)
		{
			return EntityBase<ServerStatistiche>.Save(out avviso, ref serverstatistiche);
		}

        public static void NuovaInviata(string ip)
        {
            var server = Server.GetItem(ip);

			if (server == null)
				return;

            server.Inviate++;
            server.Save();

            var stat = GetItem(server, DateTime.Today);

			if (stat == null)
				stat = new ServerStatistiche();

            stat.Server = server;
			stat.Data = DateTime.Today;
            stat.Inviate += 1;

            stat.Save();
        }

        public static void NuovaErrata(string ip)
        {
            var server = Server.GetItem(ip);

            if (server == null)
                return;

            server.Errate++;
            server.Save();

            var stat = GetItem(server, DateTime.Today);

            if (stat == null)
                stat = new ServerStatistiche();

			stat.Server = server;
            stat.Data = DateTime.Today;
			stat.Errate += 1;

            stat.Save();
        }

		#endregion
	}

	#region Extension Methods

	public static class ServerStatisticheExtension
	{
		/// <summary>
		/// Salva o aggiorna un oggetto del tipo 'ServerStatistiche'
		/// </summary>
		public static bool Save(this ServerStatistiche serverstatistiche)
		{
			if (serverstatistiche == null)
				return false;

			string avviso;
			return ServerStatistiche.Save(out avviso, ref serverstatistiche);
		}

		/// <summary>
		/// Salva o aggiorna un oggetto del tipo 'ServerStatistiche'
		/// </summary>
		public static bool Save(this ServerStatistiche serverstatistiche, out string avviso)
		{
			if (serverstatistiche == null)
			{
				avviso = "L'entità 'ServerStatistiche' è null";
				return false;
			}

			return ServerStatistiche.Save(out avviso, ref serverstatistiche);
		}

		/// <summary>
		/// Elimina un oggetto del tipo 'ServerStatistiche'
		/// </summary>
		public static bool Delete(this ServerStatistiche serverstatistiche, out string avviso)
		{
			if (serverstatistiche == null)
			{
				avviso = "L'entità 'ServerStatistiche' è null";
				return false;
			}

			return ServerStatistiche.Delete(out avviso, serverstatistiche);
		}

		/// <summary>
		/// Elimina un oggetto del tipo 'ServerStatistiche'
		/// </summary>
		public static bool Delete(this ServerStatistiche serverstatistiche)
		{
			return ServerStatistiche.Delete(serverstatistiche);
		}
	}

	#endregion
}