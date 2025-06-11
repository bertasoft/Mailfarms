#region Using

using Business.Code;
using Business.Collection;
using CommonNetCore.Entity;
using CommonNetCore.Entity.Attribute;
using CommonNetCore.Entity.DataTransformation;
using CommonNetCore.Entity.Validation.Attribute;

#endregion

namespace Business.Entity
{
    [DatabaseTable]
    public class UtentiAccessi : EntityBase<UtentiAccessi>
    {
        #region Enum

        #endregion

        #region Constructors

        public UtentiAccessi()
        {
            IpAccesso = string.Empty;
        }

        #endregion

        #region Auto Properties

        [DatabaseColumn]
        [ExternalReferences(typeof(UtentiCollection), "Utenti")]
        private long IdUtenti { get; set; }

        [ObjectNull(ErrorMessage = "Il campo 'Utenti' non può rimanere vuoto")]
        public Utenti Utenti
        {
            get => Utenti.GetItem(IdUtenti);
            set => IdUtenti = value?.Id ?? 0;
        }

        [DatabaseColumn]
        public System.DateTime DataAccesso { get; set; }

        [ObjectNull(ErrorMessage = "Il campo 'IpAccesso' non può rimanere null")]
        [DatabaseColumn]
        public string IpAccesso { get; set; }

        #endregion

        #region Auto Methods

        #endregion

        #region Properties

        #endregion

        #region Methods

        /// <summary>
        ///     Elimina un oggetto del tipo 'UtentiAccessi'
        /// </summary>
        public static bool Delete(UtentiAccessi utentiaccessi)
        {
            return Delete(out _, utentiaccessi);
        }

        /// <summary>
        ///     Elimina un oggetto del tipo 'UtentiAccessi'
        /// </summary>
        public new static bool Delete(out string avviso, UtentiAccessi utentiaccessi)
        {
            return EntityBase<UtentiAccessi>.Delete(out avviso, utentiaccessi);
        }

        /// <summary>
        ///     Salva o aggiorna un oggetto del tipo 'UtentiAccessi'
        /// </summary>
        public static bool Save(out string avviso, ref UtentiAccessi utentiaccessi)
        {
            return EntityBase<UtentiAccessi>.Save(out avviso, ref utentiaccessi);
        }

        #endregion
    }

    #region Extension Methods

    public static class UtentiAccessiExtension
    {
        /// <summary>
        ///     Salva o aggiorna un oggetto del tipo 'UtentiAccessi'
        /// </summary>
        public static bool Save(this UtentiAccessi utentiaccessi)
        {
            if (utentiaccessi == null)
                return false;

            string avviso;
            return UtentiAccessi.Save(out avviso, ref utentiaccessi);
        }

        /// <summary>
        ///     Salva o aggiorna un oggetto del tipo 'UtentiAccessi'
        /// </summary>
        public static bool Save(this UtentiAccessi utentiaccessi, out string avviso)
        {
            if (utentiaccessi == null)
            {
                avviso = "L'entità 'UtentiAccessi' è null";
                return false;
            }

            return UtentiAccessi.Save(out avviso, ref utentiaccessi);
        }

        /// <summary>
        ///     Elimina un oggetto del tipo 'UtentiAccessi'
        /// </summary>
        public static bool Delete(this UtentiAccessi utentiaccessi, out string avviso)
        {
            if (utentiaccessi == null)
            {
                avviso = "L'entità 'UtentiAccessi' è null";
                return false;
            }

            return UtentiAccessi.Delete(out avviso, utentiaccessi);
        }

        /// <summary>
        ///     Elimina un oggetto del tipo 'UtentiAccessi'
        /// </summary>
        public static bool Delete(this UtentiAccessi utentiaccessi)
        {
            return UtentiAccessi.Delete(utentiaccessi);
        }
    }

    #endregion
}