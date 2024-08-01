namespace Brayns.System
{
    public class ScheduledTaskSetupCard : Page<ScheduledTaskSetupCard,ScheduledTaskSetup>
    {
        public ScheduledTaskSetupCard()
        {
            UnitCaption = Label("Scheduled task setup");

            var area = Controls.ContentArea.Create(this);
            {
                var general = new Controls.Group(area, Label("Notifications"));
                {
                    new Controls.Field(general, Rec.MailProfile);
                    new Controls.Field(general, Rec.MailSenderAddress);
                    new Controls.Field(general, Rec.MailRecipientAddress);
                    new Controls.Field(general, Rec.NotifyIfError);
                }
            }
        }
    }
}
