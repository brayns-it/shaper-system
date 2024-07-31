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

            CleanupAuthentication();
        }

        private static void Session_Starting(bool sessionIsNew)
        {
            if (CurrentSession.Database == null) return;

            Session session = new() { TableLock = true };
            var authMgmt = new AuthenticationManagement();

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

                    CurrentSession.DatabaseDebug = session.DatabaseDebug.Value;

                    if ((CurrentSession.AuthenticationId != null) && (CurrentSession.AuthenticationId.Length > 0))
                        authMgmt.RefreshSessionToken(CurrentSession.AuthenticationId);
                }
                return;
            }

            if ((CurrentSession.AuthenticationId != null) && (CurrentSession.AuthenticationId.Length > 0))
            {
                if (!authMgmt.TryAuthenticateToken(CurrentSession.AuthenticationId))
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