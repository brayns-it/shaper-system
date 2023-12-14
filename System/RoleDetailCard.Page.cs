namespace Brayns.System
{
    public class RoleDetailCard : Page<RoleDetailCard, RoleDetail>
    {
        public RoleDetailCard()
        {
            UnitCaption = Label("Role detail");
            AutoIncrementKey = true;

            var area = Controls.ContentArea.Create(this);
            {
                var general = new Controls.Group(area, Label("General"));
                {
                    new Controls.Field(general, Rec.ObjectType);
                    new Controls.Field(general, Rec.ObjectName);
                    new Controls.Field(general, Rec.Execution);
                }
            }
        }
    }
}
