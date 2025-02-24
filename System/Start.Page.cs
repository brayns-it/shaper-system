namespace Brayns.System
{
    public partial class Start : Page<Start>
    {
        string _lastNotifications = "";
        Controls.Notifications? _notifications;

        protected override void Initialize()
        {
            UnitCaption = Label("Start");
            PageType = PageTypes.Start;
            
            var appCenter = Controls.AppCenter.Create(this);
            {
                Controls.Indicator.Create(appCenter);
                Controls.Search.Create(appCenter);
                
                _notifications = Controls.Notifications.Create(appCenter);

                var userCenter = Controls.UserCenter.Create(appCenter);
                {
                    var actLogout = new Controls.Action(userCenter, Label("Logout"), Icon.FromName("fas fa-sign-out-alt"));
                    actLogout.Triggering += ActionLogout_Triggering;
                }

                var navigationPane = Controls.NavigationPane.Create(appCenter);
                {
                    var grpAdmin = new Controls.ActionGroup(navigationPane, "group-admin", Label("Administration"));
                    {
                        var actGeneral = new Controls.Action(grpAdmin, "admin-general", Label("General"), Icon.FromName("fas fa-cog"));
                        {
                            new Controls.Action(actGeneral, Label("Information"), Icon.FromName("fas fa-info-circle"))
                            {
                                Run = typeof(InformationCard)
                            };

                            new Controls.Action(actGeneral, Label("Scheduled tasks"), Icon.FromName("fas fa-clock"))
                            {
                                Run = typeof(ScheduledTaskList)
                            };
                        }

                        var actAuth = new Controls.Action(grpAdmin, Label("Authentication"), Icon.FromName("fas fa-user-lock"));
                        {
                            new Controls.Action(actAuth, Label("Credentials"), Icon.FromName("fas fa-unlock"))
                            {
                                Run = typeof(CredentialList)
                            };

                            new Controls.Action(actAuth, Label("Devices"), Icon.FromName("fas fa-laptop"))
                            {
                                Run = typeof(DeviceList)
                            };

                            new Controls.Action(actAuth, Label("Roles"), Icon.FromName("fas fa-user-tag"))
                            {
                                Run = typeof(RoleList)
                            };

                            new Controls.Action(actAuth, Label("Sessions"), Icon.FromName("fas fa-list"))
                            {
                                Run = typeof(SessionList)
                            };

                            new Controls.Action(actAuth, Label("Providers"), Icon.FromName("fas fa-key"))
                            {
                                Run = typeof(AuthenticationProviderList)
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

                        var actSystem = new Controls.Action(grpAdmin, "admin-system", Label("System"), Icon.FromName("fas fa-microchip"));
                        {
                            new Controls.Action(actSystem, Label("Administration"), Icon.FromName("fas fa-tools"))
                            {
                                Run = typeof(Shaper.Systems.Admin)
                            };

                            new Controls.Action(actSystem, Label("Log"), Icon.FromName("fas fa-list"))
                            {
                                Run = typeof(ApplicationLogList)
                            };

                            new Controls.Action(actSystem, Label("Mail"), Icon.FromName("fas fa-envelope"))
                            {
                                Run = typeof(MailSetupList)
                            };

                            new Controls.Action(actSystem, Label("Print"), Icon.FromName("fas fa-print"))
                            {
                                Run = typeof(PrintSetupList)
                            };

                            new Controls.Action(actSystem, Label("Setup"), Icon.FromName("fas fa-wrench"))
                            {
                                Run = typeof(Shaper.Systems.Setup)
                            };
                        }
                    }
                }
            }

            Controls.Footer.Create(this);

            Loading += Start_Loading;
            UnitPolling += Start_UnitPolling;
            Loaded += Start_Loaded;
        }

        private void Start_Loaded()
        {
            var user = new User();
            user.Get(CurrentSession.UserId);
            if (user.StartPageName.Value.Length > 0)
            {
                var prx = Shaper.Loader.Proxy.CreateFromName(user.StartPageName.Value);
                prx.GetObject<BasePage>().Run();
            }
        }

        private void GetNotifications()
        {
            var notif = new Notification();
            notif.UserID.SetRange(CurrentSession.UserId);
            notif.IsRead.SetRange(false);
            notif.TableAscending = false;

            var currNotifs = "";
            List<Controls.NotificationItem> items = new();

            int i = 0;
            if (notif.FindSet())
                while ((i < 10) && notif.Read())
                {
                    Controls.NotificationItem item = new();
                    item.Tag = notif.EntryNo.Value;
                    item.Title = notif.Title.Value;
                    item.Description = notif.Description.Value;
                    item.DateTime = notif.CreationDateTime.Value;
                    item.Triggering += () =>
                    {
                        var notif2 = new Notification();
                        if (notif.Get((int)item.Tag))
                        {
                            notif.IsRead.Value = true;
                            notif.Modify();
                            Commit();
                            GetNotifications();
                        }
                    };
                    items.Add(item);
                    i++;

                    currNotifs += notif.EntryNo.Value.ToString() + ",";
                }

            // redraw only if necessary
            if (currNotifs != _lastNotifications)
            {
                _notifications!.Clear();
                foreach (var item in items)
                    item.Attach(_notifications);

                _notifications.Redraw();
                _lastNotifications = currNotifs;
            }
        }

        private void Start_UnitPolling()
        {
            GetNotifications();
        }

        protected override void AfterExtend()
        {
            Control("group-admin")!.MoveLast();
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

            if (ControlExists<Controls.NavigationPane>())
                Control<Controls.NavigationPane>()!.Caption = user.Name.Value;
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
