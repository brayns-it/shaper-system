namespace Brayns.System
{
    public class UserCard : Page<UserCard, User>
    {
        protected Fields.Text Token { get; init; } = new Fields.Text(Label("Token"));

        public UserCard()
        {
            UnitCaption = Label("User");

            var area = Controls.ContentArea.Create(this);
            {
                var general = new Controls.Group(area);
                {
                    new Controls.Field(general, Rec.ID);
                    new Controls.Field(general, Rec.Name);
                    new Controls.Field(general, Rec.EMail);
                    new Controls.Field(general, Rec.Password) { InputType = Shaper.Controls.InputType.Password };
                    new Controls.Field(general, Rec.Type);
                    new Controls.Field(general, Rec.LastLogin) { ReadOnly = true };
                    new Controls.Field(general, Rec.Enabled);
                    new Controls.Field(general, Rec.Superuser);
                }

                new Controls.Subpage<UserRoleList, UserRole>(area)
                {
                    Filter = (tgt) => tgt.UserID.SetRange(Rec.ID.Value)
                };
            }
        }
    }
}
