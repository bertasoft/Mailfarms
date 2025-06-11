using System;
using System.Collections.Generic;
using System.Linq;
using Business.Entity;
using CommonNetCore.GlobalExtension;
using CommonNetCore.Misc;
using MailKit;

namespace SmtpRelayer.Smtp
{
    public class SmtpLogger : IProtocolLogger
    {
        private readonly string _uniqueIdentifier;

        private readonly List<string> _messageTrace = new List<string>();

        private string _lastStatusCode;
        private int _data;

        private DateTime _avvio;
        private DateTime _lastCommand;

        public IAuthenticationSecretDetector AuthenticationSecretDetector { get; set; }

        public SmtpLogger(string uniqueIdentifier)
        {
            _uniqueIdentifier = uniqueIdentifier;
        }

        public void Dispose()
        {
            var email = Email.GetItem(_uniqueIdentifier);

            //nel caso invii una email di anteprima
            if (email == null)
                return;

            if (!_messageTrace.Any())
                return;

            _messageTrace.Add("Tempo totale: " + (DateTime.Now - _avvio).DurationString());

            var emailLog = new EmailLog
            {
                UniqueIdentifier = Guid.NewGuid().ToString(),
                Email = email,
                Data = DateTime.Now,
                Testo = string.Join(Environment.NewLine, _messageTrace.ToArray())
            };

            if (!emailLog.Save(out var avviso))
                ManagerLog.Error(avviso);

            if (!string.IsNullOrEmpty(_lastStatusCode))
                email.StatusCode4xx5xx = _lastStatusCode;

            if (!email.Save(out avviso))
                ManagerLog.Error(avviso);

            _messageTrace.Clear();
        }

        public void LogConnect(Uri uri)
        {
            _avvio = DateTime.Now;
            _lastCommand = DateTime.Now;

            var message = "C: Mi collego a " + uri;

            Console.WriteLine(message);
            _messageTrace.Add(message);
            _messageTrace.Add(string.Empty);
        }

        public void LogClient(byte[] buffer, int offset, int count)
        {
            if (_data == 1)
            {
                var message = "(" + (DateTime.Now - _lastCommand).TotalMilliseconds.ToString("F0") + ") C: Invio i byte della mail...";

                _lastCommand = DateTime.Now;

                Console.WriteLine(message);
                _messageTrace.Add(message);
                _messageTrace.Add(string.Empty);

                _data++;
            }

            if (_data == 0)
            {
                var stringa = buffer.Skip(offset).Take(count).ToArray().GetString().Trim();

                var tokens = stringa.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var token in tokens)
                {
                    var message = "(" + (DateTime.Now - _lastCommand).TotalMilliseconds.ToString("F0") + ") C: " + token;

                    _lastCommand = DateTime.Now;

                    Console.WriteLine(message);
                    _messageTrace.Add(message);
                }

                _messageTrace.Add(string.Empty);
            }
        }

        public void LogServer(byte[] buffer, int offset, int count)
        {
            var stringa = buffer.Skip(offset).Take(count).ToArray().GetString().Trim();

            var tokens = stringa.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var token in tokens)
            {
                //dentro _lastStatusCode tengo traccia solo degli errori
                if (token.StartsWith("4") || token.StartsWith("5"))
                    _lastStatusCode = token;

                //non tengo traccia dei dati inviati al server, quando il server risponde questo codice,
                //il client alla prossima chiamata invia i dati
                if (token.StartsWith("354"))
                    _data = 1;

                var messaggio = "(" + (DateTime.Now - _lastCommand).TotalMilliseconds.ToString("F0") + ") S: " + token;

                Console.WriteLine(messaggio);
                _messageTrace.Add(messaggio);
                
            }

            _lastCommand = DateTime.Now;

            _messageTrace.Add(string.Empty);

            if (_data != 1)
                _data = 0;
        }
    }
}
