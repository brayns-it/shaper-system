namespace Brayns.System
{
    public class ClientManagement : Codeunit
    {
        AuthenticationManagement AuthMgmt = new();

        public bool RememberToken { get; set; }

        public AccessTokenResponse Login(string idOrEmail, string password)
        {
            var user = AuthMgmt.GetUser(idOrEmail, password);
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
            AccessTokenResponse result = AuthMgmt.AuthenticateUser(
                user,
                AccessTokenFormat.Guid,
                RememberToken ? (30 * 86400) : 0);

            DateTimeOffset? exp = (result.expires_in > 0) ? DateTimeOffset.Now.AddSeconds(result.expires_in) : null;
            Client.SetAuthenticationToken(result.access_token, exp);

            return result;
        }
    }
}
