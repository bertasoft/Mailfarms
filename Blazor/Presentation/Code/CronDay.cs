using System;
using Business.Collection;
using Business.Entity;
using CommonNetCore.GlobalExtension;
using CommonNetCore.Misc;

namespace MailFarmsBlazor.Code
{
    public class CronDay : CronScheduler.CronBase
    {
        /// <summary>
        ///     Eseguito una volta al giorno
        /// </summary>
        public CronDay() : base("Day", CronScheduler.TypeOfExecutionEnum.Day, 1)
        {

        }

        public override void CronJob()
        {
            foreach (var email in EmailCollection.GetList(wherePredicate: "Stato != 0 AND DataUltimoTentativo < " + DateTime.Now.AddDays(-Impostazioni.GetValore(Impostazioni.ImpostazioniEnum.ScadenzaGiorni).ToInt())))
                email.Delete();
        }
    }
}
