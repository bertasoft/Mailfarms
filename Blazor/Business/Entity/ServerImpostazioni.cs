#region Using

using System;
using System.Linq;
using System.Threading.Tasks;
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
	public class ServerImpostazioni : EntityBase<ServerImpostazioni>
    {
		#region Enum

        public enum ServerImpostazioniEnum
        {
            DkimDominio,
            DkimPrivateKey,
            DkimSelector,
            DominioTentativoSuccessivoMinuti,
            EmailNumeroMassimoTentativi,
            EmailTentativoSuccessivoMinuti,

            //non metto helo perché viene impostato diversamente per ogni server
        }

        #endregion

        #region Constructors

        #endregion

        #region Fields

        [EntityColumnUnique(false)]
        [DatabaseColumn]
        [StringMaxLength(200, ErrorMessage = "il Nome può essere lungo massimo 200 caratteri")]
        public string Nome { get; set; }

        [DatabaseColumn]
        public string Valore { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Ritorna un oggetto del tipo 'ServerImpostazioni' dal campo 'Nome'
        /// </summary>
        public static ServerImpostazioni GetItem(string nome)
        {
            return GetItem("Nome", nome);
        }

        public static bool Save(ServerImpostazioniEnum impostazioniEnum, string value)
        {
            var impostazione = GetItem(impostazioniEnum);
            impostazione.Valore = value;

            return EntityBase<ServerImpostazioni>.Save(out string avviso, ref impostazione);
        }

        public static string GetValore(ServerImpostazioniEnum impostazioniEnum)
        {
            return GetItem(impostazioniEnum).Valore;
        }

        public static bool Save(out string avviso, ref ServerImpostazioni impostazioni)
        {
            Task.Run(() =>
            {
                var servers = ServerCollection.GetList(attivo: true);

                foreach (var server in servers)
                {
                    Server.AggiornaImpostazioni(server);
                }
            });

            return EntityBase<ServerImpostazioni>.Save(out avviso, ref impostazioni);
        }

        /// Ottiene l'oggetto ServerImpostazioni da una colonna unique
        public static ServerImpostazioni GetItem(ServerImpostazioniEnum impostazioniEnum)
        {
            var impostazione = GetItem("Nome", impostazioniEnum.ToStringFast());

            if (impostazione == null)
            {
                impostazione = new ServerImpostazioni
                {
                    Nome = impostazioniEnum.ToString(),
                    Valore = impostazioniEnum.ToString()
                };

                EntityBase<ServerImpostazioni>.Save(out _, ref impostazione);
            }

            return impostazione;
        }

        public static bool Save(string impostazioni, string valore)
        {
            if (Enum.TryParse(impostazioni, true, out ServerImpostazioniEnum impostazioniEnum))
                return Save(impostazioniEnum, valore);

            return false;
        }

        public static bool Save(out string avviso, string impostazioni, string valore)
        {
            var impostazione = GetItem(impostazioni);

            if (impostazione == null)
                impostazione = new ServerImpostazioni();

            impostazione.Nome = impostazioni;
            impostazione.Valore = valore;

            return EntityBase<ServerImpostazioni>.Save(out avviso, ref impostazione);
        }

        #endregion
    }
}