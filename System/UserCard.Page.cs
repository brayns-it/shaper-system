namespace Brayns.System
{
    public partial class UserCard : Page<UserCard, User>
    {
        protected Fields.Text Token { get; init; } = new Fields.Text(Label("Token"));

        public UserCard()
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
                    new Controls.Field(general, "type", Rec.Type);
                    new Controls.Field(general, "authProvider", Rec.AuthenticationProvider);
                    new Controls.Field(general, "lastlogin", Rec.LastLogin) { ReadOnly = true };
                    new Controls.Field(general, "enabled", Rec.Enabled);
                    new Controls.Field(general, "superuser", Rec.Superuser);
                }

                new Controls.Subpage<UserRoleList, UserRole>(area)
                {
                    Filter = (tgt) => tgt.UserID.SetRange(Rec.ID.Value)
                };
            }

            Extend();
        }
    }
}
