using System;
using Business.Entity;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CommonNetCore;
using CommonNetCore.Misc;
using Microsoft.Extensions.Primitives;

namespace SmtpRelayer
{
    public static partial class Relayer
    {
        internal static readonly object _lockBag = new();

        private static readonly Timer _timer = new(delegate
        {
            lock (_lockBag)
            {
                foreach (var run in Program.DominiRunner)
                {
                    if (run.Value.Running)
                        continue;

                    run.Value.CancellationTokenSource.Cancel();

                    Program.DominiRunner.Remove(run.Key);

                    Program.Trace("RimossoRunner, " + run.Key);
                }
            }
        }, null, 1000, 1000);

        public static void AddEmail(Email email)
        {
            var dominio = Email.GetDomain(email.DestinatarioEmail);

            lock (_lockBag)
            {
                if (!Program.DominiRunner.TryGetValue(dominio, out Runner runner))
                {
                    Program.Trace("NuovoRunner, " + dominio);
                    runner = new Runner(dominio);
                }

                runner.NuovaEmail(email);

                Program.DominiRunner[dominio] = runner;
            }
        }
    }
}
