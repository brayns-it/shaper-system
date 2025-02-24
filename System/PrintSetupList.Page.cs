namespace Brayns.System
{
    public class PrintSetupList : Page<PrintSetupList, PrintSetup>
    {
        public PrintSetupList()
        {
            UnitCaption = Label("Print setup");
            Card = typeof(PrintSetupCard);

            var content = Controls.ContentArea.Create(this);
            {
                var grid = new Controls.Grid(content);
                {
                    new Controls.Field(grid, Rec.ProfileCode);
                    new Controls.Field(grid, Rec.Description);
                    new Controls.Field(grid, Rec.PrintServer);
                    new Controls.Field(grid, Rec.Default);
                }
            }
        }
    }
}
