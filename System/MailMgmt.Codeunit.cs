using System.Net;
using System.Net.Mail;

namespace Brayns.System
{
    public class MailMgmt : Codeunit
    {
        public MailSetup Setup { get; private set; }

        public MailMgmt()
        {
            Setup = new MailSetup();
            Setup.Default.SetRange(true);
            Setup.FindFirst();
        }

        public void SetProfile(string code)
        {
            if (!Setup.Get(code))
                Setup.Init();
        }

        public void Send(MailMessage message)
        {
            switch (Setup.Protocol.Value)
            {
                case MailSetupProtocol.SMTP:
                    SendSmtp(message);
                    break;

                default:
                    throw new Error(Label("Invalid mail protocol"));
            }
        }

        private bool HasHeader(MailMessage message, string key)
        {
            foreach (string hdr in message.Headers.Keys)
                if (hdr.Equals(key, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        private void SendSmtp(MailMessage message)
        {
            Setup.SmtpServer.Test();

            var client = new SmtpClient(Setup.SmtpServer.Value);
            if (Setup.SmtpPort.Value > 0) client.Port = Setup.SmtpPort.Value;
            if (Setup.SmtpUseTls.Value) client.EnableSsl = true;

            if (Setup.SmtpUser.Value.Length > 0)
                client.Credentials = new NetworkCredential(Setup.SmtpUser.Value, Functions.DecryptString(Setup.SmtpPassword.Value));

            if (!HasHeader(message, "Message-ID"))
                message.Headers.Add("Message-ID", "<" + Guid.NewGuid().ToString("n") + "@" + CurrentSession.Server + ">");

            client.Send(message);
        }

    }
}
