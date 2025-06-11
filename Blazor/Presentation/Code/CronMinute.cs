using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Business.Collection;
using Business.Entity;
using CommonNetCore.Misc;
using Microsoft.AspNetCore.Components;

namespace MailFarmsBlazor.Code
{
    public class CronMinute : CronScheduler.CronBase
    {
        public CronMinute()
            : base("CronMinute", CronScheduler.TypeOfExecutionEnum.Minutes, 1, useDb: false, logStartEnd: false)
        {

        }

        public override void CronJob()
        {
            BloccoAccesso.ResetSbloccabili();

            BloccoIp.ResetSbloccabili();
        }
    }
}
