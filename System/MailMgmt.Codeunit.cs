using System.Net;
using System.Net.Mail;

namespace Brayns.System
{
    public class MailMgmt : Codeunit
    {
        public MailSetup Setup { get; private set; }
        public MailMessage Message { get; set; }

        public MailMgmt()
        {
            Setup = new MailSetup();
            Setup.Default.SetRange(true);
            Setup.FindFirst();

            Message = new();
        }

        public void SetProfile(string code)
        {
            if (!Setup.Get(code))
                Setup.Init();
        }

        public void Send()
        {
            switch (Setup.Protocol.Value)
            {
                case MailSetupProtocol.SMTP:
                    SendSmtp();
                    break;

                default:
                    throw new Error(Label("Invalid mail protocol"));
            }
        }

        private bool HasHeader(string key)
        {
            foreach (string hdr in Message.Headers.Keys)
                if (hdr.Equals(key, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        private void SendSmtp()
        {
            Setup.SmtpServer.Test();

            var client = new SmtpClient(Setup.SmtpServer.Value);
            if (Setup.SmtpPort.Value > 0) client.Port = Setup.SmtpPort.Value;
            if (Setup.SmtpUseTls.Value) client.EnableSsl = true;

            if (Setup.SmtpUser.Value.Length > 0)
                client.Credentials = new NetworkCredential(Setup.SmtpUser.Value, Functions.DecryptString(Setup.SmtpPassword.Value));

            if ((Message.From == null) && (Setup.SmtpSender.Value.Length > 0))
                Message.From = new MailAddress(Setup.SmtpSender.Value);

            if (!HasHeader("Message-ID"))
                Message.Headers.Add("Message-ID", "<" + Guid.NewGuid().ToString("n") + "@" + CurrentSession.Server + ">");

            client.Send(Message);
        }

    }
}
