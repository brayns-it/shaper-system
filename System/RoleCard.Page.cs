namespace Brayns.System
{
    public class RoleCard : Page<RoleCard, Role>
    {
        public RoleCard()
        {
            UnitName = "Role card";
            UnitCaption = Label("Role");

            var area = Controls.ContentArea.Create(this);
            {
                var general = new Controls.Group(area, Label("General"));
                {
                    new Controls.Field(general, Rec.Code);
                    new Controls.Field(general, Rec.Description);
                }
            }
        }
    }
}
