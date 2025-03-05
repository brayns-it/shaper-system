namespace Brayns.System
{
    public class MailSetupProtocol : OptList
    {
        [Label("SMTP")]
        public const int SMTP = 0;
    }

    public class MailSetupAuthentication : OptList
    {
        public const int NONE = 0;

        [Label("Plain")]
        public const int PLAIN = 1;

        [Label("Office 365 OAuth")]
        public const int O365_OAUTH = 2;
    }

    public class MailSetup : Table<MailSetup>
    {
        public Fields.Code ProfileCode { get; } = new("Profile code", Label("Profile code"), 10);
        public Fields.Text Description { get; } = new("Description", Label("Description"), 30);
        public Fields.Option<MailSetupProtocol> Protocol { get; } = new("Protocol", Label("Protocol"));
        public Fields.Boolean Default { get; } = new("Default", Label("Default"));
        public Fields.Text SmtpServer { get; } = new("SMTP server", Label("SMTP server"), 200);
        public Fields.Integer SmtpPort { get; } = new("SMTP port", Label("SMTP port")) { BlankZero = true };
        public Fields.Text SmtpUser { get; } = new("SMTP user", Label("SMTP user"), 100);
        public Fields.Text SmtpPassword { get; } = new("SMTP password", Label("SMTP password"), 100);
        public Fields.Text SmtpSender { get; } = new("SMTP sender", Label("SMTP sender"), 100);
        public Fields.Text SmtpSenderName { get; } = new("SMTP sender name", Label("SMTP sender name"), 100);
        public Fields.Boolean SmtpUseTls { get; } = new("SMTP use TLS", Label("SMTP use TLS"));
        public Fields.Option<MailSetupAuthentication> Authentication { get; } = new("Authentication", Label("Authentication"));
        public Fields.Text ClientID { get; } = new("Client ID", Label("Client ID"), 100);
        public Fields.Text TenantID { get; } = new("Tenant ID", Label("Tenant ID"), 100);
        public Fields.Text ClientSecret { get; } = new("Client secret", Label("Client secret"), 100);

        public MailSetup()
        {
            TableName = "Mail setup";
            UnitCaption = Label("Mail setup");
            TablePrimaryKey.Add(ProfileCode);

            SmtpPassword.Validating += SmtpPassword_Validating;
            ClientSecret.Validating += ClientSecret_Validating;
        }

        private void ClientSecret_Validating()
        {
            ClientSecret.Value = Functions.EncryptString(ClientSecret.Value);
        }

        private void SmtpPassword_Validating()
        {
            SmtpPassword.Value = Functions.EncryptString(SmtpPassword.Value);
        }
    }
}
