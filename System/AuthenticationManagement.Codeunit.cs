﻿using System.DirectoryServices.Protocols;
using System.Net;

namespace Brayns.System
{
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
    }
}
