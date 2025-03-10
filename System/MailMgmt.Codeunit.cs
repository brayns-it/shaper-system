using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using Microsoft.Identity.Client;

namespace Brayns.System
{
    public class MailAddress
    {
        public string Address { get; set; } = "";
        public string Name { get; set; } = "";

        public MailAddress(string address = "", string name = "")
        {
            Address = address;
            Name = name;
        }
    }

    public class MailAddressCollection : List<MailAddress>
    {
        public void Add(string address)
        {
            Add(new MailAddress(address));
        }

        public void Add(string address, string name)
        {
            Add(new MailAddress(address, name));
        }
    }

    public class MailMgmt : Codeunit
    {
        public MailSetup Setup { get; private set; }
        public MailAddress From { get; set; } = new();
        public string Subject { get; set; } = "";
        public MailAddressCollection To { get; } = new();
        public MailAddressCollection Cc { get; } = new();
        public MailAddressCollection Bcc { get; } = new();
        public string HtmlBody { get; set; } = "";

        public MailMgmt(string code = "")
        {
            Setup = new MailSetup();
            if (code.Length == 0)
                Setup.Default.SetRange(true);
            else
                Setup.ProfileCode.SetRange(code);
            Setup.FindFirst();

            From = new MailAddress(Setup.SmtpSender.Value, Setup.SmtpSenderName.Value);
        }

        public bool TrySend()
        {
            try
            {
                Send();
                return true;
            }
            catch
            {
                return false;
            }
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

        private void AuthenticateSmtp(MailKit.Net.Smtp.SmtpClient client)
        {
            switch (Setup.Authentication.Value)
            {
                case MailSetupAuthentication.PLAIN:
                    client.Authenticate(Setup.SmtpUser.Value, Functions.DecryptString(Setup.SmtpPassword.Value));
                    break;

                case MailSetupAuthentication.O365_OAUTH:
                    var confApp = ConfidentialClientApplicationBuilder.Create(Setup.ClientID.Value)
                        .WithAuthority("https://login.microsoftonline.com/" + Setup.TenantID.Value + "/v2.0")
                        .WithClientSecret(Functions.DecryptString(Setup.ClientSecret.Value))
                        .Build();

                    var scopes = new string[] { "https://outlook.office365.com/.default" };
                    var authToken = confApp.AcquireTokenForClient(scopes).ExecuteAsync().Result;
                    client.Authenticate(new SaslMechanismOAuth2(Setup.SmtpUser.Value, authToken.AccessToken));
                    break;

                default:
                    break;
            }
        }

        private void SendSmtp()
        {
            Setup.SmtpServer.Test();

            MimeKit.MimeMessage message = new();
            message.From.Add(new MimeKit.MailboxAddress(From.Name, From.Address));
            message.Sender = new(From.Name, From.Address);

            foreach (var addr in To)
                message.To.Add(new MimeKit.MailboxAddress(addr.Name, addr.Address));

            foreach (var addr in Cc)
                message.Cc.Add(new MimeKit.MailboxAddress(addr.Name, addr.Address));

            foreach (var addr in Bcc)
                message.Bcc.Add(new MimeKit.MailboxAddress(addr.Name, addr.Address));

            message.Subject = Subject;
            message.Body = new MimeKit.TextPart("html", HtmlBody);

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                int port = Setup.SmtpPort.Value;
                if (port == 0) port = Setup.SmtpUseTls.Value ? 587 : 25;

                client.Connect(Setup.SmtpServer.Value, port, Setup.SmtpUseTls.Value ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                AuthenticateSmtp(client);
                client.Send(message);
                client.Disconnect(true);
            }
        }

    }
}
