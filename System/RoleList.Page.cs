namespace Brayns.System
{
    public class RoleList : Page<RoleList, Role>
    {
        public RoleList()
        {
            UnitCaption = Label("Roles");
            Card = typeof(RoleCard);

            var area = Controls.ContentArea.Create(this);
            {
                var grid = new Controls.Grid(area);
                {
                    new Controls.Field(grid, Rec.Code);
                    new Controls.Field(grid, Rec.Description);
                }
            }
        }
    }
}
