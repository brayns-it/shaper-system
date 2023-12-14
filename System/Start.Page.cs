namespace Brayns.System
{
    public partial class Start : Page<Start>
    {
        public Start()
        {
            UnitCaption = Label("Start");
            PageType = PageTypes.Start;

            var appCenter = Controls.AppCenter.Create(this);
            {
                Controls.Indicator.Create(appCenter);
                Controls.Search.Create(appCenter);
                
                var notifications = Controls.Notifications.Create(appCenter);
                notifications.Getting += Notifications_Getting;
                notifications.Triggering += Notifications_Triggering;

                var userCenter = Controls.UserCenter.Create(appCenter);
                {
                    var actLogout = new Controls.Action(userCenter, Label("Logout"), Icon.FromName("fas fa-sign-out-alt"));
                    actLogout.Triggering += ActionLogout_Triggering;
                }

                var navigationPane = Controls.NavigationPane.Create(appCenter);
                {
                    var grpAdmin = new Controls.ActionGroup(navigationPane, Label("Administration"));
                    {
                        var actGeneral = new Controls.Action(grpAdmin, Label("General"), Icon.FromName("fas fa-cog"));
                        {
                            new Controls.Action(actGeneral, Label("Information"), Icon.FromName("fas fa-info-circle"))
                            {
                                Run = typeof(InformationCard)
                            };
                        }

                        var actAuth = new Controls.Action(grpAdmin, Label("Authentication"), Icon.FromName("fas fa-user-lock"));
                        {
                            new Controls.Action(actAuth, Label("Roles"), Icon.FromName("fas fa-user-tag"))
                            {
                                Run = typeof(RoleList)
                            };

                            new Controls.Action(actAuth, Label("Sessions"), Icon.FromName("fas fa-list"))
                            {
                                Run = typeof(SessionList)
                            };

                            new Controls.Action(actAuth, Label("Tokens"), Icon.FromName("fas fa-ticket-alt"))
                            {
                                Run = typeof(TokenList)
                            };

                            new Controls.Action(actAuth, Label("Users"), Icon.FromName("fas fa-user"))
                            {
                                Run = typeof(UserList)
                            };
                        }

                        var actSystem = new Controls.Action(grpAdmin, Label("System"), Icon.FromName("fas fa-microchip"));
                        {
                            new Controls.Action(actSystem, Label("Administration"), Icon.FromName("fas fa-tools"))
                            {
                                Run = typeof(Shaper.Systems.Admin)
                            };
                        }
                    }
                }
            }

            Controls.Footer.Create(this);

            Loading += Start_Loading;
            Extend();
        }

        private void Start_Loading()
        {
            var nfo = new Information();
            nfo.Get();

            if (ControlExists<Controls.Indicator>())
                Control<Controls.Indicator>()!.Caption = nfo.Indicator.Value;

            if (ControlExists<Controls.Footer>())
                Control<Controls.Footer>()!.Caption = nfo.GetFooter();

            var user = new User();
            user.Get(CurrentSession.UserId);

            if (ControlExists<Controls.UserCenter>())
                Control<Controls.UserCenter>()!.Caption = user.Name.Value;
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
    }
}
