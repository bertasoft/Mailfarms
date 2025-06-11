#region Using

using System;
using Business.Collection;
using CommonNetCore.Entity;
using CommonNetCore.Entity.Attribute;
using CommonNetCore.Entity.DataTransformation;
using CommonNetCore.Entity.Validation.Attribute;
using MailFarms_SharedService.Entity;

#endregion

namespace Business.Entity
{
    [DatabaseTable]
    [ExternalDependency(typeof(EmailLogCollection), "IdEmail", true)]
    [ExternalDependency(typeof(EmailAllegatiCollection), "IdEmail", true)]
    public class Email : EntityBase<Email>
    {
        #region Enum

        #endregion

        #region Constructors

        public Email()
        {
            UniqueIdentifier = string.Empty;
            MittenteEmail = string.Empty;
            MittenteNome = string.Empty;
            DestinatarioEmail = string.Empty;
            DestinatarioNome = string.Empty;
            Oggetto = string.Empty;
            Contenuto = string.Empty;
            UrlEliminazione = string.Empty;
            StatusCode4xx5xx = string.Empty;
            RispondiA = string.Empty;
        }

        #endregion

        #region Auto Properties

        [StringNotNullOrEmpty(ErrorMessage = "Il campo 'UniqueIdentifier' non può rimanere vuoto")]
        [EntityColumnUnique(ErrorMessage = "Il campo 'Email.UniqueIdentifier' contiene un valore già salvato")]
        [DatabaseColumn]
        public string UniqueIdentifier { get; set; }

        [Lower]
        [StringEmail(ErrorMessage = "Il campo 'MittenteEmail' deve contenere una email valida")]
        [DatabaseColumn]
        public string MittenteEmail { get; set; }

        [StringNotNullOrEmpty(ErrorMessage = "Il campo 'MittenteNome' non deve rimanere vuoto")]
        [DatabaseColumn]
        public string MittenteNome { get; set; }

        [Lower]
        [StringNotNullOrEmpty(ErrorMessage = "Il campo 'DestinatarioEmail' non deve rimanere vuoto")]
        [StringEmail(ErrorMessage = "Il campo 'DestinatarioEmail' deve contenere una email valida")]
        [DatabaseColumn]
        public string DestinatarioEmail { get; set; }

        [StringNotNullOrEmpty(ErrorMessage = "Il campo 'DestinatarioNome' non deve rimanere vuoto")]
        [DatabaseColumn]
        public string DestinatarioNome { get; set; }

        /// <summary>
        ///     Utilizzato per il camp "Require-Recipient-Valid-Since"
        /// </summary>
        [DatabaseColumn]
        public DateTime DestinatarioDataRegistrazione { get; set; }

        [StringNotNullOrEmpty(ErrorMessage = "Il campo 'Oggetto' non deve rimanere vuoto")]
        [DatabaseColumn]
        public string Oggetto { get; set; }

        [StringNotNullOrEmpty(ErrorMessage = "Il campo 'Contenuto' non può rimanere vuoto")]
        [DatabaseColumn]
        public string Contenuto { get; set; }

        /// <summary>
        ///     Quando la mail è stata inserita in email
        /// </summary>
        [DatabaseColumn]
        public DateTime DataArrivo { get; set; }

        /// <summary>
        ///     L'ultimo tentativo di invio
        /// </summary>
        [DatabaseColumn]
        public DateTime DataUltimoTentativo { get; set; }

        /// <summary>
        ///     Il prossimo tentativo di invio dell'email
        /// </summary>
        [DatabaseColumn]
        public DateTime DataProssimoTentativo { get; set; }

        /// <summary>
        ///     Quante volte ho provato ad inviare la mail
        /// </summary>
        [DatabaseColumn]
        public long NumeroTentativi { get; set; }

        /// <summary>
        /// Tramite questa chiamata get la mail viene eliminata dalla mailing list
        /// </summary>
        [DatabaseColumn]
        public string UrlEliminazione { get; set; }

        [DatabaseColumn]
        [ObjectNull(ErrorMessage = "Il campo 'StatusCode4xx5xx' non deve rimanere null")]
        public string StatusCode4xx5xx { get; set; }

        [DatabaseColumn]
        public long Stato { get; set; }

        [DatabaseColumn]
        public bool Immediata { get; set; }

        [DatabaseColumn]
        public string RispondiA { get; set; }

        #endregion

        #region Auto Methods

        /// <summary>
        ///     Ritorna un oggetto del tipo 'Email' dal campo 'Guid'
        /// </summary>
        public static Email GetItem(string uniqueIdentifier)
        {
            return GetItem("UniqueIdentifier", uniqueIdentifier);
        }

        #endregion

        #region Properties

        public string Mittente => MittenteNome + " (" + MittenteEmail + ")";

        public string Destinatario => DestinatarioNome + " (" + DestinatarioEmail + ")";

        public Stato.StatoEnum Statoenum
        {
            get => (Stato.StatoEnum)Stato;
            set
            {
                Stato = (long)value;
            }
        }

        public EmailAllegatiCollection Allegati => EmailAllegatiCollection.GetList(wherePredicate: "IdEmail == " + Id);

        public EmailLogCollection Log => EmailLogCollection.GetList(wherePredicate: "IdEmail == " + Id);

        #endregion

        #region Methods

        /// <summary>
        ///     Elimina un oggetto del tipo 'Email'
        /// </summary>
        public static bool Delete(Email email)
        {
            return Delete(out _, email);
        }

        /// <summary>
        ///     Elimina un oggetto del tipo 'Email'
        /// </summary>
        public new static bool Delete(out string avviso, Email email)
        {
            return EntityBase<Email>.Delete(out avviso, email);
        }

        /// <summary>
        ///     Salva o aggiorna un oggetto del tipo 'Email'
        /// </summary>
        public static bool Save(out string avviso, ref Email email)
        {
            return EntityBase<Email>.Save(out avviso, ref email);
        }

        public static string GetDomain(string email)
        {
            if (string.IsNullOrEmpty(email))
                return email;

            if (!email.Contains("@"))
                return string.Empty;

            return email.Substring(email.IndexOf("@", StringComparison.Ordinal) + 1);
        }

        #endregion
    }

    #region Extension Methods

    public static class EmailExtension
    {
        /// <summary>
        ///     Salva o aggiorna un oggetto del tipo 'Email'
        /// </summary>
        public static bool Save(this Email email)
        {
            if (email == null)
                return false;

            return Email.Save(out _, ref email);
        }

        /// <summary>
        ///     Salva o aggiorna un oggetto del tipo 'Email'
        /// </summary>
        public static bool Save(this Email email, out string avviso)
        {
            if (email == null)
            {
                avviso = "L'entità 'Email' è null";
                return false;
            }

            return Email.Save(out avviso, ref email);
        }

        /// <summary>
        ///     Elimina un oggetto del tipo 'Email'
        /// </summary>
        public static bool Delete(this Email email, out string avviso)
        {
            if (email == null)
            {
                avviso = "L'entità 'Email' è null";
                return false;
            }

            return Email.Delete(out avviso, email);
        }

        /// <summary>
        ///     Elimina un oggetto del tipo 'Email'
        /// </summary>
        public static bool Delete(this Email email)
        {
            return Email.Delete(email);
        }
    }

    #endregion
}