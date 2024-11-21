namespace Brayns.System
{
    public class ScheduledTaskCard : Page<ScheduledTaskCard, ScheduledTask>
    {
        public ScheduledTaskCard()
        {
            UnitCaption = Label("Scheduled task");

            var content = Controls.ContentArea.Create(this);
            {
                var general = new Controls.Group(content, "general", Label("General"));
                {
                    new Controls.Field(general, Rec.Description);
                    new Controls.Field(general, Rec.ObjectName);
                    new Controls.Field(general, Rec.MethodName);
                    new Controls.Field(general, Rec.Parameter);
                    new Controls.Field(general, Rec.Status) { ReadOnly = true };
                    new Controls.Field(general, Rec.NextRunTime);
                    new Controls.Field(general, Rec.ReferenceCode);
                    new Controls.Field(general, Rec.IntervalSec);
                    new Controls.Field(general, Rec.MaximumRetries);
                    new Controls.Field(general, Rec.RetrySec);

                    var runAlways = new Controls.Field(general, Rec.RunAlways);
                    runAlways.Validating += () => ToggleScheduleVisible();
                }

                var schedule = new Controls.Group(content, "schedule", Label("Schedule"));
                {
                    schedule.Visible = false;

                    new Controls.Field(schedule, Rec.StartingTime);
                    new Controls.Field(schedule, Rec.EndingTime);
                    new Controls.Field(schedule, Rec.RunOnMonday);
                    new Controls.Field(schedule, Rec.RunOnTuesday);
                    new Controls.Field(schedule, Rec.RunOnWednesday);
                    new Controls.Field(schedule, Rec.RunOnThursday);
                    new Controls.Field(schedule, Rec.RunOnFriday);
                    new Controls.Field(schedule, Rec.RunOnSaturday);
                    new Controls.Field(schedule, Rec.RunOnSunday);
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

                    var debug = new Controls.Action(task, Label("Debug"), Icon.FromName("fas fa-bug"));
                    debug.Triggering += () =>
                    {
                        Rec.Reload();
                        Rec.StartAsDebug();
                        Update();
                    };
                }
            }

            DataReading += () => ToggleScheduleVisible();
        }

        void ToggleScheduleVisible()
        {
            Control("schedule")!.Visible = !Rec.RunAlways.Value;
            Control<Controls.ContentArea>()!.Redraw();
        }
    }
}
