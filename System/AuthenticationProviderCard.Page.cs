namespace Brayns.System
{
    public class AuthenticationProviderCard : Page<AuthenticationProviderCard, AuthenticationProvider>
    {
        public Fields.Text AdUser { get; } = new(Label("User name"));
        public Fields.Text AdPassword { get; } = new(Label("Password"));

        public AuthenticationProviderCard()
        {
            UnitCaption = Label("Authentication provider");

            var area = Controls.ContentArea.Create(this);
            {
                var general = new Controls.Group(area, Label("General"));
                {
                    new Controls.Field(general, Rec.Code);
                    new Controls.Field(general, Rec.Description);
                    new Controls.Field(general, Rec.ProviderType);
                }

                var ad = new Controls.Group(area, Label("Active Directory"));
                {
                    new Controls.Field(ad, Rec.AdDomain) { Caption = Label("Domain name") };
                    new Controls.Field(ad, AdUser);
                    new Controls.Field(ad, AdPassword) { InputType = Shaper.Controls.InputType.Password };

                    var tryAdAuth = new Controls.Action(ad, Label("Try authentication"));
                    tryAdAuth.Triggering += TryAdAuth_Triggering;
                }
            }
        }

        private void TryAdAuth_Triggering()
        {
            var authMgmt = new AuthenticationManagement();
            if (authMgmt.ValidateActiveDirectory(AdUser.Value, AdPassword.Value, Rec.AdDomain.Value))
                new Message(Label("AD authentication successful")).RunModal();
            else
                throw new Error(Label("AD authentication failed"));
        }
    }
}
