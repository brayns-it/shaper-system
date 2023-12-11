namespace Brayns.System
{
    public class SystemManagement : Codeunit
    {
        static SystemManagement()
        {
            Shaper.Application.Initializing += Application_Initializing;
            Shaper.Session.Starting += Session_Starting;
            Shaper.Session.Stopping += Session_Stopping;
            Shaper.Session.Destroying += Session_Destroying;
        }

        private static void Session_Starting()
        {
            if (CurrentSession.Database == null) return;

            Session session = new() { TableLock = true };

            if (!CurrentSession.IsNew)
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

            CleanupAuthentication();

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
                        CurrentSession.UserId = user.ID.Value;
                        invalid = false;
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

        private static void Session_Destroying()
        {
            if (CurrentSession.Database == null) return;

            Session session = new() { TableLock = true };
            if (session.Get(CurrentSession.Id))
                session.Delete();
        }

        private static void Application_Initializing()
        {
            if (CurrentSession.Database == null) return;

            Session session = new();
            session.Environment.SetRange(Shaper.Application.GetEnvironmentName());
            session.Server.SetRange(CurrentSession.Server);
            session.DeleteAll();

            CleanupAuthentication();
        }

        public static void CleanupAuthentication()
        {
            Authentication authentication = new();
            authentication.ExpireDateTime.SetFilter("<{0}", DateTime.Now);
            authentication.DeleteAll();
            Commit();
        }
    }
}