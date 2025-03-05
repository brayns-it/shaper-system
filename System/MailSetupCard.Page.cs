namespace Brayns.System
{
    public class MailSetupCard : Page<MailSetupCard, MailSetup>
    {
        public Fields.Text TestAddress { get; } = new("Test address", Label("Test address"), 100);

        public MailSetupCard()
        {
            UnitCaption = Label("Mail setup");
            AllowInsert = false;
            AllowDelete = false;

            var content = Controls.ContentArea.Create(this);
            {
                var general = new Controls.Group(content, "general", Label("General"));
                {
                    new Controls.Field(general, Rec.ProfileCode);
                    new Controls.Field(general, Rec.Description);
                    new Controls.Field(general, Rec.Protocol);
                    new Controls.Field(general, Rec.Default);
                }

                var smtp = new Controls.Group(content, "smtp", Label("SMTP"));
                {
                    new Controls.Field(smtp, Rec.SmtpServer);
                    new Controls.Field(smtp, Rec.SmtpPort);
                    new Controls.Field(smtp, Rec.SmtpSender);
                    new Controls.Field(smtp, Rec.SmtpSenderName);
                    new Controls.Field(smtp, Rec.SmtpUseTls);
                }

                var auth = new Controls.Group(content, "auth", Label("Authentication"));
                {
                    new Controls.Field(auth, Rec.Authentication);
                    new Controls.Field(auth, Rec.SmtpUser);
                    new Controls.Field(auth, Rec.SmtpPassword) { InputType = Controls.InputType.Password };
                    new Controls.Field(auth, Rec.ClientID);
                    new Controls.Field(auth, Rec.TenantID);
                    new Controls.Field(auth, Rec.ClientSecret) { InputType = Controls.InputType.Password };
                }

                var test = new Controls.Group(content, "test", Label("Test"));
                {
                    new Controls.Field(test, TestAddress);

                    var testSend = new Controls.Action(test, Label("Send test"));
                    testSend.Triggering += TestSend_Triggering;
                }
            }
        }

        private void TestSend_Triggering()
        {
            var mailMgmt = new MailMgmt(Rec.ProfileCode.Value);
            mailMgmt.From = new(Rec.SmtpSender.Value, Rec.SmtpSenderName.Value);
            mailMgmt.To.Add(TestAddress.Value);
            mailMgmt.Subject = Label("Test mail from {0}", CurrentSession.ApplicationName);
            mailMgmt.HtmlBody = Label("Test mail from {0} sent via {1}", CurrentSession.ApplicationName, Rec.Description.Value);
            mailMgmt.Send();

            Message.Show(Label("Mail message sent successfully"));
        }
    }
}
