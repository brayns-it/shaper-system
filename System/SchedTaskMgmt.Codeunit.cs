using System.Text;
using System.Net.Mail;

namespace Brayns.System
{
    public delegate void SchedTaskNotifyHandler(MailMessage msg);
    public delegate void SchedTaskSelectingHandler(ScheduledTask task);

    public class SchedTaskMgmt : Codeunit
    {
        private static int _lastEntryNo = 0;
        private static object _lockTasks = new();
        private static Dictionary<int, RunningTask> Tasks = new();

        public static event SchedTaskSelectingHandler? TaskSelecting;

        public static void ApplicationInitialize()
        {
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
                                ApplicationLog.Add(ApplicationLogType.WARNING, Label("Task {0} was running, next run time {1}", schedTask.Description.Value, schedTask.NextRunTime.Value));
                            }
                            catch
                            {
                                schedTask.Status.Value = ScheduledTaskStatus.ERROR;
                                schedTask.RunOnce.Value = false;
                                schedTask.Modify();

                                ApplicationLog.Add(ApplicationLogType.WARNING, Label("Task {0} was running, setting in error", schedTask.Description.Value));
                            }
                            break;

                        case ScheduledTaskStatus.STOPPING:
                            schedTask.Status.Value = ScheduledTaskStatus.DISABLED;
                            schedTask.RunOnce.Value = false;
                            schedTask.Modify();
                            break;
                    }
                }
        }

        public static void RunNext()
        {
            var task = new ScheduledTask();
            task.NextRunTime.SetFilter("<={0}", DateTime.Now);
            task.Status.SetRange(ScheduledTaskStatus.ENABLED);
            TaskSelecting?.Invoke(task);

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
                            task.RunOnce.Value = false;
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
            ScheduledTask task = new();
            task.Get((int)sender.Tag!);

            ClearTask(task);
            RemoveFromList(task);

            if ((task.MaximumRetries.Value > 0) && (task.CurrentTry.Value < task.MaximumRetries.Value) && 
                (task.RetrySec.Value > 0) && (task.NextRunTime.Value > DateTime.MinValue) && (!task.RunOnce.Value))
            {
                task.Status.Value = ScheduledTaskStatus.ENABLED;
                task.NextRunTime.Value = DateTime.Now.AddSeconds(task.RetrySec.Value);
                task.Modify();

                ApplicationLog.Add(ApplicationLogType.WARNING, ex, Label("Task {0} retry {1}", task.Description.Value, task.CurrentTry.Value));
            }
            else
            {
                task.Status.Value = ScheduledTaskStatus.ERROR;
                task.RunOnce.Value = false;
                task.Modify();

                ApplicationLog.Add(ex, Label("Task {0}", task.Description.Value));

                Commit();
                TryNotifyError(task, ex);
            }
        }

        public static void TryNotifyError(SchedTaskNotifyHandler? onMessage)
        {
            TryNotifyError(new Exception(), onMessage);
        }

        public static void TryNotifyError(Exception ex, SchedTaskNotifyHandler? onMessage)
        {
            try
            {
                Brayns.Shaper.Classes.FormattedException fe = new Shaper.Classes.FormattedException(ex);

                var schedSetup = new ScheduledTaskSetup();
                schedSetup.Get();
                if (!schedSetup.NotifyIfError.Value) return;

                var mailMgmt = new MailMgmt();
                if (schedSetup.MailProfile.Value.Length > 0)
                    mailMgmt.SetProfile(schedSetup.MailProfile.Value);

                if (schedSetup.MailSenderAddress.Value.Length > 0)
                    mailMgmt.Message.From = new MailAddress(schedSetup.MailSenderAddress.Value);

                mailMgmt.Message.To.Add(new MailAddress(schedSetup.MailRecipientAddress.Value));
                mailMgmt.Message.Subject = Label("Error in {0}", CurrentSession.ApplicationName);
                mailMgmt.Message.IsBodyHtml = true;
                mailMgmt.Message.Body = "<p><b>" + fe.Message + "</b></p><pre>" + string.Join("\r\n", fe.Trace.ToArray()) + "</pre>";

                onMessage?.Invoke(mailMgmt.Message);

                mailMgmt.Send();
            }
            catch
            {
                // do nothing
            }
        }

        private static void TryNotifyError(ScheduledTask task, Exception ex)
        {
            TryNotifyError(ex, (msg) => msg.Subject = Label("Task {0} error, {1}", task.Description.Value, CurrentSession.ApplicationName));
        }

        private static void Task_Starting(RunningTask sender)
        {
            ScheduledTask task = new();
            task.Get((int)sender.Tag!);
            task.RunningSessionID.Value = CurrentSession.Id;
            task.Status.Value = ScheduledTaskStatus.RUNNING;
            task.LastRunTime.Value = DateTime.Now;
            task.Modify();
        }
    }
}
