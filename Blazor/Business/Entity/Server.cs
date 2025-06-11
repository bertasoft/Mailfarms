#region Using

using Business.Code;
using Business.Collection;
using CommonNetCore.Entity;
using CommonNetCore.Entity.Attribute;
using CommonNetCore.Entity.DataTransformation;
using CommonNetCore.Entity.Validation.Attribute;
using System;
using System.Threading.Tasks;

#endregion

namespace Business.Entity
{
    [DatabaseTable]
    [ExternalDependency(typeof(ServerStatisticheCollection), "IdServer", true)]
    [ExternalDependency(typeof(ServerDominiBannatiCollection), "IdServer", true)]
    public class Server : EntityBase<Server>
    {
        #region Enum

        #endregion

        #region Constructors

        public Server()
        {
            Ip = string.Empty;
        }

        #endregion

        #region Auto Properties

        [Lower]
        [StringIp(ErrorMessage = "Il campo deve essere un indirizzo Ip valido")]
        [EntityColumnUnique(ErrorMessage = "Il campo inserito per 'Ip' è già salvato")]
        [StringNotNullOrEmpty(ErrorMessage = "Il campo 'Ip' non può rimanere vuoto")]
        [DatabaseColumn]
        public string Ip { get; set; }
        
        [DatabaseColumn]
        public string Helo { get; set; }

        /// <summary>
        /// Consuma la sua coda, email in attesa di essere inviate
        /// </summary>
        [DatabaseColumn]
        public bool Attivo { get; set; }

        /// <summary>
        /// Accetta nuove email da mettere in coda
        /// </summary>
        [DatabaseColumn]
        public bool Riceve { get; set; }

        [DatabaseColumn]
        public long Inviate { get; set; }

        [DatabaseColumn]
        public long Errate { get; set; }

        #endregion

        #region Auto Methods

        public static Server GetItem(string indirizzoIp)
        {
            return Server.GetItem("Ip", indirizzoIp);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Ritorna il numero degli oggetti di tipo "ServerStatistiche"
        /// </summary>
        public long ServerStatisticheCount
        {
            get { return ServerStatisticheCollection.GetCount(wherePredicate: "IdServer = " + Id); }
        }

        /// <summary>
        /// Ritorna l'elenco degli oggetti di tipo "ServerStatistiche"
        /// </summary>
        public ServerStatisticheCollection ServerStatisticheCollection
        {
            get { return ServerStatisticheCollection.GetList(wherePredicate: "IdServer = " + Id); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Elimina un oggetto del tipo 'Server'
        /// </summary>
        public static bool Delete(Server server)
        {
            return Delete(out _, server);
        }

        /// <summary>
        /// Elimina un oggetto del tipo 'Server'
        /// </summary>
        public new static bool Delete(out string avviso, Server server)
        {
            return EntityBase<Server>.Delete(out avviso, server);
        }

        internal static void AggiornaImpostazioni(Server server)
        {
            if (server == null)
                return;

            Task.Run(async () =>
            {
                foreach (var p in Enum.GetNames(typeof(ServerImpostazioni.ServerImpostazioniEnum)))
                {
                    var enumValue = (ServerImpostazioni.ServerImpostazioniEnum)Enum.Parse(typeof(ServerImpostazioni.ServerImpostazioniEnum), p);

                    var impostazione = ServerImpostazioni.GetItem(enumValue);

                    await MailFarms_SharedService.Code.RequestWindowsService.AggiornaConfigurazione(server.Ip, p, impostazione.Valore);
                };
            });
        }

        /// <summary>
        /// Salva o aggiorna un oggetto del tipo 'Server'
        /// </summary>
        public static bool Save(out string avviso, ref Server server)
        {
            var corrente = GetItem(server.Id);

            if (!EntityBase<Server>.Save(out avviso, ref server))
                return false;

            if (corrente == null)
                AggiornaImpostazioni(server);

            //aggiorna lo stato di attivo che ha effetto immediato sulla coda
            MailFarms_SharedService.Code.RequestWindowsService.Attivo(server.Ip, server.Attivo).GetAwaiter().GetResult();

            //aggiorna l'indirizzo ip
            if (corrente == null || corrente.Ip != server.Ip)
            {
                var result = MailFarms_SharedService.Code.RequestWindowsService.AggiornaIndirizzoIp(server.Ip).GetAwaiter().GetResult();

                if (!result.Result)
                {
                    //se non era presente lo elimino
                    if (corrente == null)
                        server.Delete();
                    else
                    {
                        //altrimenti ripristino l'ip precedente
                        server.Ip = corrente.Ip;
                        EntityBase<Server>.Save(server);
                    }

                    avviso = result.Avviso;
                    return false;
                }
            }

            return true;
        }

        #endregion
    }

    #region Extension Methods

    public static class ServerExtension
    {
        /// <summary>
        /// Salva o aggiorna un oggetto del tipo 'Server'
        /// </summary>
        public static bool Save(this Server server)
        {
            if (server == null)
                return false;

            return Server.Save(out _, ref server);
        }

        /// <summary>
        /// Salva o aggiorna un oggetto del tipo 'Server'
        /// </summary>
        public static bool Save(this Server server, out string avviso)
        {
            if (server == null)
            {
                avviso = "L'entità 'Server' è null";
                return false;
            }

            return Server.Save(out avviso, ref server);
        }

        /// <summary>
        /// Elimina un oggetto del tipo 'Server'
        /// </summary>
        public static bool Delete(this Server server, out string avviso)
        {
            if (server == null)
            {
                avviso = "L'entità 'Server' è null";
                return false;
            }

            return Server.Delete(out avviso, server);
        }

        /// <summary>
        /// Elimina un oggetto del tipo 'Server'
        /// </summary>
        public static bool Delete(this Server server)
        {
            return Server.Delete(server);
        }
    }

    #endregion
}