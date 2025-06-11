#region Using

using System.Web;
using Business.Code;
using CommonNetCore.Entity;
using CommonNetCore.Entity.Attribute;
using CommonNetCore.Entity.Validation.Attribute;

#endregion

namespace Business.Entity
{
    [DatabaseTable]
    public class Etichette : EntityBase<Etichette>
    {
        #region Enum

        public enum EtichetteEnum
        {
            EmailRecuperaDatiAccessoContenuto,
            EmailRecuperaDatiAccessoOggetto,
            EmailFooter,
            EmailAccessoBloccatoOggetto,
            EmailAccessoBloccatoContenuto,
        }

        #endregion

        #region Constructors

        public Etichette()
        {
            Nome = string.Empty;
        }

        #endregion

        #region Auto Properties

        //Questa region viene creata automaticamente, eventuali modifiche andranno perse se ricostruita tramite tool

        [DatabaseColumn]
        [EntityColumnUnique(false, ErrorMessage = "Il campo 'Nome' contiene un valore già salvato")]
        [ObjectNull(ErrorMessage = "Il campo 'Nome' non può rimanere null")]
        public string Nome { get; set; }

        [DatabaseColumn]
        [StringNotNullOrEmpty(ErrorMessage = "Il campo 'Valore' non può rimanere vuoto")]
        public string Valore { get; set; }

        #endregion

        #region Auto Methods

        /// <summary>
        ///     Ritorna un oggetto del tipo 'Etichette' dal campo 'Nome'
        /// </summary>
        public static bool Delete(Etichette etichette)
        {
            return Delete(out _, etichette);
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        /// <summary>
        ///     Prende direttamente il valore dell'etichetta
        /// </summary>
        public static string GetValore(EtichetteEnum etichetteEnum, bool commonReplace = true)
        {
            if (!commonReplace)
                return GetItem(etichetteEnum.ToString()).Valore;

            return GetItem(etichetteEnum.ToString()).Valore.CommonReplace();
        }

        /// <summary>
        ///     Prende direttamente il valore dell'etichetta
        /// </summary>
        public static string GetValoreJsEncode(EtichetteEnum etichetteEnum)
        {
            return HttpUtility.JavaScriptStringEncode(GetItem(etichetteEnum.ToString()).Valore).CommonReplace();
        }

        /// <summary>
        ///     Prende l'etichetta dall'enumeratore e dalla lingua correntemente selezionata
        /// </summary>
        public static Etichette GetItem(EtichetteEnum etichetteEnum)
        {
            return GetItem(etichetteEnum.ToString());
        }

        /// <summary>
        ///     Legge un valore dall'etichetta, se replace è true codifica i tag interni
        /// </summary>
        public static Etichette GetItem(string nomeEtichetta)
        {
            var etichette = GetItem("Nome", nomeEtichetta);

            if (etichette == null)
            {
                etichette = new Etichette();
                etichette.Nome = nomeEtichetta;
                etichette.Valore = nomeEtichetta;
                Save(etichette);
            }

            if (string.IsNullOrEmpty(etichette.Valore))
            {
                etichette.Valore = nomeEtichetta;
                Save(etichette);
            }

            return etichette;
        }

        public new static bool Save(Etichette etichette)
        {
            return Save(out _, ref etichette);
        }

        public static bool Save(out string avviso, ref Etichette etichette)
        {
            //Prima salvo la categoria, recupero l'id 
            return EntityBase<Etichette>.Save(out avviso, ref etichette);
        }

        #endregion
    }

    #region Extension Methods

    public static class EtichetteExtension
    {
        /// <summary>
        ///     Salva o aggiorna un oggetto del tipo 'Etichette'
        /// </summary>
        public static bool Save(this Etichette etichette, out string avviso)
        {
            if (etichette == null)
            {
                avviso = "L'entità 'Etichette' è null";
                return false;
            }

            return Etichette.Save(out avviso, ref etichette);
        }

        /// <summary>
        ///     Elimina un oggetto del tipo 'Etichette'
        /// </summary>
        public static bool Delete(this Etichette etichette, out string avviso)
        {
            if (etichette == null)
            {
                avviso = "L'entità 'Etichette' è null";
                return false;
            }

            return Etichette.Delete(out avviso, etichette);
        }

        /// <summary>
        ///     Elimina un oggetto del tipo 'Etichette'
        /// </summary>
        public static bool Delete(this Etichette etichette)
        {
            return Etichette.Delete(etichette);
        }
    }

    #endregion
}