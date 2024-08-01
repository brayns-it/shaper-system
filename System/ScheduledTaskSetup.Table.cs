namespace Brayns.System
{
    public class ScheduledTaskSetup : Table<ScheduledTaskSetup>
    {
        public Fields.Code PrimaryKey { get; } = new("Primary key", Label("Primary key"), 20);
        public Fields.Code MailProfile { get; } = new("Mail profile", Label("Mail profile"), 10);
        public Fields.Text MailSenderAddress { get; } = new("Mail sender address", Label("Mail sender address"), 100);
        public Fields.Text MailRecipientAddress { get; } = new("Mail recipient address", Label("Mail recipient address"), 100);
        public Fields.Boolean NotifyIfError { get; } = new("Notify if error", Label("Notify if error"));

        public ScheduledTaskSetup()
        {
            TableName = "Scheduled task setup";
            UnitCaption = Label("Scheduled task setup");
            TablePrimaryKey.Add(PrimaryKey);

            AddRelation<MailSetup>(MailProfile);
        }
    }
}
