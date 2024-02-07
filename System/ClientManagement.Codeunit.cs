namespace Brayns.System
{
    public class AccessTokenResponse
    {
        public string? access_token;
        public string? token_type;
        public int? expires_in;
    }

    [Published]
    public class ClientManagement : Codeunit
    {
        public bool RememberToken { get; set; }

        public AccessTokenResponse LoginByID(string userid, string password)
        {
            var authMgmt = new AuthenticationManagement();
            var user = authMgmt.GetUserById(userid, password);
            return AuthenticateUser(user);
        }

        public AccessTokenResponse LoginByEmail(string email, string password)
        {
            var authMgmt = new AuthenticationManagement();
            var user = authMgmt.GetUserByEmail(email, password);
            return AuthenticateUser(user);
        }

        public void Logout()
        {
            if (CurrentSession.AuthenticationId != null)
            {
                Authentication auth = new();
                if (auth.Get(CurrentSession.AuthenticationId!))
                    auth.Delete();
            }

            var session = new Session();
            if (session.Get(CurrentSession.Id))
                session.Delete();

            Shaper.Loader.Permissions.Clear();

            Client.ClearAuthenticationToken();

            CurrentSession.AuthenticationId = null;
            CurrentSession.UserId = "";
            CurrentSession.IsSuperuser = false;
        }

        public AccessTokenResponse AuthenticateUser(User user)
        {
            var session = new Session();
            if (!session.Get(CurrentSession.Id))
                throw session.ErrorNotFound();

            user.LastLogin.Value = DateTime.Now;
            user.Modify();

            session.UserID.Value = user.ID.Value;
            session.Modify();

            Authentication auth = new();
            auth.ID.Value = Guid.NewGuid();
            auth.CreationDateTime.Value = DateTime.Now;
            if (RememberToken)
                auth.ExpireDateTime.Value = DateTime.Now.AddDays(30);
            else
                auth.ExpireDateTime.Value = DateTime.Now.AddDays(1);
            auth.UserID.Value = user.ID.Value;
            auth.Insert();

            DateTimeOffset? exp = null;
            if (RememberToken)
                exp = DateTimeOffset.Now.AddDays(30);

            Client.SetAuthenticationToken(auth.ID.Value, exp);

            CurrentSession.AuthenticationId = auth.ID.Value;
            CurrentSession.UserId = user.ID.Value;
            CurrentSession.IsSuperuser = user.Superuser.Value;

            ApplicationLog log = new();
            log.Add(ApplicationLogType.INFORMATION, Label("User logged in"));

            return new()
            {
                access_token = auth.ID.Value.ToString("n"),
                token_type = "bearer",
                expires_in = Convert.ToInt32(auth.ExpireDateTime.Value.Subtract(DateTime.Now).TotalSeconds)
            };
        }
    }
}
