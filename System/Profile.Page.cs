namespace Brayns.System
{
    public class ProfileCard : Page<ProfileCard, User>
    {
        public ProfileCard()
        {
            UnitCaption = Label("User profile");
            AllowModify = true;
            AllowInsert = false;
            AllowDelete = false;

            var content = Controls.ContentArea.Create(this);
            {
                var general = new Controls.Group(content, Label("General"));
                {
                    new Controls.Field(general, Rec.ID) { ReadOnly = true };
                    new Controls.Field(general, Rec.Name);
                    new Controls.Field(general, Rec.EMail) { ReadOnly = true };
                    new Controls.Field(general, Rec.Password) { InputType = Shaper.Controls.InputType.Password };
                }
            }

            Loading += ProfileCard_Loading;
        }

        private void ProfileCard_Loading()
        {
            Rec.TableFilterLevel = Shaper.Fields.FilterLevel.Private;
            Rec.ID.SetRange(CurrentSession.UserId);
            Rec.TableFilterLevel = Shaper.Fields.FilterLevel.Public;
        }
    }
}
