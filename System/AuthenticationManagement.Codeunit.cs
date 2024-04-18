using System.DirectoryServices.Protocols;
using System.Net;

namespace Brayns.System
{
    public class AccessTokenResponse
    {
        public string access_token = "";
        public string token_type = "";
        public int expires_in = 0;
    }

    public enum AccessTokenFormat
    {
        Guid,
        DoubleGuid,
        Sha256
    }

    public class AuthenticationManagement : Codeunit
    {
        public bool ValidateActiveDirectory(string username, string password, string serverName)
        {
            LdapDirectoryIdentifier identifier = new LdapDirectoryIdentifier(serverName, 636);
            NetworkCredential creds = new NetworkCredential(username, password);
            LdapConnection connection = new LdapConnection(identifier)
            {
                AuthType = AuthType.Basic,
                SessionOptions =
                {
                    ProtocolVersion = 3,
                    SecureSocketLayer = true
                }
            };
            connection.SessionOptions.VerifyServerCertificate += (s, c) => true;
            try
            {
                connection.Bind(creds);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Error ErrorInvalidCredentials(string userid)
        {
            ApplicationLog log = new();
            log.Add(ApplicationLogType.SECURITY, Label("User {0} failed login", userid));
            Commit();

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

            if (provider.ProviderType.Value != AuthenticationProviderType.NONE)
                if (user.AuthenticationID.Value.Length == 0)
                    throw ErrorInvalidCredentials(user.ID.Value);

            switch (provider.ProviderType.Value)
            {
                case AuthenticationProviderType.NONE:
                    throw ErrorInvalidCredentials(user.ID.Value);

                case AuthenticationProviderType.ACTIVE_DIRECTORY:
                    if (!ValidateActiveDirectory(user.AuthenticationID.Value, password, provider.AdServer.Value))
                        throw ErrorInvalidCredentials(user.ID.Value);
                    return;
            }
        }

        public User GetUser(string idOrEmail, string password)
        {
            idOrEmail = idOrEmail.Trim();
            if (idOrEmail.Length == 0)
                throw new Error(Label("Invalid user ID"));

            var user = new User();
            user.Enabled.SetRange(true);

            if (idOrEmail.Contains("@"))
            {
                user.EMail.SetRange(idOrEmail);
                if (user.Count() > 1)
                    throw new Error(Label("Ambiguous e-mail '{0}'", idOrEmail));
            }
            else
                user.ID.SetRange(idOrEmail);

            if (!user.FindFirst())
                throw ErrorInvalidCredentials(idOrEmail);

            TestUserPassword(user, password);
            return user;
        }

        public User? TryGetUser(string idOrEmail, string password)
        {
            try
            {
                return GetUser(idOrEmail, password);
            }
            catch
            {
                return null;
            }
        }

        public bool TryAuthenticateToken(string token)
        {
            string userId = "";
            bool logAuth = false;

            Authentication authentication = new();
            authentication.ID.SetRange(token);
            if (authentication.FindFirst())
            {
                userId = authentication.UserID.Value;
                logAuth = true;
            }
            else
            {
                Session sess = new();
                sess.AccessToken.SetRange(token);
                if (sess.FindFirst())
                    userId = sess.UserID.Value;
            }

            if (userId.Length == 0)
                return false;

            User user = new();
            if (!user.Get(userId))
                return false;

            user.LastLogin.Value = DateTime.Now;
            user.Modify();

            CurrentSession.UserId = user.ID.Value;
            CurrentSession.IsSuperuser = user.Superuser.Value;

            if ((CurrentSession.Type == Shaper.SessionTypes.WEBCLIENT) && logAuth)
            {
                ApplicationLog log = new();
                log.Add(ApplicationLogType.INFORMATION, Label("User logged in (token)"));
            }

            if ((CurrentSession.UserId.Length > 0) && (!Shaper.Loader.Permissions.Exists()))
                LoadPermissions();

            Commit();
            return true;
        }

        public void LoadPermissions()
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

        public AccessTokenResponse AuthenticateUser(User user, AccessTokenFormat tokenFormat = AccessTokenFormat.Guid, int expireSeconds = 0)
        {
            var session = new Session();
            if (!session.Get(CurrentSession.Id))
                throw session.ErrorNotFound();

            session.UserID.Value = user.ID.Value;
            session.Modify();

            user.LastLogin.Value = DateTime.Now;
            user.Modify();

            AccessTokenResponse token = new();
            token.token_type = "bearer";
            token.expires_in = expireSeconds;

            switch (tokenFormat)
            {
                case AccessTokenFormat.Guid:
                    token.access_token = Guid.NewGuid().ToString("n");
                    break;
                case AccessTokenFormat.DoubleGuid:
                    token.access_token = Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n");
                    break;
                case AccessTokenFormat.Sha256:
                    token.access_token = Functions.Hash(Guid.NewGuid().ToString());
                    break;
            }

            Authentication auth = new();
            auth.ID.Value = token.access_token!;
            auth.CreationDateTime.Value = DateTime.Now;
            auth.ExpireDateTime.Value = DateTime.Now.AddSeconds((expireSeconds > 0) ? expireSeconds : 30);  // allow refresh
            auth.UserID.Value = user.ID.Value;
            auth.Insert();

            CurrentSession.AuthenticationId = token.access_token!;
            CurrentSession.UserId = user.ID.Value;
            CurrentSession.IsSuperuser = user.Superuser.Value;

            ApplicationLog log = new();
            log.Add(ApplicationLogType.INFORMATION, Label("User logged in"));

            if ((CurrentSession.UserId.Length > 0) && (!Shaper.Loader.Permissions.Exists()))
                LoadPermissions();

            return token;
        }
    }
}
