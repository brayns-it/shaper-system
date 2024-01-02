using System.DirectoryServices.AccountManagement;

namespace Brayns.System
{
    public class AuthenticationManagement : Codeunit
    {
        public bool ValidateActiveDirectory(string username, string password, string domain)
        {
#pragma warning disable CA1416
            using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, domain))
            {
                return pc.ValidateCredentials(username, password);
            }
#pragma warning restore CA1416
        }

        public Error ErrorInvalidCredentials(string userid)
        {
            using (ApplicationLog log = new())
            {
                log.Connect();
                log.Add(ApplicationLogType.SECURITY, Label("User {0} failed login", userid));
            }

            return new Error(Label("Invalid user or password"));
        }

        public void TestUserPassword(User user, string password)
        {
            if (user.AuthenticationProvider.Value.Length == 0)
            {
                if (user.Password.Value == "plain:" + password)
                {
                    user.Password.Validate(password);
                    user.Modify();
                }
                else
                {
                    if (user.Password.Value != user.HashPassword(password))
                        throw ErrorInvalidCredentials(user.ID.Value);
                }
                return;
            }

            var provider = new AuthenticationProvider();
            provider.Get(user.AuthenticationProvider.Value);
            switch (provider.ProviderType.Value)
            {
                case AuthenticationProviderType.NONE:
                    throw ErrorInvalidCredentials(user.ID.Value);

                case AuthenticationProviderType.ACTIVE_DIRECTORY:
                    if (!ValidateActiveDirectory(user.ID.Value, password, provider.AdDomain.Value))
                        throw ErrorInvalidCredentials(user.ID.Value);
                    return;
            }
        }

        public User GetUserById(string userid, string password)
        {
            userid = userid.Trim();
            if (userid.Length == 0)
                throw new Error(Label("Invalid ID"));

            var user = new User();
            user.ID.SetRange(userid);
            user.Enabled.SetRange(true);
            if (!user.FindFirst())
                throw ErrorInvalidCredentials(userid);

            TestUserPassword(user, password);
            return user;
        }

        public User GetUserByEmail(string email, string password)
        {
            email = email.Trim();
            if (email.Length == 0)
                throw new Error(Label("Invalid e-mail"));

            var user = new User();
            user.ID.SetRange(email);
            user.Enabled.SetRange(true);
            if (user.Count() > 1)
                throw new Error(Label("Ambiguous e-mail '{0}'", email));
            if (!user.FindFirst())
                throw ErrorInvalidCredentials(email);

            TestUserPassword(user, password);
            return user;
        }
    }
}
