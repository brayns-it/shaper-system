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
                }

                var schedule = new Controls.Group(content, "schedule", Label("Schedule"));
                {
                    new Controls.Field(schedule, Rec.StartingTime);
                    new Controls.Field(schedule, Rec.EndingTime);
                    new Controls.Field(schedule, Rec.IntervalSec);
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
                var enable = new Controls.Action(actions, Label("Enable"), Icon.FromName("fas fa-check"));
                enable.Triggering += () =>
                {
                    Rec.SetEnabled();
                    Update();
                };

                var disable = new Controls.Action(actions, Label("Disable"), Icon.FromName("fas fa-ban"));
                disable.Triggering += () =>
                {
                    Rec.SetDisabled();
                    Update();
                };
            }
        }
    }
}
