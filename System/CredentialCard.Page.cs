using System.Net.Mail;

namespace Brayns.System
{
    public class CredentialCard : Page<CredentialCard, Credential>
    {
        public Fields.Text TestCommand { get; } = new("Test command", Label("Test command"), 100);

        public CredentialCard()
        {
            UnitCaption = Label("Credentials");

            var content = Controls.ContentArea.Create(this);
            {
                var general = new Controls.Group(content, "general", Label("General"));
                {
                    new Controls.Field(general, Rec.Code);
                    new Controls.Field(general, Rec.Description);
                    new Controls.Field(general, Rec.Type);
                    new Controls.Field(general, Rec.Host);
                    new Controls.Field(general, Rec.Username);
                    new Controls.Field(general, Rec.Password) { InputType = Controls.InputType.Password };
                    new Controls.Field(general, Rec.Domain);
                }
            }

            var test = new Controls.Group(content, "test", Label("Test"));
            {
                new Controls.Field(test, TestCommand);

                var testSend = new Controls.Action(test, Label("Try"));
                testSend.Triggering += () => new CredentialMgmt(Rec.Code.Value).Test(TestCommand.Value);
            }
        }
    }
}
