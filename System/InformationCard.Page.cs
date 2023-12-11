namespace Brayns.System
{
    public class InformationCard : Page<InformationCard, Information>
    {
        public InformationCard()
        {
            UnitName = "Information Card";
            UnitCaption = Label("Information");

            var content = Controls.ContentArea.Create(this);
            {
                var general = new Controls.Group(content, "general", Label("General"));
                {
                    new Controls.Field(general, Rec.Name);
                    new Controls.Field(general, Rec.Description);
                    new Controls.Field(general, Rec.Footer);
                    new Controls.Field(general, Rec.Indicator);
                }
            }
        }
    }
}
