using System;

namespace SmtpRelayer
{
    /// <summary>
    /// Quando arriva una nuova email, scatta questo evento che viene intercettato e lancia immediatamente il processo di invio
    /// </summary>
    public class AlertInjection
    {
        public Action OnNewEmailCallback { get; set; }
    }
}
