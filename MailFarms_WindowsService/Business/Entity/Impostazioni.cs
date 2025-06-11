#region

#region Creazione e modifica

// Data di creazione 21:15 04/01/2014
// Ultima modifica 19:15 28/07/2014

#endregion

using System;
using CommonNetCore.Entity;
using CommonNetCore.Entity.Attribute;
using CommonNetCore.GlobalExtension;

#endregion

namespace Business.Entity
{
    [DatabaseTable]
    public class Impostazioni : EntityBase<Impostazioni>
    {
        #region Enums

        public enum ImpostazioniEnum
        {
            Helo,
            DkimDominio,
            DkimPrivateKey,
            DkimSelector,
            DominioTentativoSuccessivoMinuti,
            EmailNumeroMassimoTentativi,
            EmailTentativoSuccessivoMinuti,

            /// <summary>
            /// Scollegato dalle impostazioni, viene impostato singolarmente quando in mailfarms.com viene aggiunto il record o modificato l'ip
            /// </summary>
            IndirizzoIp,
            EmailTotaliDaInizio            
        }

        #endregion

        #region Constructors

        #endregion

        #region Fields

        [EntityColumnUnique(false, ErrorMessage = "Il nome è già utilizzato")]
        [DatabaseColumn]
        public string Nome { get; set; }

        [DatabaseColumn]
        public string Valore { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Ritorna un oggetto del tipo 'Impostazioni' dal campo 'Nome'
        /// </summary>
        public static Impostazioni GetItem(string nome)
        {
            return GetItem("Nome", nome);
        }

        /// <summary>
        ///     Ritorna un oggetto del tipo 'Impostazioni' dal campo 'Nome'
        /// </summary>
        public static Impostazioni GetItemRemote(string nome)
        {
            var item = GetItem(nome);

            if (item == null)
            {
                item = new Impostazioni
                {
                    Nome = nome,
                    Valore = nome
                };

                if (!Save(item))
                    return null;
            }

            return GetItem(nome);
        }

        public static bool Save(ImpostazioniEnum impostazioniEnum, string value)
        {
            var impostazione = GetItem(impostazioniEnum);
            impostazione.Valore = value;

            return EntityBase<Impostazioni>.Save(out _, ref impostazione);
        }

        public static string GetValore(ImpostazioniEnum impostazioniEnum)
        {
            return GetItem(impostazioniEnum).Valore;
        }

        public static bool Save(out string avviso, ref Impostazioni impostazioni)
        {
            return EntityBase<Impostazioni>.Save(out avviso, ref impostazioni);
        }

        /// Ottiene l'oggetto Impostazioni da una colonna unique
        public static Impostazioni GetItem(ImpostazioniEnum impostazioniEnum)
        {
            var impostazioneString = impostazioniEnum.ToStringFast();

            var impostazione =  GetItem("Nome", impostazioneString);

            if (impostazione == null)
            {
                impostazione = new Impostazioni
                {
                    Nome = impostazioneString,
                    Valore = impostazioneString
                };

                EntityBase<Impostazioni>.Save(out _, ref impostazione);
            }

            return impostazione;
        }

        public static bool Save(string impostazioni, string valore)
        {
            if (Enum.TryParse(impostazioni, true, out ImpostazioniEnum impostazioniEnum))
                return Save(impostazioniEnum, valore);

            return false;
        }

        public static bool Save(out string avviso, string impostazioni, string valore)
        {
            var impostazione = GetItem(impostazioni);

            if (impostazione == null)
                impostazione = new Impostazioni();

            impostazione.Nome = impostazioni;
            impostazione.Valore = valore;

            return EntityBase<Impostazioni>.Save(out avviso, ref impostazione);
        }

        #endregion
    }

    #region Extension Methods

    public static class ImpostazioniExtension
    {
    }

    #endregion
}