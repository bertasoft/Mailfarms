#region

using System;
using Business.Code;
using CommonNetCore.Entity;
using CommonNetCore.Entity.Attribute;
using CommonNetCore.Entity.Validation.Attribute;

#endregion

namespace Business.Entity
{
    /// <summary>
    ///     Tiene traccia delle email bannate
    /// </summary>
    [DatabaseTable]
    public class BloccoEmail : EntityBase<BloccoEmail>
    {
        #region Enum

        #endregion

        #region Constructors

        public BloccoEmail()
        {
            Email = string.Empty;
        }

        #endregion

        #region Auto Properties

        [EntityColumnUnique(false, ErrorMessage = "Il valore specificato per il campo email è già presente")]
        [StringNotNullOrEmpty(ErrorMessage = "Inserire un indirizzo email valido")]
        [StringEmail(ErrorMessage = "L'indirizzo email deve essere valido")]
        [StringMaxLength(200, ErrorMessage = "L'email può essere lunga massimo 200 caratteri")]
        [DatabaseColumn]
        public string Email { get; set; }
        
        [DatabaseColumn]
        public int Tentativi { get; set; }

        [DatabaseColumn]
        public DateTime Data { get; set; }

        #endregion

        #region Auto Methods

        public static BloccoEmail GetItem(string RegistrazioniEmailBloccate)
        {
            return GetItem("Email", RegistrazioniEmailBloccate);
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        /// <summary>
        ///     Elimina un oggetto del tipo 'RegistrazioniEmailBloccate'
        /// </summary>
        public static bool Delete(BloccoEmail registrazioniEmailBloccate)
        {
            string avviso;
            return Delete(out avviso, registrazioniEmailBloccate);
        }

        /// <summary>
        ///     Elimina un oggetto del tipo 'RegistrazioniEmailBloccate'
        /// </summary>
        public new static bool Delete(out string avviso, BloccoEmail registrazioniEmailBloccate)
        {
            return EntityBase<BloccoEmail>.Delete(out avviso, registrazioniEmailBloccate);
        }

        /// <summary>
        ///     Salva o aggiorna un oggetto del tipo 'RegistrazioniEmailBloccate'
        /// </summary>
        public static bool Save(out string avviso, ref BloccoEmail registrazioniEmailBloccate)
        {
            return EntityBase<BloccoEmail>.Save(out avviso, ref registrazioniEmailBloccate);
        }

        /// <summary>
        ///     Aggiunge o incrementa i tentativi di una email bloccata
        /// </summary>
        public static void BloccaEmail(string email, string notifica = null)
        {
            var emailBloccate = GetItem(email) ?? new BloccoEmail
            {
                Email = email,
                Tentativi = 0,
                Data = DateTime.Now
            };

            emailBloccate.Tentativi++;

            Save(emailBloccate);
        }

        #endregion
    }

    #region Extension Methods

    public static class RegistrazioniEmailBloccateExtension
    {
        /// <summary>
        ///     Salva o aggiorna un oggetto del tipo 'RegistrazioniEmailBloccate'
        /// </summary>
        public static bool Save(this BloccoEmail RegistrazioniEmailBloccate, out string avviso)
        {
            if (RegistrazioniEmailBloccate == null)
            {
                avviso = "L'entità 'RegistrazioniEmailBloccate' è null";
                return false;
            }

            return BloccoEmail.Save(out avviso, ref RegistrazioniEmailBloccate);
        }

        /// <summary>
        ///     Elimina un oggetto del tipo 'RegistrazioniEmailBloccate'
        /// </summary>
        public static bool Delete(this BloccoEmail RegistrazioniEmailBloccate, out string avviso)
        {
            if (RegistrazioniEmailBloccate == null)
            {
                avviso = "L'entità 'RegistrazioniEmailBloccate' è null";
                return false;
            }

            return BloccoEmail.Delete(out avviso, RegistrazioniEmailBloccate);
        }

        /// <summary>
        ///     Elimina un oggetto del tipo 'RegistrazioniEmailBloccate'
        /// </summary>
        public static bool Delete(this BloccoEmail RegistrazioniEmailBloccate)
        {
            return BloccoEmail.Delete(RegistrazioniEmailBloccate);
        }
    }

    #endregion
}