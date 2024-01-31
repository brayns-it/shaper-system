namespace Brayns.System
{
    public class SchedTaskMgmt : Codeunit
    {
        public static void RunNext()
        {
            var task = new ScheduledTask();
            task.TableLock = true;
            task.NextRunTime.SetFilter("<={0}", DateTime.Now);
            task.Status.SetRange(ScheduledTaskStatus.ENABLED);
            if (task.FindFirst())
            {
                task.Status.Value = ScheduledTaskStatus.RUNNING;
                task.Modify();
                Commit();

                new Thread(new ParameterizedThreadStart(RunTask)).Start(task.EntryNo.Value);
            }
        }

        private static void RunTask(object? o)
        {
            ScheduledTask task;
            try
            {
                CurrentSession.Start(new Shaper.SessionArgs()
                {
                    Id = Guid.NewGuid(),
                    Type = Shaper.SessionTypes.BATCH
                });
                CurrentSession.IsSuperuser = true;

                task = new();
                task.Get((int)o!);
            }
            catch
            {
                CurrentSession.Stop(true);
                return;
            }

            try
            {
                task.ObjectName.Test();
                task.MethodName.Test();

                var prx = Shaper.Loader.Proxy.CreateFromName(task.ObjectName.Value);
                prx.Invoke(task.MethodName.Value, new object[] { task.Parameter.Value });

                task.Refresh();
                task.SetEnabled();
            }
            catch (Exception ex)
            {
                Rollback();

                while (ex.InnerException != null)
                    ex = ex.InnerException;

                var log = new ApplicationLog();
                log.Add(ApplicationLogType.ERROR, Label("Task {0} error {1}", task.Description.Value, ex.Message));
                Commit();

                task.Refresh();
                task.Status.Value = ScheduledTaskStatus.ERROR;
                task.Modify();
            }

            Commit();
            CurrentSession.Stop(true);
        }
    }
}
