using System.Net.Mail;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Business.Code
{
    /// <summary>
    /// Verificare la risposta del server se ha inviato la mail
    /// https://stackoverflow.com/questions/48118781/asp-net-mailkit-smtp-response
    /// https://github.com/jstedfast/MailKit/issues/602
    /// https://stackoverflow.com/questions/45027910/get-the-delivery-status-of-email-with-mimekit-mailkit-library
    /// </summary>
    public class DSNSmtpClient : SmtpClient
    {
        protected override DeliveryStatusNotification? GetDeliveryStatusNotifications(MimeMessage message, MailboxAddress mailbox)
        {
            return DeliveryStatusNotification.Never |
                   DeliveryStatusNotification.Delay |
                   DeliveryStatusNotification.Failure |
                   DeliveryStatusNotification.Success;
        }

        public DeliveryStatusNotification? GetStatus(MimeMessage message)
        {
            return GetDeliveryStatusNotifications(message, message.Sender);
        }
    }
}
