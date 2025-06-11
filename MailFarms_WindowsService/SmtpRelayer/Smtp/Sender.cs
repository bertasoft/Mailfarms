using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Business.Entity;
using CommonNetCore.GlobalExtension;
using CommonNetCore.Misc;
using DnsClient;
using DnsClient.Protocol;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Cryptography;
using Org.BouncyCastle.Crypto;

namespace SmtpRelayer.Smtp
{
    public static class Sender
    {
        private static readonly LookupClient Lookup;

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        static Sender()
        {
            Lookup = new LookupClient(new LookupClientOptions()
            {
                UseCache = true,
                Timeout = TimeSpan.FromSeconds(10),
                MinimumCacheTimeout = TimeSpan.FromSeconds(10)
            });
        }

        private static async Task<Tuple<string, MxRecord[]>> GetMx(string domain)
        {
            try
            {
                var result = await Lookup.QueryAsync(domain, QueryType.MX).ConfigureAwait(false);

                if (result.HasError)
                    return new Tuple<string, MxRecord[]>(result.ErrorMessage, Array.Empty<MxRecord>());

                return new Tuple<string, MxRecord[]>(string.Empty, result.AllRecords.MxRecords().ToArray());
            }
            catch (Exception ex)
            {
                return new Tuple<string, MxRecord[]>(ex.Message, Array.Empty<MxRecord>());
            }
        }

        public static async Task<Tuple<bool, string>> InviaEmail(Email email, string taskName)
        {
            Console.WriteLine("Task [" + taskName + "]: Inizio l'invio a " + email.DestinatarioEmail);

            var emailAddress = email.DestinatarioEmail;

            var mailFarmsDominio = Impostazioni.GetValore(Impostazioni.ImpostazioniEnum.DkimDominio);
            var dominioEmail = Email.GetDomain(emailAddress);

            var data = DateTime.Now;

            var message = new MimeMessage();

            message.Headers.Clear();

            message.Headers.Add(HeaderId.ReplyTo, !string.IsNullOrEmpty(email.RispondiA) ? email.RispondiA : email.MittenteEmail);

            message.Headers.Add("X-Complaints-To", "abuse@" + mailFarmsDominio);

            try
            {

                //si può fare anche con POST
                //https://support.google.com/mail/answer/81126?hl=en

                if (!string.IsNullOrEmpty(email.UrlEliminazione))
                    message.Headers.Add(HeaderId.ListUnsubscribe, Encoding.UTF8, "<" + email.UrlEliminazione + ">");

                message.Headers.Add(HeaderId.MessageId, "<" + email.UniqueIdentifier + "@" + mailFarmsDominio + ">");

                if (email.DestinatarioDataRegistrazione != default(DateTime))
                    message.Headers.Add("Require-Recipient-Valid-Since", email.DestinatarioEmail + "; " + email.DestinatarioDataRegistrazione.To2822());

                message.Headers.Add(HeaderId.XMailer, "mailfarms.com_service");
                message.Headers.Add(HeaderId.Date, data.To2822());
                message.Subject = email.Oggetto;
                message.Date = data;
                message.Importance = MessageImportance.Normal;
                message.From.Add(new MailboxAddress(Encoding.UTF8, email.MittenteNome.Decode(), email.MittenteEmail));

                try
                {
                    message.To.Add(new MailboxAddress(Encoding.UTF8, email.DestinatarioNome.Decode(), email.DestinatarioEmail));
                }
                catch (Exception ex)
                {
                    return new Tuple<bool, string>(false, ex.ToString());
                }

                var bb = new BodyBuilder
                {
                    HtmlBody = email.Contenuto,
                    TextBody = email.Contenuto.StripTagsCharArray().Decode()
                };

                if (email.Allegati != null)
                {
                    foreach (var allegato in email.Allegati)
                    {
                        if (string.IsNullOrEmpty(allegato.NomeFile))
                            continue;

                        var bytes = await allegato.FileBytesAsync();

                        if (bytes == null || bytes.Length == 0)
                            continue;

                        bb.Attachments.Add(allegato.NomeFile.FileNameCompatible(), bytes);
                    }
                }

                message.Body = bb.ToMessageBody();
                message.Prepare(EncodingConstraint.SevenBit);


                var dkimPrivateKey = Impostazioni.GetItem(Impostazioni.ImpostazioniEnum.DkimPrivateKey).Valore;
                var dkimKeySelector = Impostazioni.GetItem(Impostazioni.ImpostazioniEnum.DkimSelector).Valore;

                if (!dkimPrivateKey.IsNullOrEmpty() && !dkimKeySelector.IsNullOrEmpty())
                {
                    using var reader = new StringReader(dkimPrivateKey);

                    var r = new Org.BouncyCastle.OpenSsl.PemReader(reader);

                    var o = r.ReadObject() as AsymmetricCipherKeyPair;

                    var signer = new DkimSigner(o.Private, mailFarmsDominio, dkimKeySelector);

                    var headers = new List<HeaderId>
                    {
                        HeaderId.Sender,
                        HeaderId.From,
                        HeaderId.To,
                        HeaderId.Subject,
                        HeaderId.MessageId,
                        HeaderId.Date,
                        HeaderId.MimeVersion
                    };

                    signer.Sign(message, headers);
                }

            }
            catch (Exception ex)
            {
                return new Tuple<bool, string>(false, ex.ToString());
            }

            var avviso = string.Empty;

            
            using var client = new SmtpClient(new SmtpLogger(email.UniqueIdentifier)); //dispose di smtplogger viene chiamato dal dispose del client

            client.Timeout = 120000; //2 minuti
            client.LocalDomain = Impostazioni.GetValore(Impostazioni.ImpostazioniEnum.Helo);
            client.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
            client.CheckCertificateRevocation = false;
            client.SslProtocols = SslProtocols.Ssl2 | SslProtocols.Ssl3 | SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13;

            Console.WriteLine("Task [" + taskName + "]: " + email.DestinatarioEmail + ", cerco i server MX");

            var tuple = await GetMx(dominioEmail).ConfigureAwait(false);

            var mxRecords = tuple.Item2;

            if (!string.IsNullOrEmpty(tuple.Item1))
                return new Tuple<bool, string>(false, tuple.Item1);

            if (mxRecords.Length == 0)
            {
                avviso = "Task [" + taskName + "]: " + email.DestinatarioEmail + ", non ci sono server MX";
                Console.WriteLine(avviso);
                return new Tuple<bool, string>(false, avviso);
            }

            foreach (var record in mxRecords)
            {
                //provo tutti i server finchè non parte
                try
                {
                    var ip = await Dns.GetHostEntryAsync(record.Exchange).ConfigureAwait(false);

                    var host = ip.HostName;

                    try
                    {
                        Console.WriteLine("Task [" + taskName + "]: " + email.DestinatarioEmail + ", mi collego a " + host + " ...");

                        await client.ConnectAsync(host, options: MailKit.Security.SecureSocketOptions.Auto).ConfigureAwait(false);

                        Console.WriteLine("Task [" + taskName + "]: " + email.DestinatarioEmail + ", collegato con successo");

                        try
                        {
                            Console.WriteLine("Task [" + taskName + "]: " + email.DestinatarioEmail + ", provo ad inviare la mail");

#if RELEASE
                            await client.SendAsync(message).ConfigureAwait(false);
#else
                            await File.AppendAllTextAsync("C:\\Email\\MailFarmsWindowsServiceLog.txt", email.Oggetto + ", " + email.DestinatarioEmail + ", " + host + Environment.NewLine);
#endif

                            Console.WriteLine("Task [" + taskName + "]: " + email.DestinatarioEmail + ", email inviata con successo");

                            return new Tuple<bool, string>(true, string.Empty);
                        }
                        catch (SmtpCommandException ex)
                        {
                            Console.WriteLine("Task [" + taskName + "]: " + email.DestinatarioEmail + ", error sending message: {0}", ex);
                            Console.WriteLine("\tStatusCode: {0}", ex.StatusCode);

                            //NON LOGGO PERCHè HO PERSO GLI AVVISI GRAYLIST
                            //avviso = "Task [" + taskName + "]: " + email.DestinatarioEmail + ", errorCode: " + ex.ErrorCode + ", StatusCode: " + ex.StatusCode;

                            //se salta l'invio esco
                            return new Tuple<bool, string>(false, string.Empty);
                        }
                        catch (SmtpProtocolException ex)
                        {
                            Console.WriteLine("Task [" + taskName + "]: " + email.DestinatarioEmail + ", protocol error while sending message: {0}", ex.Message);

                            //NON LOGGO PERCHè QUESTI ERRORI VENGONO LOGGATI ANCHE COL LOGGER / EVITO DUPLICATI
                            //avviso = emailAddress + ": " + ex;

                            //se salta l'invio esco
                            return new Tuple<bool, string>(false, string.Empty);
                        }
                    }
                    catch (Exception ex)
                    {
                        //se salta la connessione provo con un altro server MX

                        //NON LOGGO PERCHè QUESTI ERRORI VENGONO LOGGATI ANCHE COL LOGGER / EVITO DUPLICATI
                        Console.WriteLine(emailAddress + ": " + ex);
                    }
                    finally
                    {
                        try
                        {
                            await client.DisconnectAsync(true).ConfigureAwait(false);
                        }
                        catch
                        {

                        }
                    }
                }
                catch (Exception ex)
                {
                    avviso = emailAddress + ": " + ex;
                }
            }

            return new Tuple<bool, string>(false, avviso);
        }
    }
}
