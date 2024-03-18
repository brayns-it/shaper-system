namespace Brayns.System
{
    public class ScheduledTaskStatus : OptList
    {
        [Label("Disabled")]
        public const int DISABLED = 0;

        [Label("Enabled")]
        public const int ENABLED = 1;

        [Label("Running")]
        public const int RUNNING = 2;

        [Label("Error")]
        public const int ERROR = 3;
    }

    public class ScheduledTask : Table<ScheduledTask>
    {
        public Fields.Integer EntryNo { get; } = new("Entry no.", Label("Entry no.")) { AutoIncrement = true };
        public Fields.Text Description { get; } = new("Description", Label("Description"), 100);
        public Fields.Option<ScheduledTaskStatus> Status { get; } = new("Status", Label("Status"));
        public Fields.Boolean RunOnMonday { get; } = new("Run on monday", Label("Run on monday"));
        public Fields.Boolean RunOnTuesday { get; } = new("Run on tuesday", Label("Run on tuesday"));
        public Fields.Boolean RunOnWednesday { get; } = new("Run on wednesday", Label("Run on wednesday"));
        public Fields.Boolean RunOnThursday { get; } = new("Run on thursday", Label("Run on thursday"));
        public Fields.Boolean RunOnFriday { get; } = new("Run on friday", Label("Run on friday"));
        public Fields.Boolean RunOnSaturday { get; } = new("Run on saturday", Label("Run on saturday"));
        public Fields.Boolean RunOnSunday { get; } = new("Run on sunday", Label("Run on sunday"));
        public Fields.Time StartingTime { get; } = new("Starting time", Label("Starting time"));
        public Fields.Time EndingTime { get; } = new("Ending time", Label("Ending time"));
        public Fields.Integer IntervalSec { get; } = new("Interval (sec)", Label("Interval (sec)"));
        public Fields.Text ObjectName { get; } = new("Object name", Label("Object name"), 250);
        public Fields.Text MethodName { get; } = new("Method name", Label("Method name"), 250);
        public Fields.Text Parameter { get; } = new("Parameter", Label("Parameter"), 250);
        public Fields.DateTime NextRunTime { get; } = new("Next run time", Label("Next run time"));

        public ScheduledTask()
        {
            TableName = "Scheduled task";
            UnitCaption = Label("Scheduled task");
            TablePrimaryKey.Add(EntryNo);
        }

        public void SetDisabled()
        {
            Refresh();
            Status.Value = ScheduledTaskStatus.DISABLED;
            Modify();
        }

        public void SetEnabled()
        {
            if (Status.Value == ScheduledTaskStatus.RUNNING)
                throw new Error(Label("Task is running"));

            IntervalSec.Test();

            DateTime dt = DateTime.Today;
            DateTime max = dt.AddDays(8);
            while (dt < max)
            {
                bool today = false;
                if ((dt.DayOfWeek == DayOfWeek.Monday) && (!RunOnMonday.Value)) today = true;
                if ((dt.DayOfWeek == DayOfWeek.Tuesday) && (!RunOnTuesday.Value)) today = true;
                if ((dt.DayOfWeek == DayOfWeek.Wednesday) && (!RunOnWednesday.Value)) today = true;
                if ((dt.DayOfWeek == DayOfWeek.Thursday) && (!RunOnThursday.Value)) today = true;
                if ((dt.DayOfWeek == DayOfWeek.Friday) && (!RunOnFriday.Value)) today = true;
                if ((dt.DayOfWeek == DayOfWeek.Saturday) && (!RunOnSaturday.Value)) today = true;
                if ((dt.DayOfWeek == DayOfWeek.Sunday) && (!RunOnSunday.Value)) today = true;
                if (today)
                {
                    dt = dt.AddDays(1);
                    continue;
                }

                dt += StartingTime.Value.TimeOfDay;

                while (dt <= DateTime.Now)
                    dt = dt.AddSeconds(IntervalSec.Value);

                if (dt.TimeOfDay > EndingTime.Value.TimeOfDay)
                {
                    dt = dt.AddDays(1).Date;
                    continue;
                }

                NextRunTime.Value = dt;
                Status.Value = ScheduledTaskStatus.ENABLED;
                Modify();

                return;
            }

            throw new Error(Label("Unable to calculate next run time"));
        }
    }
}
