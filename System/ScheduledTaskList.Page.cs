namespace Brayns.System
{
    public class ScheduledTaskList : Page<ScheduledTaskList, ScheduledTask>
    {
        public ScheduledTaskList()
        {
            UnitCaption = Label("Scheduled tasks");
            Card = typeof(ScheduledTaskCard);

            var area = Controls.ContentArea.Create(this);
            {
                var grid = new Controls.Grid(area);
                {
                    new Controls.Field(grid, Rec.Description);
                    new Controls.Field(grid, Rec.ObjectName);
                    new Controls.Field(grid, Rec.MethodName);
                    new Controls.Field(grid, Rec.NextRunTime);
                    new Controls.Field(grid, Rec.Status);
                }
            }
        }
    }
}
