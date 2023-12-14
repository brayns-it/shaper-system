namespace Brayns.System
{
    public class RoleDetailList : Page<RoleDetailList, RoleDetail>
    {
        public RoleDetailList()
        {
            UnitCaption = Label("Role details");
            Card = typeof(RoleDetailCard);

            var area = Controls.ContentArea.Create(this);
            {
                var grid = new Controls.Grid(area);
                {
                    new Controls.Field(grid, Rec.ObjectType);
                    new Controls.Field(grid, Rec.ObjectName);
                    new Controls.Field(grid, Rec.Execution);
                }
            }
        }
    }
}
