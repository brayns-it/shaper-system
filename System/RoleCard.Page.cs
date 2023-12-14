namespace Brayns.System
{
    public class RoleCard : Page<RoleCard, Role>
    {
        public RoleCard()
        {
            UnitCaption = Label("Role");

            var area = Controls.ContentArea.Create(this);
            {
                var general = new Controls.Group(area, Label("General"));
                {
                    new Controls.Field(general, Rec.Code);
                    new Controls.Field(general, Rec.Description);
                }

                new Controls.Subpage<RoleDetailList, RoleDetail>(area)
                {
                    Filter = (target) => target.RoleCode.SetRange(Rec.Code.Value)
                };
            }
        }
    }
}
