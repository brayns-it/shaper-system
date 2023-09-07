namespace Brayns.System.Module
{
    [Shaper.Classes.SystemModule]
    public class SystemModule : Shaper.Classes.SystemModule
    {
        Session session = new();

        public override void SessionStart()
        {
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
            session.ProcessID.Value = CurrentSession.ProcessID;
            session.ThreadID.Value = CurrentSession.ThreadID;
            session.DatabaseID.Value = CurrentSession.Database!.GetConnectionId();
            session.LastDateTime.Value = DateTime.Now;
            session.UserID.Value = CurrentSession.UserId;
            session.Active.Value = true;
            session.Modify();
        }

        public override void SessionStop()
        {
            if (session.Get(CurrentSession.Id))
            {
                session.Active.Value = false;
                session.Modify();
            }
        }

        public override void SessionDestroy()
        {
            if (session.Get(CurrentSession.Id))
                session.Delete();
        }

        public static void LoadInDomain()
        {
            // do nothing
        }
    }
}