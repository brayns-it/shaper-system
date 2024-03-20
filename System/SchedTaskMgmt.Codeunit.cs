namespace Brayns.System
{
    public class SchedTaskMgmt : Codeunit
    {
        private static object _lockTasks = new();
        private static Dictionary<int, RunningTask> Tasks = new();

        public static void ApplicationInitialize()
        {
            ScheduledTask schedTask = new();
            schedTask.RunningEnvironment.SetRange(Shaper.Application.GetEnvironmentName());
            schedTask.RunningServer.SetRange(CurrentSession.Server);

            schedTask.Status.SetFilter("{0}|{1}", ScheduledTaskStatus.RUNNING, ScheduledTaskStatus.STARTING);
            schedTask.Status.ModifyAll(ScheduledTaskStatus.ERROR);

            schedTask.Status.SetRange(ScheduledTaskStatus.STOPPING);
            schedTask.Status.ModifyAll(ScheduledTaskStatus.DISABLED);
        }

        public static void RunNext()
        {
            var task = new ScheduledTask();
            task.TableLock = true;
            task.NextRunTime.SetFilter("<={0}", DateTime.Now);
            task.Status.SetRange(ScheduledTaskStatus.ENABLED);
            if (task.FindFirst())
            {
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
                        }
                    }
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
            while (ex.InnerException != null)
                ex = ex.InnerException;

            ScheduledTask task = new();
            task.Get((int)sender.Tag!);

            ClearTask(task);

            task.Status.Value = ScheduledTaskStatus.ERROR;
            task.Modify();

            var log = new ApplicationLog();
            log.Add(ApplicationLogType.ERROR, Label("Task {0} error {1}", task.Description.Value, ex.Message));

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
