using System;
using Business.Code;
using Business.Entity;
using CommonNetCore.Entity;

namespace Business.Collection
{
    public class BloccoIpCollection : EntityCollectionBase<BloccoIp, BloccoIpCollection>
    {
        public static long GetCount(string filtroNomeEmail = null)
        {
            if (string.IsNullOrEmpty(filtroNomeEmail))
                return EntityCollectionBase<BloccoIp, BloccoIpCollection>.GetCount();

            return GetCount("Email.StartWith(@0)", new object[] { filtroNomeEmail });
        }

        public static BloccoIpCollection GetList(string ip = null, int item4Page = -1, long page = -1, string orderPredicate = null)
        {
            if (string.IsNullOrEmpty(ip))
                return GetList(item4Page, page, orderPredicate: orderPredicate);

            return GetList(item4Page, page, "Ip.StartWith(@0)", new object[]
            {
                ip
            }, orderPredicate);
        }

        public static long GetCountByIp(string ip)
        {
            if (string.IsNullOrEmpty(ip))
                return GetCount();

            return GetCount("Ip.Contains(@)", new object[] { ip });
        }

        public static BloccoIpCollection GetList(string ip, long page, int item4Page)
        {
            if (string.IsNullOrEmpty(ip))
                return GetList(item4Page, page, orderPredicate: "Ip ASC");

            return GetList(item4Page, page, "Ip.Contains(@)", new object[]
            {
                ip
            }, "Ip ASC");
        }

        /// <summary>
        ///     Restituisco gli ip che sono stati bloccati e sono sbloccabili perchè hanno superato mezz'ora senza nuovi tentativi
        /// </summary>
        internal static BloccoIpCollection GetListSbloccabili()
        {
            var dataPrecisione5Minuti = DateTime.Today.AddHours(DateTime.Now.Hour);

            var res = DateTime.Now.Minute / 5; //Divisione intera
            var minutiDaAggiungere = res * 5;

            //Prendo la data aggiornata ogni 5 minuti, per evitare di fare query continue, in questo modo uso la cache

            dataPrecisione5Minuti = dataPrecisione5Minuti.AddMinutes(minutiDaAggiungere);
            return GetList(wherePredicate: "Data < " + dataPrecisione5Minuti.AddMinutes(-30));
        }
    }
}