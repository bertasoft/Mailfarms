#region

#region Creazione e modifica

// Data di creazione 17:27 24/11/2014
// Ultima modifica 19:40 27/11/2014

#endregion

#region

#region Creazione e modifica

// Data di creazione 17:23 26/08/2014
// Ultima modifica 10:38 30/09/2014

#endregion

#region

#region Creazione e modifica

// Data di creazione 22:39 21/08/2014
// Ultima modifica 23:10 21/08/2014

#endregion

using Business.Code;
using Business.Collection;
using CommonNetCore.Entity;
using CommonNetCore.Entity.Attribute;
using CommonNetCore.Entity.Validation.Attribute;
using CommonNetCore.Misc;
using ManagerEmail = Business.Code.ManagerEmail;

#region

using System;

#endregion

#endregion

#endregion

#endregion

namespace Business.Entity
{
    [DatabaseTable]
    public class BloccoAccesso : EntityBase<BloccoAccesso>
    {
        #region Enums

        #endregion

        #region Constructors

        public BloccoAccesso()
        {
            Email = string.Empty;
        }

        #endregion

        #region Fields

        [EntityColumnUnique(false, ErrorMessage = "Il valore specificato per il campo email è già presente")]
        [StringNotNullOrEmpty(ErrorMessage = "Inserire un indirizzo email valido")]
        [StringEmail(ErrorMessage = "L'indirizzo email deve essere valido")]
        [DatabaseColumn]
        public string Email { get; set; }

        [DatabaseColumn]
        public int NumTentativo { get; set; }

        [DatabaseColumn]
        public DateTime DataTentativo { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Prende un oggetto del tipo Utenti da Email
        /// </summary>
        /// <returns>Ritorna l'oggetto Utenti</returns>
        public static BloccoAccesso GetItem(string email)
        {
            return GetItem("Email", email);
        }

        /// <summary>
        ///     Salva o aggiorna un oggetto del tipo EmailBloccate da una colonna di chiave univoca
        /// </summary>
        /// <param name="avviso">Ritorna eventuali avvisi di errore</param>
        /// <param name="emailbloccate">L'oggetto da eliminare</param>
        /// <returns>Ritorna true se l'operazione ha avuto successo, altrimenti false</returns>
        public static bool Save(out string avviso, ref BloccoAccesso emailbloccate)
        {
            return EntityBase<BloccoAccesso>.Save(out avviso, ref emailbloccate);
        }

        /// <summary>
        ///     Elimina un oggetto del tipo EmailBloccate
        /// </summary>
        /// <param name="avviso">Ritorna eventuali avvisi di errore</param>
        /// <param name="emailbloccate">L'oggetto da eliminare</param>
        /// <returns>Ritorna true se l'operazione ha avuto successo, altrimenti false</returns>
        public new static bool Delete(out string avviso, BloccoAccesso emailbloccate)
        {
            return EntityBase<BloccoAccesso>.Delete(out avviso, emailbloccate);
        }

        /// <summary>
        ///     Dopo un accesso corretto, rimuovo l'email se è stata bloccata
        /// </summary>
        internal static void SbloccaEmail(string email)
        {
            var emailBloccate = GetItem(email);

            if (emailBloccate == null)
                return;

            Delete(out _, emailBloccate);
        }

        /// <summary>
        ///     Aggiunge o incrementa i tentativi di accesso fallito
        /// </summary>
        public static void BloccaEmail(string email)
        {
            var emailBloccate = GetItem(email) ?? new BloccoAccesso
            {
                Email = email,
                NumTentativo = 0
            };

            emailBloccate.DataTentativo = DateTime.Now;
            emailBloccate.NumTentativo++;
            Save(emailBloccate);
        }

        /// <summary>
        ///     Ritorna true se l'accesso è bloccato, il blocco rimane attivo per 30 minuti
        ///     L'utente può fare massimo 20 tentativi
        /// </summary>
        public static bool AccessoBloccato(string email)
        {
            var emailBloccate = GetItem(email);

            if (emailBloccate == null)
                return false;

            //Invio l'email solo una volta
            if (emailBloccate.NumTentativo > 20 && emailBloccate.DataTentativo > DateTime.Now.AddMinutes(-30))
            {
                ManagerEmail.SbloccaAccesso(Utenti.GetItem(email));

                return true;
            }

            return false;
        }

        /// <summary>
        ///     Elimina le email bloccate
        /// </summary>
        public static void ResetSbloccabili()
        {
            foreach (var sbloccabile in BloccoAccessoCollection.GetListSbloccabili())
                Delete(out _, sbloccabile);
        }

        #endregion
    }
}