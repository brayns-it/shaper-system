using System.Net.Mail;

namespace Brayns.System
{
    public class PrintSetupCard : Page<PrintSetupCard, PrintSetup>
    {
        public PrintSetupCard()
        {
            UnitCaption = Label("Print setup");

            var content = Controls.ContentArea.Create(this);
            {
                var general = new Controls.Group(content, "general", Label("General"));
                {
                    new Controls.Field(general, Rec.ProfileCode);
                    new Controls.Field(general, Rec.Description);
                    new Controls.Field(general, Rec.PrintServer);
                    new Controls.Field(general, Rec.AuthToken) { InputType = Controls.InputType.Password };
                    new Controls.Field(general, Rec.Default);
                }
            }

            var actions = Controls.ActionArea.Create(this);
            {
                var test = new Controls.Action(actions, Label("Test token"), Icon.FromName("fas fa-bolt"));
                test.Triggering += () => new PrintMgmt(Rec.ProfileCode.Value).Test();
            }
        }
    }
}
