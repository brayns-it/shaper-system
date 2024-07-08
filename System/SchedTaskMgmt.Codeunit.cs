using System.Text;

namespace Brayns.System
{
    public class SchedTaskMgmt : Codeunit
    {
        private static int _lastEntryNo = 0;
        private static object _lockTasks = new();
        private static Dictionary<int, RunningTask> Tasks = new();

        public static void ApplicationInitialize()
        {
            var log = new ApplicationLog();

            ScheduledTask schedTask = new();
            schedTask.RunningEnvironment.SetRange(Shaper.Application.GetEnvironmentName());
            schedTask.RunningServer.SetRange(CurrentSession.Server);
            if (schedTask.FindSet())
                while (schedTask.Read())
                {
                    switch (schedTask.Status.Value)
                    {
                        case ScheduledTaskStatus.RUNNING:
                        case ScheduledTaskStatus.STARTING:
                            log.Add(ApplicationLogType.WARNING, Label("Task {0} was running, setting in error", schedTask.Description.Value));

                            schedTask.Status.Value = ScheduledTaskStatus.ERROR;
                            schedTask.Modify();
                            break;

                        case ScheduledTaskStatus.STOPPING:
                            schedTask.Status.Value = ScheduledTaskStatus.DISABLED;
                            schedTask.Modify();
                            break;
                    }
                }
        }

        public static void RunNext()
        {
            var log = new ApplicationLog();

            var task = new ScheduledTask();
            task.NextRunTime.SetFilter("<={0}", DateTime.Now);
            task.Status.SetRange(ScheduledTaskStatus.ENABLED);
            
            // avoid stuck on always running processes
            if (_lastEntryNo > 0) 
            {
                task.EntryNo.SetFilter(">{0}", _lastEntryNo);
                if (task.IsEmpty())
                {
                    _lastEntryNo = 0;
                    task.EntryNo.SetRange();
                }
            }

            if (task.FindFirst())
            {
                _lastEntryNo = task.EntryNo.Value;

                task.Status.Value = ScheduledTaskStatus.STARTING;
                task.RunningServer.Value = CurrentSession.Server;
                task.RunningEnvironment.Value = Shaper.Application.GetEnvironmentName();
                task.Modify();
                Commit();

                RunningTask rt = new();
                rt.Tag = task.EntryNo.Value;
                rt.TypeName = task.ObjectName.Value;
                rt.MethodName = task.MethodName.Value;
                rt.Parameters = task.Parameter.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                rt.Starting += Task_Starting;
                rt.Error += Task_Error;
                rt.Finishing += Task_Finishing;

                lock (_lockTasks)
                    Tasks.Add(task.EntryNo.Value, rt);

                rt.Start();
            }

            // stop request
            task.Reset();
            task.Status.SetRange(ScheduledTaskStatus.STOPPING);
            task.RunningEnvironment.SetRange(Shaper.Application.GetEnvironmentName());
            task.RunningServer.SetRange(CurrentSession.Server);
            if (task.FindSet())
                while (task.Read())
                {
                    lock (_lockTasks)
                    {
                        if (Tasks.ContainsKey(task.EntryNo.Value))
                            Tasks[task.EntryNo.Value].Stop();
                        else
                        {
                            ClearTask(task);
                            task.Status.Value = ScheduledTaskStatus.DISABLED;
                            task.Modify();
                            Commit();
                        }
                    }
                }

            // is alive?
            lock (_lockTasks)
            {
                List<int> toDel = new();
                foreach (var t in Tasks.Keys)
                {
                    if (!Tasks[t].IsAlive)
                    {
                        task.Get(t);
                        task.Status.Value = ScheduledTaskStatus.ERROR;
                        task.Modify();

                        log.Add(ApplicationLogType.WARNING, Label("Task {0} unhandled error", task.Description.Value));

                        Commit();

                        toDel.Add(t);
                    }
                }
                foreach (var t in toDel)
                    Tasks.Remove(t);
            }
        }

        private static void RemoveFromList(ScheduledTask task)
        {
            lock (_lockTasks)
            {
                if (Tasks.ContainsKey(task.EntryNo.Value))
                    Tasks.Remove(task.EntryNo.Value);
            }
        }

        private static void ClearTask(ScheduledTask task)
        {
            task.RunningServer.Value = "";
            task.RunningEnvironment.Value = "";
            task.RunningSessionID.Value = Guid.Empty;
        }

        private static void Task_Finishing(RunningTask sender)
        {
            ScheduledTask task = new();
            task.Get((int)sender.Tag!);

            ClearTask(task);

            task.SetEnabled();
            task.Modify();

            RemoveFromList(task);
        }

        private static void Task_Error(RunningTask sender, Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ex.StackTrace ?? "");
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                sb.Append(ex.StackTrace ?? "");
            }

            ScheduledTask task = new();
            task.Get((int)sender.Tag!);

            ClearTask(task);

            task.Status.Value = ScheduledTaskStatus.ERROR;
            task.Modify();

            var log = new ApplicationLog();
            log.Add(ApplicationLogType.ERROR, Label("Task {0} error {1}", task.Description.Value, ex.Message), sb.ToString());

            RemoveFromList(task);
        }

        private static void Task_Starting(RunningTask sender)
        {
            ScheduledTask task = new();
            task.Get((int)sender.Tag!);
            task.RunningSessionID.Value = CurrentSession.Id;
            task.Status.Value = ScheduledTaskStatus.RUNNING;
            task.Modify();
        }
    }
}
