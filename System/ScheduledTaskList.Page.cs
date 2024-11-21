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
                    new Controls.Field(grid, Rec.LastRunTime);
                    new Controls.Field(grid, Rec.Status);
                }
            }

            var actions = Controls.ActionArea.Create(this);
            {
                var task = new Controls.Action(actions, Label("Task"));
                {
                    var enable = new Controls.Action(task, Label("Enable"), Icon.FromName("fas fa-check"));
                    enable.Triggering += () =>
                    {
                        Rec.Reload();
                        Rec.SetEnabled();
                        Update();
                    };

                    var disable = new Controls.Action(task, Label("Disable"), Icon.FromName("fas fa-ban"));
                    disable.Triggering += () =>
                    {
                        Rec.Reload();
                        Rec.SetDisabled();
                        Update();
                    };


                    var startNow = new Controls.Action(task, Label("Start now"), Icon.FromName("fas fa-play"));
                    startNow.Triggering += () =>
                    {
                        Rec.Reload();
                        Rec.StartNow();
                        Update();
                    };
                }

                var setup = new Controls.Action(actions, Label("Setup"), Icon.FromName("fas fa-gear"))
                {
                    Run = typeof(ScheduledTaskSetupCard)
                };
            }
        }
    }
}
