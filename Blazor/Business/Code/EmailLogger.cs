using Business.Entity;
using MailKit;
using System;
using System.Linq;
using System.Text;
using CommonNetCore.GlobalExtension;
using CommonNetCore.Misc;

namespace Business.Code
{
    public class EmailLogger : IProtocolLogger
    {
        private readonly string _uniqueIdentifier;

        private readonly StringBuilder _sb = new StringBuilder(10);

        private string _lastStatusCode;
        private int _data;

        public EmailLogger(string uniqueIdentifier)
        {
            _uniqueIdentifier = uniqueIdentifier;
        }

        public void Dispose()
        {
            var email = Email.GetItem(_uniqueIdentifier);

            //nel caso invii una email di anteprima
            if (email == null)
                return;

            var testo = _sb.ToString();

            if (testo.IsNullOrEmpty())
                return;

            var emailLog = new EmailLog
            {
                Email = email,
                Data = DateTime.Now,
                Testo = _sb.ToString()
            };

            if (!emailLog.Save(out var avviso))
                ManagerLog.Error(avviso);

            if (!string.IsNullOrEmpty(_lastStatusCode))
                email.StatusCode4xx5xx = _lastStatusCode;

            if (!email.Save(out avviso))
                ManagerLog.Error(avviso);
        }

        public void LogConnect(Uri uri)
        {

        }

        public void LogClient(byte[] buffer, int offset, int count)
        {
            if (_data == 1)
            {
                Console.Write("C: Invio i byte della mail ..." + Environment.NewLine);
                _sb.Append("C: Invio i byte della mail ..." + Environment.NewLine);
                _data++;
            }

            if (_data == 0)
            {
                Console.Write(Environment.NewLine);

                _sb.Append(Environment.NewLine);

                var stringa = buffer.Skip(offset).Take(count).ToArray().GetString().Trim();

                var tokens = stringa.Split('\n');

                foreach (var token in tokens)
                {
                    Console.Write(token + Environment.NewLine);

                    _sb.Append(token + Environment.NewLine);
                }
            }
        }

        public void LogServer(byte[] buffer, int offset, int count)
        {
            Console.Write(Environment.NewLine);

            _sb.Append(Environment.NewLine);

            var stringa = buffer.Skip(offset).Take(count).ToArray().GetString().Trim();

            var tokens = stringa.Split('\n');

            foreach (var token in tokens)
            {
                //tengo traccia solo degli errori
                if (token.StartsWith("4") || token.StartsWith("5"))
                    _lastStatusCode = token;

                //non tengo traccia dei dati inviati al server, quando il server risponde questo codice,
                //il client alla prossima chiamata invia i dati
                if (token.StartsWith("354"))
                    _data = 1;

                Console.Write(token + Environment.NewLine);

                _sb.Append(token + Environment.NewLine);
            }

            if (_data != 1)
                _data = 0;
        }

        public IAuthenticationSecretDetector AuthenticationSecretDetector { get; set; }
    }
}
