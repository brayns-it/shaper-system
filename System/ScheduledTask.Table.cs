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

        [Label("Stopping")]
        public const int STOPPING = 4;

        [Label("Starting")]
        public const int STARTING = 5;
    }

    public class ScheduledTask : Table<ScheduledTask>
    {
        public Fields.Integer EntryNo { get; } = new("Entry no.", Label("Entry no.")) { AutoIncrement = true };
        public Fields.Code ReferenceCode { get; } = new("Reference code", Label("Reference code"), 20);
        public Fields.Text Description { get; } = new("Description", Label("Description"), 100);
        public Fields.Option<ScheduledTaskStatus> Status { get; } = new("Status", Label("Status"));
        public Fields.Boolean RunAlways { get; } = new("Run always", Label("Run always"));
        public Fields.Boolean RunOnMonday { get; } = new("Run on monday", Label("Run on monday"));
        public Fields.Boolean RunOnTuesday { get; } = new("Run on tuesday", Label("Run on tuesday"));
        public Fields.Boolean RunOnWednesday { get; } = new("Run on wednesday", Label("Run on wednesday"));
        public Fields.Boolean RunOnThursday { get; } = new("Run on thursday", Label("Run on thursday"));
        public Fields.Boolean RunOnFriday { get; } = new("Run on friday", Label("Run on friday"));
        public Fields.Boolean RunOnSaturday { get; } = new("Run on saturday", Label("Run on saturday"));
        public Fields.Boolean RunOnSunday { get; } = new("Run on sunday", Label("Run on sunday"));
        public Fields.Time StartingTime { get; } = new("Starting time", Label("Starting time"));
        public Fields.Time EndingTime { get; } = new("Ending time", Label("Ending time"));
        public Fields.Integer MaximumRetries { get; } = new("Maximum retries", Label("Maximum retries")) { BlankZero = true };
        public Fields.Integer RetrySec { get; } = new("Retry (sec)", Label("Retry (sec)")) { BlankZero = true };
        public Fields.Integer CurrentTry { get; } = new("Current try", Label("Current try"));
        public Fields.Integer IntervalSec { get; } = new("Interval (sec)", Label("Interval (sec)"));
        public Fields.Text ObjectName { get; } = new("Object name", Label("Object name"), 250);
        public Fields.Text MethodName { get; } = new("Method name", Label("Method name"), 250);
        public Fields.Text Parameter { get; } = new("Parameter", Label("Parameter"), 250);
        public Fields.DateTime NextRunTime { get; } = new("Next run time", Label("Next run time"));
        public Fields.DateTime LastRunTime { get; } = new("Last run time", Label("Last run time"));
        public Fields.Guid RunningSessionID { get; } = new("Running session ID", Label("Running session ID"));
        public Fields.Text RunningEnvironment { get; } = new("Running environment", Label("Running environment"), 100);
        public Fields.Text RunningServer { get; } = new("Running server", Label("Running server"), 100);

        public ScheduledTask()
        {
            TableName = "Scheduled task";
            UnitCaption = Label("Scheduled task");
            TablePrimaryKey.Add(EntryNo);

            AddRelation<Session>(RunningSessionID);

            Deleting += ScheduledTask_Deleting;
        }

        private void ScheduledTask_Deleting()
        {
            switch (Status.Value)
            {
                case ScheduledTaskStatus.STARTING:
                case ScheduledTaskStatus.RUNNING:
                case ScheduledTaskStatus.STOPPING:
                    throw new Error(Label("Wait until {0} has been stopped", Description.Value));
            }
        }

        public void StartNow()
        {
            if ((Status.Value != ScheduledTaskStatus.ENABLED) && (Status.Value != ScheduledTaskStatus.DISABLED) &&
                (Status.Value != ScheduledTaskStatus.ERROR))
                throw new Error(Label("Status cannot be {0}", Status.Value));

            Status.Value = ScheduledTaskStatus.ENABLED;
            NextRunTime.Value = DateTime.MinValue;
            Modify();
        }

        public void SetDisabled()
        {
            switch (Status.Value)
            {
                case ScheduledTaskStatus.ENABLED:
                case ScheduledTaskStatus.ERROR:
                    Status.Value = ScheduledTaskStatus.DISABLED;
                    Modify();
                    break;

                case ScheduledTaskStatus.STARTING:
                case ScheduledTaskStatus.RUNNING:
                    Status.Value = ScheduledTaskStatus.STOPPING;
                    Modify();
                    break;
            }
        }

        public void SetEnabled(DateTime? nextRunTime = null)
        {
            if (Status.Value == ScheduledTaskStatus.STOPPING)
                throw new Error(Label("Wait until {0} has been stopped", Description.Value));

            if (Status.Value == ScheduledTaskStatus.STARTING)
                throw new Error(Label("Wait until {0} has been started", Description.Value));

            if (nextRunTime != null)
                NextRunTime.Value = nextRunTime.Value;

            if (NextRunTime.Value == DateTime.MinValue)
            {
                Status.Value = ScheduledTaskStatus.DISABLED;
                CurrentTry.Value = 0;
                Modify();

                return;
            }

            IntervalSec.Test();

            if (RunAlways.Value)
            {
                var dt2 = DateTime.Now;
                if (dt2 < NextRunTime.Value.AddSeconds(IntervalSec.Value))
                    dt2 = dt2.AddSeconds(IntervalSec.Value);

                NextRunTime.Value = dt2;
                Status.Value = ScheduledTaskStatus.ENABLED;
                CurrentTry.Value = 0;
                Modify();

                return;
            }

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

                if (dt < NextRunTime.Value.AddSeconds(IntervalSec.Value))
                    dt = dt.AddSeconds(IntervalSec.Value);

                if (dt.TimeOfDay > EndingTime.Value.TimeOfDay)
                {
                    dt = dt.AddDays(1).Date;
                    continue;
                }

                NextRunTime.Value = dt;
                Status.Value = ScheduledTaskStatus.ENABLED;
                CurrentTry.Value = 0;
                Modify();

                return;
            }

            throw new Error(Label("Unable to calculate next run time"));
        }
    }
}
