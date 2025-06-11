using Business.Collection;
using Business.Entity;
using DnsClient;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Cryptography;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using DnsClient.Protocol;

namespace Business.Code
{
    public static class Sender
    {
        private static readonly LookupClient Lookup = new LookupClient(new LookupClientOptions()
        {
            UseCache = true,
            MaximumCacheTimeout = TimeSpan.FromSeconds(10),
        });

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private static DnsClient.Protocol.MxRecord[] GetMx(out string avviso, string domain)
        {
            avviso = string.Empty;

            try
            {
                var result = Lookup.Query(domain, QueryType.MX);

                if (result.HasError)
                {
                    avviso = result.ErrorMessage;
                    return Array.Empty<MxRecord>();
                }

                return result.AllRecords.MxRecords().ToArray();
            }
            catch
            {
                
            }

            return Array.Empty<MxRecord>();
        }

     //   public static bool InviaEmail(out string avviso, SmtpRelayerApi.Email email)
     //   {
     //       var emailAddress = email.DestinatarioEmail;

     //       var mailFarmsDominio = Impostazioni.GetItem(SmtpRelayerApi.RemoteEntity.Impostazioni.ImpostazioniEnum.DkimDominio).Valore;
     //       var dominioEmail = Email.GetDomain(emailAddress);

     //       var data = DateTime.Now;

     //       var message = new MimeMessage();

     //       message.Headers.Clear();

     //       if (!string.IsNullOrEmpty(email.RispondiA))
     //           message.Headers.Add(HeaderId.ReplyTo, email.RispondiA);
     //       else
     //           message.Headers.Add(HeaderId.ReplyTo, email.MittenteEmail);
            
     //       message.Headers.Add("X-Complaints-To", "abuse@" + mailFarmsDominio);

     //       try
     //       {

     //           //https://support.google.com/mail/answer/81126?hl=en

     //           if (!string.IsNullOrEmpty(email.UrlEliminazione))
     //           {
     //               /*
     //                *
     //                Add the following headers for one-click unsubscribe as described in RFC 8058:
     //List-Unsubscribe-Post: List-Unsubscribe=One-Click
     //List-Unsubscribe: <https://example.com/unsubscribe/opaquepart>

     //       If the recipient unsubscribes, you'll get this POST request:

     //POST /unsubscribe/opaquepart HTTP/1.1
     //Host: example.com
     //Content-Type: application/x-www-form-urlencoded
     //Content-Length: 26

     //List-Unsubscribe=One-Click*
     //                *
     //                */
     //               //message.Headers.Add(HeaderId.ListUnsubscribe, "<mailto:bounce-" + email.UniqueIdentifier + "@" + dominio + ">, <" + email.UrlEliminazione + ">");
     //           }

     //           message.Headers.Add(HeaderId.MessageId, "<" + email.UniqueIdentifier + "@" + mailFarmsDominio + ">");

     //           if (email.DestinatarioDataRegistrazione != default(DateTime))
     //               message.Headers.Add("Require-Recipient-Valid-Since", email.DestinatarioEmail + "; " + email.DestinatarioDataRegistrazione.To2822());

     //           message.Headers.Add(HeaderId.XMailer, "mailfarms.com_service");
     //           message.Headers.Add(HeaderId.Date, data.To2822());
     //           message.Subject = email.Oggetto;
     //           message.Date = data;
     //           message.Importance = MessageImportance.Normal;
     //           message.From.Add(new MailboxAddress(Encoding.UTF8, email.MittenteNome, email.MittenteEmail));

     //           try
     //           {
     //               message.To.Add(new MailboxAddress(Encoding.UTF8, email.DestinatarioNome, email.DestinatarioEmail));
     //           }
     //           catch (Exception ex)
     //           {
     //               avviso = ex.ToString();
     //               return false;
     //           }

     //           var bb = new BodyBuilder
     //           {
     //               HtmlBody = email.Contenuto
     //           };

     //           if (email.Allegati != null)
     //           {
     //               foreach (var allegato in email.Allegati)
     //                   bb.Attachments.Add(allegato.FileName.FileNameCompatible(), allegato.FileBytes);
     //           }

     //           message.Body = bb.ToMessageBody();
     //           message.Prepare(EncodingConstraint.SevenBit);


     //           var dkimPrivateKey = Impostazioni.GetItem(SmtpRelayerApi.RemoteEntity.Impostazioni.ImpostazioniEnum.DkimPrivateKey).Valore;
     //           var dkimKeySelector = Impostazioni.GetItem(SmtpRelayerApi.RemoteEntity.Impostazioni.ImpostazioniEnum.DkimSelector).Valore;

     //           if (!dkimPrivateKey.IsNullOrEmpty() && !dkimKeySelector.IsNullOrEmpty())
     //           {
     //               var reader = new StringReader(dkimPrivateKey);

     //               var r = new Org.BouncyCastle.OpenSsl.PemReader(reader);

     //               var o = r.ReadObject() as AsymmetricCipherKeyPair;

     //               var signer = new DkimSigner(o.Private, mailFarmsDominio, dkimKeySelector);

     //               var headers = new List<HeaderId>
     //               {
     //                   HeaderId.Sender,
     //                   HeaderId.From,
     //                   HeaderId.To,
     //                   HeaderId.Subject,
     //                   HeaderId.MessageId,
     //                   HeaderId.Date,
     //                   HeaderId.MimeVersion
     //               };

     //               signer.Sign(message, headers);
     //           }

     //       }
     //       catch (Exception ex)
     //       {
     //           avviso = ex.ToString();
     //           return false;
     //       }

     //       var logger = new EmailLogger(email.UniqueIdentifier);

     //       using (var client = new SmtpClient(logger)
     //       {
     //           Timeout = 30000,
     //           LocalDomain = mailFarmsDominio,
     //           ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true
     //       })
     //       {
     //           client.AuthenticationMechanisms.Remove("XOAUTH2");
     //           client.CheckCertificateRevocation = false;

     //           Console.WriteLine("Cerco i server MX");

     //           var mxRecords = GetMx(out avviso, dominioEmail);

     //           if (!string.IsNullOrEmpty(avviso))
     //               return false;

     //           if (mxRecords.Length == 0)
     //           {
     //               avviso = "Non ci sono server MX";
     //               Console.WriteLine(avviso);
     //               return false;
     //           }


     //           foreach (var record in mxRecords)
     //           {
     //               //provo tutti i server finchè non parte
     //               try
     //               {
     //                   var ip = Dns.GetHostEntry(record.Exchange);

     //                   var host = ip.HostName;

     //                   try
     //                   {
     //                       Console.WriteLine("Mi collego a " + host + " ...");
                            
     //                       client.Connect(host, options: MailKit.Security.SecureSocketOptions.Auto);

     //                       Console.WriteLine(emailAddress + ": Collegato con successo");

     //                       try
     //                       {
     //                           Console.WriteLine(emailAddress + ": Provo ad inviare la mail");

     //                           client.Send(message);

     //                           Console.WriteLine(emailAddress + ": Email inviata con successo");

     //                           return true;
     //                       }
     //                       catch (SmtpCommandException ex)
     //                       {
     //                           Console.WriteLine("Error sending message: {0}", ex);
     //                           Console.WriteLine("\tStatusCode: {0}", ex.StatusCode);

     //                           switch (ex.ErrorCode)
     //                           {
     //                               case SmtpErrorCode.RecipientNotAccepted:
     //                                   Console.WriteLine("\tRecipient not accepted: {0}", ex.Mailbox);
     //                                   break;
     //                               case SmtpErrorCode.SenderNotAccepted:
     //                                   Console.WriteLine("\tSender not accepted: {0}", ex.Mailbox);
     //                                   break;
     //                               case SmtpErrorCode.MessageNotAccepted:
     //                                   Console.WriteLine("\tMessage not accepted.");
     //                                   break;
     //                               case SmtpErrorCode.UnexpectedStatusCode:
     //                                   Console.WriteLine("\tUnexpectedStatusCode.");
     //                                   break;
     //                           }

     //                           avviso = "ErrorCode: " + ex.ErrorCode + ", StatusCode: " + ex.StatusCode;

     //                           //se salta l'invio esco
     //                           return false;
     //                       }
     //                       catch (SmtpProtocolException ex)
     //                       {
     //                           //Console.WriteLine("Protocol error while sending message: {0}", ex.Message);
     //                           avviso = emailAddress + ": " + ex;

     //                           //se salta l'invio esco
     //                           return false;
     //                       }
     //                   }
     //                   catch (Exception ex)
     //                   {
     //                       //se salta la connessione provo con un altro server MX
     //                       avviso = emailAddress + ": " + ex;

     //                       Console.WriteLine(avviso);
     //                   }
     //                   finally
     //                   {
     //                       try
     //                       {
     //                           client.Disconnect(true);
     //                       }
     //                       catch
     //                       {

     //                       }
     //                   }
     //               }
     //               catch (Exception ex)
     //               {
     //                   avviso = emailAddress + ": " + ex;
     //               }
     //           }
     //       }

     //       return false;
     //   }

     //   internal static bool TestEmail(string email)
     //   {
     //       if (!email.IsEmail())
     //           return false;

     //       var domain = SmtpRelayerApi.Email.GetDomain(email);

     //       using (var client = new SmtpClient
     //       {
     //           ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true
     //       })
     //       {
     //           var mxRecords = GetMx(out var avviso, domain);

     //           if (!string.IsNullOrEmpty(avviso))
     //               return false;

     //           if (mxRecords.Length == 0)
     //               return false;

     //           foreach (var record in mxRecords)
     //           {
     //               try
     //               {
     //                   var ip = Dns.GetHostEntry(record.Exchange);

     //                   string host = ip.HostName;

     //                   try
     //                   {
     //                       client.Connect(host, options: MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable);

     //                       var response = client.Verify(email);

     //                       if (response != null)
     //                           return true;
     //                   }
     //                   catch //5.1.1  metodo non implementato, presumo che la mail esista
     //                   {
     //                       return true;
     //                   }
     //                   finally
     //                   {
     //                       client.Disconnect(true);
     //                   }
     //               }
     //               catch
     //               {
     //                   return false;
     //               }
     //           }
     //       }

     //       return true;
     //   }

     //   public static SmtpRelayerApi.Email PreparaMail(IEmail coda)
     //   {
     //       var email = new SmtpRelayerApi.Email
     //       {
     //           DestinatarioEmail = coda.DestinatarioEmail,
     //           Contenuto = coda.Contenuto,
     //           MittenteNome = coda.MittenteNome,
     //           Oggetto = coda.Oggetto,
     //           DestinatarioNome = coda.DestinatarioNome,
     //           MittenteEmail = coda.MittenteEmail,
     //           DestinatarioDataRegistrazione = coda.DestinatarioDataRegistrazione,
     //           UniqueIdentifier = coda.UniqueIdentifier,
     //           UrlEliminazione = coda.UrlEliminazione,
     //           RispondiA = coda.RispondiA,
     //           Immediata = coda.Immediata
     //       };

     //       var emailAllegati = EmailAllegatiCollection.GetList(wherePredicate: "IdEmail == " + coda.Id);

     //       if (emailAllegati.Any())
     //       {
     //           email.Allegati = new List<Email.Allegato>();

     //           foreach (var emailAllegato in emailAllegati)
     //           {
     //               email.Allegati.Add(new Email.Allegato
     //               {
     //                   FileBytes = File.ReadAllBytes(emailAllegato.PercorsoDisco).Decompress(),
     //                   FileName = emailAllegato.NomeFile
     //               });
     //           }
     //       }

     //       return email;
     //   }
    }
}
