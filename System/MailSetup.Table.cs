namespace Brayns.System
{
    public class MailSetupProtocol : OptList
    {
        [Label("SMTP")]
        public const int SMTP = 0;
    }

    public class MailSetup : Table<MailSetup>
    {
        public Fields.Code ProfileCode { get; } = new("Profile code", Label("Profile code"), 10);
        public Fields.Text Description { get; } = new("Description", Label("Description"), 30);
        public Fields.Option<MailSetupProtocol> Protocol { get; } = new("Protocol", Label("Protocol"));
        public Fields.Boolean Default { get; } = new("Default", Label("Default"));
        public Fields.Text SmtpServer { get; } = new("SMTP server", Label("SMTP server"), 200);
        public Fields.Integer SmtpPort { get; } = new("SMTP port", Label("SMTP port")) { BlankZero = true };
        public Fields.Text SmtpUser { get; } = new("SMTP user", Label("SMTP user"), 50);
        public Fields.Text SmtpPassword { get; } = new("SMTP password", Label("SMTP password"), 50);
        public Fields.Text SmtpSender { get; } = new("SMTP sender", Label("SMTP sender"), 100);
        public Fields.Boolean SmtpUseTls { get; } = new("SMTP use TLS", Label("SMTP use TLS"));

        public MailSetup()
        {
            TableName = "Mail setup";
            UnitCaption = Label("Mail setup");
            TablePrimaryKey.Add(ProfileCode);

            SmtpPassword.Validating += SmtpPassword_Validating;
        }

        private void SmtpPassword_Validating()
        {
            SmtpPassword.Value = Functions.EncryptString(SmtpPassword.Value);
        }
    }
}
