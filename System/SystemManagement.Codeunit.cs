namespace Brayns.System
{
    public class SystemManagement : Codeunit
    {
        private static bool _isInitializated = false;

        static SystemManagement()
        {
            Shaper.Application.Initializing += Application_Initializing;
            Shaper.Application.Monitoring += Application_Monitoring;
            Shaper.Session.Starting += Session_Starting;
            Shaper.Session.Stopping += Session_Stopping;
            Shaper.Session.Destroying += Session_Destroying;
            Shaper.Systems.ClientManagement.RunningLogin += ClientManagement_RunningLogin;
            Shaper.Systems.ClientManagement.RunningStart += ClientManagement_RunningStart;
        }

        private static void ClientManagement_RunningStart(Shaper.Systems.ClientManagement sender)
        {
            var start = new Start();
            start.Run();
        }

        private static void ClientManagement_RunningLogin(Shaper.Systems.ClientManagement sender)
        {
            var login = new Login();
            login.Run();
        }

        private static void Application_Monitoring()
        {
            SchedTaskMgmt.RunNext();
        }

        private static void Session_Starting(bool sessionIsNew)
        {
            if (CurrentSession.Database == null) return;

            Session session = new() { TableLock = true };

            if (!sessionIsNew)
            {
                if (session.Get(CurrentSession.Id))
                {
                    session.ProcessID.Value = CurrentSession.ProcessID;
                    session.ThreadID.Value = CurrentSession.ThreadID;
                    if (CurrentSession.Database != null)
                        session.DatabaseID.Value = CurrentSession.Database.GetConnectionId();
                    session.LastDateTime.Value = DateTime.Now;
                    session.Active.Value = true;
                    session.Modify();
                }
                return;
            }

            if ((CurrentSession.AuthenticationId != null) && (CurrentSession.AuthenticationId != Guid.Empty))
            {
                bool invalid = true;

                Authentication authentication = new();
                authentication.ID.SetRange(CurrentSession.AuthenticationId);
                if (authentication.FindFirst())
                {
                    User user = new();
                    if (user.Get(authentication.UserID.Value))
                    {
                        user.LastLogin.Value = DateTime.Now;
                        user.Modify();

                        CurrentSession.UserId = user.ID.Value;
                        CurrentSession.IsSuperuser = user.Superuser.Value;
                        invalid = false;

                        if (CurrentSession.Type == Shaper.SessionTypes.WEBCLIENT)
                        {
                            ApplicationLog log = new();
                            log.Add(ApplicationLogType.INFORMATION, Label("User logged in (token)"));
                        }
                    }
                }

                if (invalid)
                    Client.ClearAuthenticationToken();
            }

            if (!session.Get(CurrentSession.Id))
            {
                session.Init();
                session.ID.Value = CurrentSession.Id;
                session.CreationDateTime.Value = DateTime.Now;
                session.Insert();
            }
            session.Type.Value = CurrentSession.Type!;
            session.Address.Value = CurrentSession.Address;
            session.Server.Value = CurrentSession.Server;
            session.Environment.Value = Shaper.Application.GetEnvironmentName();
            session.ProcessID.Value = CurrentSession.ProcessID;
            session.ThreadID.Value = CurrentSession.ThreadID;
            if (CurrentSession.Database != null)
                session.DatabaseID.Value = CurrentSession.Database.GetConnectionId();
            session.LastDateTime.Value = DateTime.Now;
            session.UserID.Value = CurrentSession.UserId;
            session.Active.Value = true;
            session.Modify();

            var nfo = new Information();
            nfo.Get();
            CurrentSession.ApplicationName = nfo.Name.Value;

            if ((CurrentSession.UserId.Length > 0) && (!Shaper.Loader.Permissions.Exists()))
                LoadPermissions();
        }

        private static void LoadPermissions()
        {
            List<Shaper.Loader.Permission> perms = new();

            var userRole = new UserRole();
            userRole.UserID.SetRange(CurrentSession.UserId);
            if (userRole.FindSet())
                while (userRole.Read())
                {
                    var roleDet = new RoleDetail();
                    roleDet.RoleCode.SetRange(userRole.RoleCode.Value);
                    if (roleDet.FindSet())
                        while (roleDet.Read())
                        {
                            switch (roleDet.Execution.Value)
                            {
                                case RolePermission.ALLOWED:
                                    perms.Add(new(roleDet.ObjectType.Value, roleDet.ObjectName.Value, Shaper.Loader.PermissionType.Execute, Shaper.Loader.PermissionMode.Allow));
                                    break;
                                case RolePermission.ALLOWED_INDIRECT:
                                    perms.Add(new(roleDet.ObjectType.Value, roleDet.ObjectName.Value, Shaper.Loader.PermissionType.Execute, Shaper.Loader.PermissionMode.AllowIndirect));
                                    break;
                                case RolePermission.DENIED:
                                    perms.Add(new(roleDet.ObjectType.Value, roleDet.ObjectName.Value, Shaper.Loader.PermissionType.Execute, Shaper.Loader.PermissionMode.Deny));
                                    break;
                            }
                        }
                }

            Shaper.Loader.Permissions.Set(perms);
        }

        private static void Session_Stopping()
        {
            if (CurrentSession.Database == null) return;

            Session session = new() { TableLock = true };
            if (session.Get(CurrentSession.Id))
            {
                session.Active.Value = false;
                session.Modify();
            }
        }

        private static void Session_Destroying(Guid sessionId)
        {
            if (CurrentSession.Database == null) return;

            Session session = new() { TableLock = true };
            if (session.Get(sessionId))
                session.Delete();
        }

        private static void Application_Initializing()
        {
            if (_isInitializated) return;
            if (CurrentSession.Database == null) return;

            var log = new ApplicationLog();
            log.Add(ApplicationLogType.INFORMATION, Label("Environment {0} starting on {1}", Shaper.Application.GetEnvironmentName(), CurrentSession.Server));

            Session session = new();
            session.Environment.SetRange(Shaper.Application.GetEnvironmentName());
            session.Server.SetRange(CurrentSession.Server);
            session.DeleteAll();

            SchedTaskMgmt.ApplicationInitialize();

            CleanupAuthentication();
            _isInitializated = true;
        }

        private static void CleanupAuthentication()
        {
            Authentication authentication = new();
            authentication.ExpireDateTime.SetFilter("<{0}", DateTime.Now);
            authentication.DeleteAll();
            Commit();
        }
    }
}