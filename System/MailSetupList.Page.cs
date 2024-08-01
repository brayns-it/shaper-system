namespace Brayns.System
{
    public class MailSetupList : Page<MailSetupList, MailSetup>
    {
        public MailSetupList()
        {
            UnitCaption = Label("Mail setup");
            Card = typeof(MailSetupCard);

            var content = Controls.ContentArea.Create(this);
            {
                var grid = new Controls.Grid(content);
                {
                    new Controls.Field(grid, Rec.ProfileCode);
                    new Controls.Field(grid, Rec.Description);
                    new Controls.Field(grid, Rec.Default);
                }
            }
        }
    }
}
