using System.Net.Mail;

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
                    new Controls.Field(smtp, Rec.SmtpUser);
                    new Controls.Field(smtp, Rec.SmtpPassword) { InputType = Controls.InputType.Password };
                    new Controls.Field(smtp, Rec.SmtpSender);
                    new Controls.Field(smtp, Rec.SmtpUseTls);
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
            var mailMgmt = new MailMgmt();
            mailMgmt.SetProfile(Rec.ProfileCode.Value);

            mailMgmt.Message.From = new MailAddress(Rec.SmtpSender.Value);
            mailMgmt.Message.To.Add(new MailAddress(TestAddress.Value));
            mailMgmt.Message.Subject = Label("Test mail from {0}", CurrentSession.ApplicationName);
            mailMgmt.Message.Body = Label("Test mail from {0} sent via {1}", CurrentSession.ApplicationName, Rec.Description.Value);

            mailMgmt.Send();

            Message.Show(Label("Mail message sent successfully"));
        }
    }
}
