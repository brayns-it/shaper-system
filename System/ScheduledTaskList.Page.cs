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

            var actions = Controls.ActionArea.Create(this);
            {
                var enable = new Controls.Action(actions, Label("Enable"), Icon.FromName("fas fa-check"));
                enable.Triggering += () =>
                {
                    Rec.Refresh();
                    Rec.SetEnabled();
                    Update();
                };

                var disable = new Controls.Action(actions, Label("Disable"), Icon.FromName("fas fa-ban"));
                disable.Triggering += () =>
                {
                    Rec.Refresh();
                    Rec.SetDisabled();
                    Update();
                };

                var setup = new Controls.Action(actions, Label("Setup"), Icon.FromName("fas fa-gear"))
                {
                    Run = typeof(ScheduledTaskSetupCard)
                };
            }
        }
    }
}
