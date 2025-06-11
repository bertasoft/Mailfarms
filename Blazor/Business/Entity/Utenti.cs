#region Using

using System;
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
    [ExternalDependency(typeof(UtentiAccessiCollection), "IdUtenti", true)]
    public class Utenti : EntityBase<Utenti>
	{
		#region Enum

		#endregion

		#region Constructors

		public Utenti()
		{
			Nome = string.Empty;
			Cognome = string.Empty;
			Email = string.Empty;
			Password = string.Empty;
		}

		#endregion

		#region Auto Properties

		[TrimCase]
        [StringNotNullOrEmpty(ErrorMessage = "Indicare il nome")]
		[DatabaseColumn]
		public string Nome { get; set; }

		[TrimCase]
        [StringNotNullOrEmpty(ErrorMessage = "Indicare il cognome")]
		[DatabaseColumn]
		public string Cognome { get; set; }

        [Lower]
        [StringEmail(ErrorMessage = "La mail non è corretta")]
        [StringNotNullOrEmpty(ErrorMessage = "La mail non è presente")]
        [EntityColumnUnique(false, ErrorMessage = "La mail indicata è già utilizzata")]
        [DatabaseColumn]
		public string Email { get; set; }

        [StringNotNullOrEmpty(ErrorMessage = "Indicare la password")]
        [StringMinLength(8, ErrorMessage = "La password deve essere di almeno 8 caratteri")]
        [StringMaxLength(30, ErrorMessage = "La password deve essere di massimo 30 caratteri")]
        [DatabaseColumn]
		public string Password { get; set; }

        [DatabaseColumn]
        public bool Attivo { get; set; }

        [DatabaseColumn]
        public bool Admin { get; set; }

		#endregion

		#region Auto Methods


		#endregion

		#region Properties

		public string NomeCognome => Nome + " " + Cognome;

        public string CognomeNomeEmail => Cognome + ", " + Nome + " (" + Email + ")";


		///// <summary>
		/////     Ritorna l'operatore correntemente loggato
		///// </summary>
		//public static Utenti Utente
		//{
		//    get { return GetItem(CookiesHelper.GetCookie("UtentiEmail")); }
		//    set
		//    {
		//        if (value == null)
		//        {
		//            CookiesHelper.ClearCookie("UtentiEmail");
		//            return;
		//        }

		//        CookiesHelper.SetCookie("UtentiEmail", value.Email);
		//    }
		//}

		#endregion

		#region Methods

		/// <summary>
		///     Prende un oggetto del tipo Utenti da Email
		/// </summary>
		/// <returns>Ritorna l'oggetto Utenti</returns>
        public static Utenti GetItem(string email)
        {
            return GetItem("Email", email);
        }

		/// <summary>
		/// Elimina un oggetto del tipo 'Utenti'
		/// </summary>
		public static bool Delete(Utenti utenti)
		{
            return Delete(out _, utenti);
		}

		/// <summary>
		/// Elimina un oggetto del tipo 'Utenti'
		/// </summary>
		public new static bool Delete(out string avviso, Utenti utenti)
		{
			return EntityBase<Utenti>.Delete(out avviso, utenti);
		}

		/// <summary>
		/// Salva o aggiorna un oggetto del tipo 'Utenti'
		/// </summary>
		public static bool Save(out string avviso, ref Utenti utenti)
		{
			return EntityBase<Utenti>.Save(out avviso, ref utenti);
		}

        public static bool Accedi(out string avviso, string email, string password, string ipAddress)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                avviso = "Le credenziali indicate non sono valide";
                return false;
            }

            if (BloccoAccesso.AccessoBloccato(email))
            {
                avviso = "L'account è stato temporaneamente bloccato";
                return false;
            }

            if (BloccoIp.IpIsLocked(ipAddress))
            {
                avviso = "L'account è stato temporaneamente bloccato";
                return false;
            }

            var utenti = GetItem(email);

            if (utenti == null)
            {
                avviso = "Le credenziali indicate non sono valide";
                return false;
            }

            if (utenti.Password != password)
            {
                avviso = "Le credenziali indicate non sono valide";
                return false;
            }

            if (!utenti.Attivo)
            {
                avviso = "Account non attivo";
                return false;
            }

            //Cancella eventuali tentativi falliti di login che non hanno raggiunto il limite massimo
            BloccoAccesso.SbloccaEmail(email);

            //Come sopra, ma sull'ip
            BloccoIp.SbloccaIp(ipAddress);

            //Inserisco nel log l'accesso
            var accesso = new UtentiAccessi
            {
                Utenti = utenti,
                IpAccesso = ipAddress,
                DataAccesso = DateTime.Now
            };

            return accesso.Save(out avviso);
        }

		#endregion
	}

	#region Extension Methods

	public static class UtentiExtension
	{
		/// <summary>
		/// Salva o aggiorna un oggetto del tipo 'Utenti'
		/// </summary>
		public static bool Save(this Utenti utenti)
		{
			if (utenti == null)
				return false;

            return Utenti.Save(out _, ref utenti);
		}

		/// <summary>
		/// Salva o aggiorna un oggetto del tipo 'Utenti'
		/// </summary>
		public static bool Save(this Utenti utenti, out string avviso)
		{
			if (utenti == null)
			{
				avviso = "L'entità 'Utenti' è null";
				return false;
			}

			return Utenti.Save(out avviso, ref utenti);
		}

		/// <summary>
		/// Elimina un oggetto del tipo 'Utenti'
		/// </summary>
		public static bool Delete(this Utenti utenti, out string avviso)
		{
			if (utenti == null)
			{
				avviso = "L'entità 'Utenti' è null";
				return false;
			}

			return Utenti.Delete(out avviso, utenti);
		}

		/// <summary>
		/// Elimina un oggetto del tipo 'Utenti'
		/// </summary>
		public static bool Delete(this Utenti utenti)
		{
			return Utenti.Delete(utenti);
		}
	}

	#endregion
}