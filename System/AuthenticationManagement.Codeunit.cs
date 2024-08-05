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
                    if (user.Password.Value != Functions.Hash(password))
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
            try
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
            catch
            {
                if (CurrentSession.Type == Shaper.SessionTypes.WEBCLIENT)
                {
                    ApplicationLog.Add(ApplicationLogType.SECURITY, Label("User {0} failed login", idOrEmail));
                    Commit();
                }

                throw;
            }
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

        public void RefreshSessionToken(string token)
        {
            Authentication authentication = new();
            authentication.ID.SetRange(token);
            authentication.Session.SetRange(true);
            authentication.ExpireDateTime.SetFilter(">={0}", DateTime.Now);
            if (authentication.FindFirst())
            {
                authentication.ExpireDateTime.Value = DateTime.Now.AddSeconds(authentication.Duration.Value);
                authentication.Modify();
                Commit();
            }
        }

        public bool TryAuthenticateToken(string token)
        {
            Authentication authentication = new();
            authentication.ID.SetRange(token);
            authentication.ExpireDateTime.SetFilter(">={0}", DateTime.Now);
            if (!authentication.FindFirst())
                return false;

            User user = new();
            user.TableLock = true;
            if (!user.Get(authentication.UserID.Value))
                return false;

            if (!user.Enabled.Value)
                return false;

            user.LastLogin.Value = DateTime.Now;
            user.Modify();

            CurrentSession.UserId = user.ID.Value;
            CurrentSession.IsSuperuser = user.Superuser.Value;

            if ((CurrentSession.Type == Shaper.SessionTypes.WEBCLIENT) && (!authentication.Session.Value))
                ApplicationLog.Add(ApplicationLogType.INFORMATION, Label("User logged in (token)"));

            Commit();

            if ((CurrentSession.UserId.Length > 0) && (!Shaper.Loader.Permissions.Exists()))
                LoadPermissions();
                        
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

        public Authentication CreateAuthenticationToken(User user, AccessTokenFormat tokenFormat = AccessTokenFormat.Guid, int expireSeconds = 0, bool forSession = false)
        {
            Authentication auth = new();

            switch (tokenFormat)
            {
                case AccessTokenFormat.Guid:
                    auth.ID.Value = Guid.NewGuid().ToString("n");
                    break;
                case AccessTokenFormat.DoubleGuid:
                    auth.ID.Value = Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n");
                    break;
                case AccessTokenFormat.Sha256:
                    auth.ID.Value = Functions.Hash(Guid.NewGuid().ToString());
                    break;
            }

            auth.CreationDateTime.Value = DateTime.Now;
            auth.UserID.Value = user.ID.Value;
            auth.ExpireDateTime.Value = DateTime.Now.AddSeconds(expireSeconds);
            auth.Session.Value = forSession;
            auth.Duration.Value = expireSeconds;
            auth.Insert();

            return auth;
        }

        public AccessTokenResponse AuthenticateUser(User user, AccessTokenFormat tokenFormat = AccessTokenFormat.Guid, int expireSeconds = 0, bool forSession = false)
        {
            var session = new Session();
            if (!session.Get(CurrentSession.Id))
                throw session.ErrorNotFound();

            session.UserID.Value = user.ID.Value;
            session.Modify();

            user.TableLock = true;
            user.Refresh();
            user.LastLogin.Value = DateTime.Now;
            user.Modify();

            var auth = CreateAuthenticationToken(user, tokenFormat, expireSeconds, forSession);

            AccessTokenResponse token = new();
            token.token_type = "bearer";
            token.expires_in = expireSeconds;
            token.access_token = auth.ID.Value;

            CurrentSession.AuthenticationId = token.access_token!;
            CurrentSession.UserId = user.ID.Value;
            CurrentSession.IsSuperuser = user.Superuser.Value;

            if (CurrentSession.Type == Shaper.SessionTypes.WEBCLIENT)
                ApplicationLog.Add(ApplicationLogType.INFORMATION, Label("User logged in"));

            Commit();

            if ((CurrentSession.UserId.Length > 0) && (!Shaper.Loader.Permissions.Exists()))
                LoadPermissions();

            return token;
        }
    }
}
