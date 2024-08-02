using System.Text;
using System.Net.Mail;

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
                            schedTask.Status.Value = ScheduledTaskStatus.ENABLED;
                            schedTask.Modify();

                            try
                            {
                                schedTask.SetEnabled();
                                log.Add(ApplicationLogType.WARNING, Label("Task {0} was running, next run time {1}", schedTask.Description.Value, schedTask.NextRunTime.Value));
                            }
                            catch
                            {
                                schedTask.Status.Value = ScheduledTaskStatus.ERROR;
                                schedTask.Modify();

                                log.Add(ApplicationLogType.WARNING, Label("Task {0} was running, setting in error", schedTask.Description.Value));
                            }
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
                task.CurrentTry.Value++;
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
            RemoveFromList(task);

            task.SetEnabled();
            task.Modify();
        }

        private static void Task_Error(RunningTask sender, Exception ex)
        {
            Exception startEx = ex;

            StringBuilder sb = new StringBuilder();
            sb.Append(ex.StackTrace ?? "");
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                sb.Append(ex.StackTrace ?? "");
            }

            var log = new ApplicationLog();

            ScheduledTask task = new();
            task.Get((int)sender.Tag!);

            ClearTask(task);
            RemoveFromList(task);

            if ((task.MaximumRetries.Value > 0) && (task.CurrentTry.Value < task.MaximumRetries.Value) && (task.RetrySec.Value > 0))
            {
                task.Status.Value = ScheduledTaskStatus.ENABLED;
                task.NextRunTime.Value = DateTime.Now.AddSeconds(task.RetrySec.Value);
                task.Modify();

                log.Add(ApplicationLogType.WARNING, Label("Task {0} error {1}, retry {2}", task.Description.Value, ex.Message, task.CurrentTry.Value), sb.ToString());
            }
            else
            {
                task.Status.Value = ScheduledTaskStatus.ERROR;
                task.Modify();

                log.Add(ApplicationLogType.ERROR, Label("Task {0} error {1}", task.Description.Value, ex.Message), sb.ToString());

                Commit();
                TryNotifyError(task, startEx);
            }
        }

        private static void TryNotifyError(ScheduledTask task, Exception ex)
        {
            try
            {
                var schedSetup = new ScheduledTaskSetup();
                schedSetup.Get();
                if (!schedSetup.NotifyIfError.Value) return;

                var mailMgmt = new MailMgmt();
                if (schedSetup.MailProfile.Value.Length > 0)
                    mailMgmt.SetProfile(schedSetup.MailProfile.Value);

                var msg = new MailMessage();
                if (schedSetup.MailSenderAddress.Value.Length > 0)
                    msg.From = new MailAddress(schedSetup.MailSenderAddress.Value);

                msg.To.Add(new MailAddress(schedSetup.MailRecipientAddress.Value));
                msg.Subject = Label("Task {0} error, {1}", task.Description.Value, CurrentSession.ApplicationName);
                msg.IsBodyHtml = true;

                var stack = new StringBuilder();
                if (ex.StackTrace != null) stack.AppendLine(ex.StackTrace);
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    if (ex.StackTrace != null) stack.AppendLine(ex.StackTrace);
                }

                msg.Body = "<p><b>" + ex.Message + "</b></p><pre>" + stack.ToString() + "</pre>";

                mailMgmt.Send(msg);
            }
            catch
            {
                // do nothing
            }
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
