#region Using

using System;
using System.Linq;
using Business.Collection;
using CommonNetCore.Entity;
using CommonNetCore.Entity.Attribute;
using CommonNetCore.Entity.DataTransformation;
using CommonNetCore.Entity.Validation.Attribute;
using CommonNetCore.GlobalExtension;

#endregion

namespace Business.Entity
{
	/// <summary>
	/// Le mail che hanno questi domini non possono essere inviate da questo server
	/// </summary>
	[DatabaseTable]
	public class ServerDominiBannati : EntityBase<ServerDominiBannati>
	{
		#region Enum

		#endregion

		#region Constructors

		public ServerDominiBannati()
		{
			Dominio = string.Empty;
		}

		#endregion

		#region Auto Properties

        [DatabaseColumn]
        [ExternalReferences(typeof(ServerCollection), "Server")]
        [EntityColumnUniqueComposite("ServerDominio", ErrorMessage = "Il campo 'Dominio' contiene un valore già salvato in questo server")]
		private long IdServer { get; set; }

        [ObjectNull(ErrorMessage = "Il campo 'Server' non può rimanere vuoto")]
        public Server Server
        {
            get => Server.GetItem(IdServer);
            set => IdServer = value?.Id ?? 0;
        }

        [Lower]
        [StringDomainUrl]
		[StringNotNullOrEmpty(ErrorMessage = "Il campo 'Dominio' non può rimanere vuoto")]
		[EntityColumnUniqueComposite("ServerDominio", ErrorMessage = "Il campo 'Dominio' contiene un valore già salvato in questo server")]
		[DatabaseColumn]
		public string Dominio { get; set; }

		#endregion

		#region Auto Methods

		/// <summary>
		/// Ritorna un oggetto del tipo 'DominiBannati' dal campo 'Dominio'
		/// </summary>
		public static ServerDominiBannati GetItem(Server server, string dominio)
        {
            if (server == null)
                return null;

			return GetItem("IdServer", server.Id, "Dominio", dominio);
		}

		#endregion

		#region Properties

	
		#endregion

		#region Methods

        /// <summary>
		/// Elimina un oggetto del tipo 'DominiBannati'
		/// </summary>
		public static bool Delete(ServerDominiBannati dominibannati)
		{
		    return Delete(out _, dominibannati);
		}

		/// <summary>
		/// Elimina un oggetto del tipo 'DominiBannati'
		/// </summary>
		public new static bool Delete(out string avviso, ServerDominiBannati dominibannati)
		{
		    return EntityBase<ServerDominiBannati>.Delete(out avviso, dominibannati);
		}

        /// <summary>
        /// Se questa mail non può essere inviata con questo server
        /// </summary>
        public static bool EmailBannata(Server server, string destinatarioEmail)
        {
            if (server == null || destinatarioEmail.IsNullOrEmpty())
                return false;

            var dominio = Email.GetDomain(destinatarioEmail);

            return GetItem(server, dominio) != null;
        }

        /// <summary>
        /// Se questa mail non può essere inviata con questo server
        /// </summary>
        public static bool DominioBannato(Server server, string dominio)
        {
            if (server == null || dominio.IsNullOrEmpty())
                return false;

            return GetItem(server, dominio) != null;
        }

		/// <summary>
		/// Salva o aggiorna un oggetto del tipo 'DominiBannati'
		/// </summary>
		public static bool Save(out string avviso, ref ServerDominiBannati dominibannati)
		{
		    avviso = string.Empty;

		    if (dominibannati == null || string.IsNullOrEmpty(dominibannati.Dominio))
		        return false;

		    if (!dominibannati.Dominio.Contains(".") || dominibannati.Dominio.Count(p => p == '.') > 1)
		    {
		        avviso = "Il dominio deve essere nel formato dominio.ext";
		        return false;
		    }

            return EntityBase<ServerDominiBannati>.Save(out avviso, ref dominibannati);
		}

		#endregion
	}

	#region Extension Methods

	public static class DominiBannatiExtension
	{
		/// <summary>
		/// Salva o aggiorna un oggetto del tipo 'DominiBannati'
		/// </summary>
		public static bool Save(this ServerDominiBannati dominibannati)
		{
			if (dominibannati == null)
				return false;

            return ServerDominiBannati.Save(out _, ref dominibannati);
		}

		/// <summary>
		/// Salva o aggiorna un oggetto del tipo 'DominiBannati'
		/// </summary>
		public static bool Save(this ServerDominiBannati dominibannati, out string avviso)
		{
			if (dominibannati == null)
			{
				avviso = "L'entità 'DominiBannati' è null";
				return false;
			}

			return ServerDominiBannati.Save(out avviso, ref dominibannati);
		}

		/// <summary>
		/// Elimina un oggetto del tipo 'DominiBannati'
		/// </summary>
		public static bool Delete(this ServerDominiBannati dominibannati, out string avviso)
		{
			if (dominibannati == null)
			{
				avviso = "L'entità 'DominiBannati' è null";
				return false;
			}

			return ServerDominiBannati.Delete(out avviso, dominibannati);
		}

		/// <summary>
		/// Elimina un oggetto del tipo 'DominiBannati'
		/// </summary>
		public static bool Delete(this ServerDominiBannati dominibannati)
		{
			return ServerDominiBannati.Delete(dominibannati);
		}
	}

	#endregion
}