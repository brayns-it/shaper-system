namespace Brayns.System
{
    public class ClientManagement : Codeunit
    {
        public class LoginResult
        {
            public string? access_token;
            public string? token_type;
        }

        public class InitializeResult
        {
            public string? label_signin;
            public string? label_password;
            public string? label_signin_button;
            public string? label_pwdlost;
            public string? label_login;
            public string? label_error;
            public string? label_search;
            public string? label_neterror;
            public string? label_ok;
            public string? label_email;
            public string? name;
            public string? description;
            public string? footer;
            public string? indicator;
            public bool? authenticated;
            public string? startpage;
        }

        [PublicAccess]
        public InitializeResult Initialize()
        {
            var result = new InitializeResult();

            var nfo = new Information();
            if (nfo.Get())
            {
                result.name = nfo.Name.Value;
                result.description = nfo.Description.Value;
                result.footer = nfo.Footer.Value;
                result.indicator = nfo.Indicator.Value;
            }

            result.authenticated = CurrentSession.UserId.Length > 0;
            result.label_signin = Label("Sign in to start your session");
            result.label_email = Label("E-Mail");
            result.label_password = Label("Password");
            result.label_signin_button = Label("Sign In");
            result.label_pwdlost = Label("I forgot my password");
            result.label_login = Label("Login");
            result.label_error = Label("Error");
            result.label_ok = Label("Ok");
            result.label_search = Label("Search");
            result.label_neterror = Label("Network error: try again later.");
            return result;
        }

        [PublicAccess]
        public LoginResult LoginByID(string userid, string password)
        {
            var user = new User();
            user.ID.SetRange(userid);
            user.Password.SetRange(user.HashPassword(password));
            if (!user.FindFirst())
                throw new Error(Label("Invalid ID or password"));

            return AuthenticateSession(user);
        }

        [PublicAccess]
        public LoginResult LoginByEmail(string email, string password)
        {
            email = email.Trim();
            if (email.Length == 0)
                throw new Error(Label("Invalid e-mail"));

            var user = new User();
            user.EMail.SetRange(email);
            if (user.Count() > 1)
                throw new Error(Label("Ambiguous e-mail '{0}'"), email);
            user.Password.SetRange(user.HashPassword(password));
            if (!user.FindFirst())
                throw new Error(Label("Invalid e-mail or password"));

            return AuthenticateSession(user);
        }

        private LoginResult AuthenticateSession(User user)
        {
            var session = new Session();
            if (!session.Get(CurrentSession.Id))
                throw session.ErrorNotFound;

            session.UserID.Value = user.ID.Value;
            session.Modify();

            CurrentSession.UserId = user.ID.Value;

            return new()
            {
                access_token = CurrentSession.Id.ToString("n"),
                token_type = "bearer"
            };
        }
    }
}
