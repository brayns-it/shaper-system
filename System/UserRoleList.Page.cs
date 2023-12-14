namespace Brayns.System
{
    public class UserRoleList : Page<UserRoleList, UserRole>
    {
        public UserRoleList()
        {
            UnitCaption = Label("User roles");
            Card = typeof(UserRoleCard);

            var area = Controls.ContentArea.Create(this);
            {
                var grid = new Controls.Grid(area);
                {
                    new Controls.Field(grid, Rec.RoleCode);
                }
            }
        }
    }
}
