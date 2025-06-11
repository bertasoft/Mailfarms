#region

#region Creazione e modifica

// Data di creazione 17:27 24/11/2014
// Ultima modifica 19:40 27/11/2014

#endregion

#region

#region Creazione e modifica

// Data di creazione 17:26 26/08/2014
// Ultima modifica 10:38 30/09/2014

#endregion

#region

#region Creazione e modifica

// Data di creazione 22:39 21/08/2014
// Ultima modifica 23:10 21/08/2014

#endregion

using System;
using Business.Code;
using CommonNetCore.Entity;

#region

using Business.Entity;

#endregion

#endregion

#endregion

#endregion

namespace Business.Collection
{
    public class BloccoAccessoCollection : EntityCollectionBase<BloccoAccesso, BloccoAccessoCollection>
    {
        public static long GetCount(string filtroNomeEmail = null)
        {
            if (string.IsNullOrEmpty(filtroNomeEmail))
                return EntityCollectionBase<BloccoAccesso, BloccoAccessoCollection>.GetCount();

            return GetCount("Email.StartWith(@0)", new object[] {filtroNomeEmail});
        }

        public static BloccoAccessoCollection GetList(string filtroNomeEmail = null, int item4page = -1, long page = -1, string orderPredicate = null)
        {
            if (string.IsNullOrEmpty(filtroNomeEmail))
                return GetList(item4page, page, orderPredicate: orderPredicate);

            return GetList(item4page, page, "Email.StartWith(@0)", new object[]
            {
                filtroNomeEmail
            }, orderPredicate);
        }

        public static BloccoAccessoCollection GetListSbloccabili()
        {
            var dataPrecisione5Minuti = DateTime.Today.AddHours(DateTime.Now.Hour);

            var res = DateTime.Now.Minute / 5; //Divisione intera
            var minutiDaAggiungere = res * 5;

            //Prendo la data aggiornata ogni 5 minuti, per evitare di fare query continue, in questo modo uso la cache

            dataPrecisione5Minuti = dataPrecisione5Minuti.AddMinutes(minutiDaAggiungere);
            return GetList(wherePredicate: "DataTentativo < " + dataPrecisione5Minuti.AddMinutes(-30));
        }
    }
}