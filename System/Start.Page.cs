namespace Brayns.System
{
    public class Start : Page<Start>
    {
        protected Controls.AppCenter AppCenter { get; init; }
        protected Controls.Indicator Indicator { get; init; }
        protected Controls.Notifications Notifications { get; init; }
        protected Controls.Search Search { get; init; }
        protected Controls.UserCenter UserCenter { get; init; }
        protected Controls.Action ActionLogout { get; init; }

        public Start()
        {
            UnitName = "Start";
            UnitCaption = Label("Start");
            PageType = PageTypes.Start;

            AppCenter = new(this);
            {
                Indicator = new(AppCenter);
                Search = new(AppCenter);
                
                Notifications = new(AppCenter);
                Notifications.Getting += Notifications_Getting;
                Notifications.Triggering += Notifications_Triggering;

                UserCenter = new(AppCenter);
                {
                    ActionLogout = new(UserCenter, Label("Logout"), "fas fa-sign-out-alt");
                    ActionLogout.Triggering += ActionLogout_Triggering;
                }
            }
        }

        private void Notifications_Triggering(string notificationID)
        {
            var notif = new Notification();
            if (notif.Get(int.Parse(notificationID)))
            {
                notif.IsRead.Value = true;
                notif.Modify();
            }
        }

        private void Notifications_Getting(List<Controls.NotificationItem> items)
        {
            var notif = new Notification();
            notif.UserID.SetRange(CurrentSession.UserId);
            notif.IsRead.SetRange(false);
            notif.TableAscending = false;

            int i = 0;
            if (notif.FindSet())
                while ((i < 10) && notif.Read())
                {
                    Controls.NotificationItem item = new();
                    item.ID = notif.EntryNo.Value.ToString();
                    item.Title = notif.Title.Value;
                    item.Description = notif.Description.Value;
                    item.DateTime = notif.CreationDateTime.Value;
                    items.Add(item);
                    i++;
                }
        }

        private void ActionLogout_Triggering()
        {
            new Confirm(Label("Logout from {0}?", CurrentSession.ApplicationName), () =>
            {
                var clMgmt = new ClientManagement();
                clMgmt.Logout();
                Client.Reload();
            }).RunModal();
        }

        protected override void OnLoad()
        {
            var nfo = new Information();
            nfo.Get();
            Indicator.Caption = nfo.Indicator.Value;

            var user = new User();
            user.Get(CurrentSession.UserId);
            UserCenter.Caption = user.Name.Value;
        }
    }
}
