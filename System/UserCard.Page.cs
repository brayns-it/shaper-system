namespace Brayns.System
{
    public partial class UserCard : Page<UserCard, User>
    {
        protected Fields.Text Token { get; init; } = new Fields.Text(Label("Token"));

        protected override void Initialize()
        {
            UnitCaption = Label("User");

            var area = Controls.ContentArea.Create(this);
            {
                var general = new Controls.Group(area, "general", Label("General"));
                {
                    new Controls.Field(general, "id", Rec.ID);
                    new Controls.Field(general, "name", Rec.Name);
                    new Controls.Field(general, "email", Rec.EMail);
                    new Controls.Field(general, "password", Rec.Password) { InputType = Shaper.Controls.InputType.Password };
                    new Controls.Field(general, "type", Rec.Type) { ReadOnly = true };
                    new Controls.Field(general, "lastlogin", Rec.LastLogin) { ReadOnly = true };
                    new Controls.Field(general, "enabled", Rec.Enabled);
                    new Controls.Field(general, "superuser", Rec.Superuser);
                }

                var extAuth = new Controls.Group(area, "ext-authentication", Label("External authentication"));
                {
                    new Controls.Field(extAuth, "authProvider", Rec.AuthenticationProvider);
                    new Controls.Field(extAuth, "authID", Rec.AuthenticationID);
                }

                new Controls.Subpage<UserRoleList, UserRole>(area)
                {
                    Filter = (tgt) => tgt.UserID.SetRange(Rec.ID.Value)
                };
            }

            var acts = Controls.ActionArea.Create(this);
            {
                var tools = new Controls.Action(acts, "act-tools", Label("Tools"), Icon.FromName("fas fa-gear"));
                {
                    var tenYearToken = new Controls.Action(tools, Label("Generate 10-years token"), Icon.FromName("fas fa-ticket-alt"));
                    tenYearToken.Triggering += TenYearToken_Triggering;
                }
            }
        }

        private void TenYearToken_Triggering()
        {
            Confirm.Show(Label("Generate 10-years token for {0}?", Rec.ID.Value), () =>
            {
                var authMgmt = new AuthenticationManagement();
                var auth = authMgmt.CreateAuthenticationToken(Rec, AccessTokenFormat.Sha256, 10 * 365 * 86400);
                auth.SystemCreated.Value = true;
                auth.Modify();

                Message.Show(auth.ID.Value);
            });
        }
    }
}
