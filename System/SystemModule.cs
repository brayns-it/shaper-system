namespace Brayns.System
{
    [Shaper.Classes.SystemModule]
    public class SystemModule : Shaper.Classes.SystemModule
    {
        Session session = new() { TableLock = true };
        Authentication authentication = new();
        User user = new();

        public override void SessionStart()
        {
            if (CurrentSession.Database == null) return;

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

                authentication.Reset();
                authentication.ID.SetRange(CurrentSession.AuthenticationId);
                if (authentication.FindFirst())
                    if (user.Get(authentication.UserID.Value))
                    {
                        CurrentSession.UserId = user.ID.Value;
                        invalid = false;
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

        public override void SessionStop()
        {
            if (CurrentSession.Database == null) return;

            if (session.Get(CurrentSession.Id))
            {
                session.Active.Value = false;
                session.Modify();
            }
        }

        public override void SessionDestroy()
        {
            if (CurrentSession.Database == null) return;

            if (session.Get(CurrentSession.Id))
                session.Delete();
        }

        public override void ApplicationStart()
        {
            if (CurrentSession.Database == null) return;

            session.Reset();
            session.Environment.SetRange(Shaper.Application.GetEnvironmentName());
            session.Server.SetRange(CurrentSession.Server);
            session.DeleteAll();

            CleanupAuthentication();
        }

        public void CleanupAuthentication()
        {
            authentication.Reset();
            authentication.ExpireDateTime.SetFilter("<{0}", DateTime.Now);
            authentication.DeleteAll();
            Commit();
        }
    }
}