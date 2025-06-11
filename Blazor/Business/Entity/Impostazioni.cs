#region

using System;
using CommonNetCore.Entity;
using CommonNetCore.Entity.Attribute;
using CommonNetCore.Entity.Validation.Attribute;
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
            ScadenzaGiorni,
            SkebbyApiUrl,
            SkebbyUserKey,
            SkebbyAccessToken
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
        ///     Ritorna un oggetto del tipo 'Impostazioni' dal campo 'Nome'
        /// </summary>
        public static Impostazioni GetItem(string nome)
        {
            return GetItem("Nome", nome);
        }

        public static bool Save(ImpostazioniEnum impostazioniEnum, string value)
        {
            var impostazione = GetItem(impostazioniEnum);
            impostazione.Valore = value;

            return EntityBase<Impostazioni>.Save(out string avviso, ref impostazione);
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
            var impostazione =  GetItem("Nome", impostazioniEnum.ToStringFast());

            if (impostazione == null)
            {
                impostazione = new Impostazioni
                {
                    Nome = impostazioniEnum.ToString(),
                    Valore = impostazioniEnum.ToString()
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