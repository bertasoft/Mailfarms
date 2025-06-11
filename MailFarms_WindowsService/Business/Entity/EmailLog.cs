#region Using

using System;
using System.Threading.Tasks;
using Business.Collection;
using CommonNetCore.Entity;
using CommonNetCore.Entity.Attribute;
using CommonNetCore.Entity.Validation.Attribute;

#endregion

namespace Business.Entity
{
    [DatabaseTable]
    public class EmailLog : EntityBase<EmailLog>
    {
        #region Enum

        #endregion

        #region Constructors

        public EmailLog()
        {
            Testo = string.Empty;
        }

        #endregion

        #region Auto Properties

        [DatabaseColumn]
        [ExternalReferences(typeof(EmailCollection), "Email")]
        public long IdEmail { get; set; }

        [ObjectNull(ErrorMessage = "Il campo 'Email' non può rimanere vuoto")]
        public Email Email
        {
            get => Email.GetItem(IdEmail);
            set => IdEmail = value?.Id ?? 0;
        }

        [DatabaseColumn]
        public DateTime Data { get; set; }

        [StringNotNullOrEmpty(ErrorMessage = "Il campo 'Testo' non può rimanere vuoto")]
        [DatabaseColumn]
        public string Testo { get; set; }
        
        [StringNotNullOrEmpty(ErrorMessage = "Il campo 'UniqueIdentifier' non può rimanere vuoto")]
        [DatabaseColumn]
        public string UniqueIdentifier { get; set; }

        #endregion

        #region Auto Methods

        #endregion

        #region Properties

        #endregion

        #region Methods

        /// <summary>
        ///     Elimina un oggetto del tipo 'Log'
        /// </summary>
        public static bool Delete(EmailLog log)
        {
            return Delete(out _, log);
        }

        /// <summary>
        ///     Elimina un oggetto del tipo 'Log'
        /// </summary>
        public new static bool Delete(out string avviso, EmailLog log)
        {
            return EntityBase<EmailLog>.Delete(out avviso, log);
        }

        /// <summary>
        ///     Salva o aggiorna un oggetto del tipo 'Log'
        /// </summary>
        public static bool Save(out string avviso, ref EmailLog log)
        {
            if (log != null)
                log.Data = DateTime.Now;

            return EntityBase<EmailLog>.Save(out avviso, ref log);
        }

        public static bool Svuota()
        {
            Task.Run(() =>
            {
                foreach (var log in EmailLogCollection.GetList())
                    log.Delete();
            });

            return true;
        }

        #endregion
    }

    #region Extension Methods

    public static class LogExtension
    {
        /// <summary>
        ///     Salva o aggiorna un oggetto del tipo 'Log'
        /// </summary>
        public static bool Save(this EmailLog log)
        {
            if (log == null)
                return false;

            string avviso;
            return EmailLog.Save(out avviso, ref log);
        }

        /// <summary>
        ///     Salva o aggiorna un oggetto del tipo 'Log'
        /// </summary>
        public static bool Save(this EmailLog log, out string avviso)
        {
            if (log == null)
            {
                avviso = "L'entità 'Log' è null";
                return false;
            }

            return EmailLog.Save(out avviso, ref log);
        }

        /// <summary>
        ///     Elimina un oggetto del tipo 'Log'
        /// </summary>
        public static bool Delete(this EmailLog log, out string avviso)
        {
            if (log == null)
            {
                avviso = "L'entità 'Log' è null";
                return false;
            }

            return EmailLog.Delete(out avviso, log);
        }

        /// <summary>
        ///     Elimina un oggetto del tipo 'Log'
        /// </summary>
        public static bool Delete(this EmailLog log)
        {
            return EmailLog.Delete(log);
        }
    }

    #endregion
}