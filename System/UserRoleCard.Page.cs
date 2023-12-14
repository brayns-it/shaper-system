namespace Brayns.System
{
    public class UserRoleCard : Page<UserRoleCard, UserRole>
    {
        public UserRoleCard()
        {
            UnitCaption = Label("User role");

            var area = Controls.ContentArea.Create(this);
            {
                var general = new Controls.Group(area, Label("General"));
                {
                    new Controls.Field(general, Rec.RoleCode);
                }
            }
        }
    }
}
