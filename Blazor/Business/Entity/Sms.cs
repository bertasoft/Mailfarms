using System;
using CommonNetCore.Entity;
using CommonNetCore.Entity.Attribute;
using CommonNetCore.Entity.Validation.Attribute;

namespace Business.Entity
{
    [DatabaseTable]
    public class Sms : EntityBase<Sms>
    {
        #region Enum

        public enum SmsStatoEnum
        {
            Coda = 0,
            Inviato = 1,
        }

        #endregion

        #region Constructors

        public Sms()
        {
            UniqueIdentifier = string.Empty;
            Numero = string.Empty;
            Testo = string.Empty;
            Destinatario = string.Empty;
            Mittente = string.Empty;
            Sistema = string.Empty;
            Errore = string.Empty;
        }

        #endregion

        #region Auto Properties

        [StringNotNullOrEmpty(ErrorMessage = "Il campo 'UniqueIdentifier' non può rimanere vuoto")]
        [EntityColumnUnique(ErrorMessage = "Il campo 'UniqueIdentifier' contiene un valore già salvato")]
        [DatabaseColumn]
        public string UniqueIdentifier { get; set; }

        [StringNumeroCellulare(ErrorMessage = "Il campo 'Numero' deve contenere un numero di celluare valido")]
        [StringNotNullOrEmpty(ErrorMessage = "Il campo 'Numero' non può rimanere vuoto")]
        [DatabaseColumn]
        public string Numero { get; set; }

        [StringNotNullOrEmpty(ErrorMessage = "Il campo 'Testo' non può rimanere vuoto")]
        [DatabaseColumn]
        public string Testo { get; set; }

        /// <summary>
        /// Il mittente che appare negli sms, usato in OVH / SKEBBY come utente certificato (es.: DOWEB)
        /// </summary>
        [StringMaxLength(11, ErrorMessage = "Il campo 'MittenteSms' non deve superare 11 caratteri")]
        [StringNotNullOrEmpty(ErrorMessage = "Il campo 'MittenteSms' non deve rimanere vuoto")]
        [DatabaseColumn]
        public string MittenteSms { get; set; }

        /// <summary>
        /// Chi ha inviato l'sms, il software, l'app, il nome dello script, nome/cognome, ecc info per capire da dove è partito
        /// </summary>
        [StringMaxLength(50, ErrorMessage = "Il campo 'Mittente' non deve superare i 50 caratteri")]
        [StringNotNullOrEmpty(ErrorMessage = "Il campo 'Mittente' non deve rimanere vuoto")]
        [DatabaseColumn]
        public string Mittente { get; set; }

        /// <summary>
        /// L'identificativo di chi ha ricevuto l'sms, nome, cognome, nome azienda ...
        /// </summary>
        [StringMaxLength(50, ErrorMessage = "Il campo 'Destinatario' non deve superare i 50 caratteri")]
        [StringNotNullOrEmpty(ErrorMessage = "Il campo 'Destinatario' non deve rimanere vuoto")]
        [DatabaseColumn]
        public string Destinatario { get; set; }

        [StringMaxLength(50, ErrorMessage = "Il campo 'Sistema' non deve superare i 50 caratteri")]
        [StringNotNullOrEmpty(ErrorMessage = "Il campo 'Sistema' non deve rimanere vuoto")]
        [DatabaseColumn]
        public string Sistema { get; set; }

        [ObjectNull(ErrorMessage = "Il campo 'Errore' non deve rimanere null")]
        [DatabaseColumn]
        public string Errore { get; set; }

        [DatabaseColumn]
        public DateTime DataCoda { get; set; }

        [DatabaseColumn]
        public DateTime DataInvio { get; set; }

        /// <summary>
        /// Coda / Inviato
        /// </summary>
        [DatabaseColumn]
        public long Stato { get; set; }

        /// <summary>
        /// Quanto è lungo l'sms
        /// </summary>
        [LongMinMax(1, 1000, ErrorMessage = "L'sms deve essere compreso tra 1 e 1000 caratteri")]
        [DatabaseColumn]
        public long Caratteri { get; set; }

        /// <summary>
        /// Quanti messaggi sono stati utilizzati per inviare l'sms
        /// </summary>
        [DatabaseColumn]
        public long NumeroMessaggi { get; set; }

        #endregion

        #region Auto Methods

        /// <summary>
        ///     Ritorna un oggetto del tipo 'Sms' dal campo 'Guid'
        /// </summary>
        public static Sms GetItem(string uniqueIdentifier)
        {
            return GetItem("UniqueIdentifier", uniqueIdentifier);
        }

        #endregion

        #region Properties


        public SmsStatoEnum Statoenum
        {
            get => (SmsStatoEnum)Stato;
            set => Stato = (long)value;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Elimina un oggetto del tipo 'Sms'
        /// </summary>
        public static bool Delete(Sms Sms)
        {
            return Delete(out _, Sms);
        }

        /// <summary>
        ///     Elimina un oggetto del tipo 'Sms'
        /// </summary>
        public new static bool Delete(out string avviso, Sms Sms)
        {
            return EntityBase<Sms>.Delete(out avviso, Sms);
        }

        /// <summary>
        ///     Salva o aggiorna un oggetto del tipo 'Sms'
        /// </summary>
        public static bool Save(out string avviso, ref Sms Sms)
        {
            return EntityBase<Sms>.Save(out avviso, ref Sms);
        }

        #endregion
    }
}
