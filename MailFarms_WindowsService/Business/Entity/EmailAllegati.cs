#region Using

using System;
using System.IO;
using System.Threading.Tasks;
using Business.Collection;
using CommonNetCore;
using CommonNetCore.Entity;
using CommonNetCore.Entity.Attribute;
using CommonNetCore.Entity.Validation.Attribute;
using CommonNetCore.GlobalExtension;
using CommonNetCore.Misc;

#endregion

namespace Business.Entity
{
    [DatabaseTable]
    public class EmailAllegati : EntityBase<EmailAllegati>
    {
        #region Enum

        #endregion

        #region Constructors

        public EmailAllegati()
        {
            NomeFile = string.Empty;
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

        [StringNotNullOrEmpty(ErrorMessage = "Il campo 'NomeFile' non può rimanere vuoto")]
        [DatabaseColumn]
        public string NomeFile { get; set; }

        [DatabaseColumn]
        public long Dimensione { get; set; }

        #endregion

        #region Auto Methods

        #endregion

        #region Properties

        public string PercorsoDisco => Settings.RepositoryDisk("Allegati") + Id;

        public async Task<byte[]> FileBytesAsync()
        {
            if (!File.Exists(PercorsoDisco))
                return null;

            return await File.ReadAllBytes(PercorsoDisco).DecompressAsync();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Elimina un oggetto del tipo 'EmailAllegati'
        /// </summary>
        public static bool Delete(EmailAllegati emailAllegati)
        {
            return Delete(out _, emailAllegati);
        }

        /// <summary>
        ///     Elimina un oggetto del tipo 'EmailAllegati'
        /// </summary>
        public new static bool Delete(out string avviso, EmailAllegati emailAllegati)
        {
            if (!EntityBase<EmailAllegati>.Delete(out avviso, emailAllegati))
                return false;

            Task.Run(() => { File.Delete(emailAllegati.PercorsoDisco); });

            return true;
        }

        /// <summary>
        ///     Salva o aggiorna un oggetto del tipo 'EmailAllegati'
        /// </summary>
        public static bool Save(out string avviso, ref EmailAllegati emailAllegati)
        {
            return EntityBase<EmailAllegati>.Save(out avviso, ref emailAllegati);
        }

        public static async Task<string> CaricaAllegato(Email email, string nomeFile, byte[] fileByte)
        {
            if (email == null || email.Id == 0)
                return "null";

            if (fileByte == null || fileByte.Length == 0)
                return "Il file non deve essere vuoto";

            var emailAllegati = new EmailAllegati
            {
                Email = email,
                NomeFile = nomeFile,
                Dimensione = fileByte.Length
            };

            if (!emailAllegati.Save(out string avviso))
                return avviso;

            try
            {
                await (await fileByte.CompressAsync()).SaveToDiskAsync(emailAllegati.PercorsoDisco);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

            return string.Empty;
        }

        #endregion
    }

    #region Extension Methods

    public static class EmailAllegatiExtension
    {
        /// <summary>
        ///     Salva o aggiorna un oggetto del tipo 'EmailAllegati'
        /// </summary>
        public static bool Save(this EmailAllegati emailAllegati)
        {
            if (emailAllegati == null)
                return false;

            return EmailAllegati.Save(out _, ref emailAllegati);
        }

        /// <summary>
        ///     Salva o aggiorna un oggetto del tipo 'EmailAllegati'
        /// </summary>
        public static bool Save(this EmailAllegati emailAllegati, out string avviso)
        {
            if (emailAllegati == null)
            {
                avviso = "L'entità 'EmailAllegati' è null";
                return false;
            }

            return EmailAllegati.Save(out avviso, ref emailAllegati);
        }

        /// <summary>
        ///     Elimina un oggetto del tipo 'EmailAllegati'
        /// </summary>
        public static bool Delete(this EmailAllegati emailAllegati, out string avviso)
        {
            if (emailAllegati == null)
            {
                avviso = "L'entità 'EmailAllegati' è null";
                return false;
            }

            return EmailAllegati.Delete(out avviso, emailAllegati);
        }

        /// <summary>
        ///     Elimina un oggetto del tipo 'EmailAllegati'
        /// </summary>
        public static bool Delete(this EmailAllegati emailAllegati)
        {
            return EmailAllegati.Delete(emailAllegati);
        }
    }

    #endregion
}