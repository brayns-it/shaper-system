namespace Brayns.System
{
    [Published]
    public class ClientManagement : Codeunit
    {
        public bool RememberToken { get; set; }

        static ClientManagement()
        {
            Brayns.Shaper.Systems.ClientManagement.ClientInitializing += ClientManagement_Initializing;
        }

        private static void ClientManagement_Initializing(Shaper.Systems.ClientManagement sender)
        {
            if (CurrentSession.UserId.Length == 0)
            {
                var login = new Login();
                login.Run();
            }
            else
            {
                var start = new Start();
                start.Run();
            }
        }

        public class AccessTokenResponse
        {
            public string? access_token;
            public string? token_type;
            public int? expires_in;
        }

        [PublicAccess]
        public AccessTokenResponse LoginByID(string userid, string password)
        {
            var user = new User();
            user.ID.SetRange(userid);
            user.Password.SetRange(user.HashPassword(password));
            user.Enabled.SetRange(true);

            if (!user.FindFirst())
            {
                user.Password.SetRange("plain:" + password);
                if (!user.FindFirst())
                {
                    using (ApplicationLog log = new())
                    {
                        log.Connect();
                        log.Add(ApplicationLogType.SECURITY, Label("User ID {0} failed login", userid));
                    }

                    throw new Error(Label("Invalid ID or password"));
                }
                else
                {
                    user.Password.Validate(password);
                    user.Modify();
                }
            }

            return AuthenticateUser(user);
        }

        [PublicAccess]
        public AccessTokenResponse LoginByEmail(string email, string password)
        {
            email = email.Trim();
            if (email.Length == 0)
                throw new Error(Label("Invalid e-mail"));

            var user = new User();
            user.EMail.SetRange(email);
            user.Enabled.SetRange(true);
            if (user.Count() > 1)
                throw new Error(Label("Ambiguous e-mail '{0}'", email));
            user.Password.SetRange(user.HashPassword(password));

            if (!user.FindFirst())
            {
                user.Password.SetRange("plain:" + password);
                if (!user.FindFirst())
                {
                    using (ApplicationLog log = new())
                    {
                        log.Connect();
                        log.Add(ApplicationLogType.SECURITY, Label("User e-mail {0} failed login", email));
                    }

                    throw new Error(Label("Invalid e-mail or password"));
                }
                else
                {
                    user.Password.Validate(password);
                    user.Modify();
                }
            }

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

        private AccessTokenResponse AuthenticateUser(User user)
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

            using (ApplicationLog log = new())
            {
                log.Connect();
                log.Add(ApplicationLogType.INFORMATION, Label("User logged in"));
            }

            return new()
            {
                access_token = auth.ID.Value.ToString("n"),
                token_type = "bearer",
                expires_in = Convert.ToInt32(auth.ExpireDateTime.Value.Subtract(DateTime.Now).TotalSeconds)
            };
        }
    }
}
