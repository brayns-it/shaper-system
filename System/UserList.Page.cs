namespace Brayns.System
{
    public class UserList : Page<UserList, User>
    {
        public UserList()
        {
            UnitCaption = Label("Users");
            Card = typeof(UserCard);

            var area = Controls.ContentArea.Create(this);
            {
                var grid = new Controls.Grid(area);
                {
                    new Controls.Field(grid, Rec.ID);
                    new Controls.Field(grid, Rec.Name);
                    new Controls.Field(grid, Rec.LastLogin);
                    new Controls.Field(grid, Rec.Enabled);
                    new Controls.Field(grid, Rec.Superuser);
                }
            }
        }
    }
}
