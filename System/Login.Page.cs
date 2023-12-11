namespace Brayns.System
{
    public class Login : Page<Login>
    {
        protected Fields.Text EMail { get; } = new(Label("E-Mail"));
        protected Fields.Text Password { get; } = new(Label("Password"));
        protected Fields.Boolean Remember { get; } = new(Label("Remember"));

        public Login()
        {
            UnitName = "Login";
            UnitCaption = Label("Login");
            PageType = PageTypes.Login;

            var content = Controls.ContentArea.Create(this);
            {
                var form = new Controls.Group(content);
                {
                    form.Primary = true;
                    form.LabelOrientation = Controls.LabelOrientation.Vertical;
                    form.FieldPerRow = Controls.FieldPerRow.One;
                    form.Collapsible = false;

                    new Controls.Field(form, EMail);
                    new Controls.Field(form, Password) { InputType = Controls.InputType.Password };
                    new Controls.Field(form, Remember);

                    var login = new Controls.Action(form, Label("Login"));
                    login.Triggering += Login_Triggering;
                    login.Shortcut = "Enter";
                }
            }

            Loading += Login_Loading;
        }

        private void Login_Loading()
        {
            var nfo = new Information();
            nfo.Get();

            Control<Controls.Group>()!.Caption = nfo.Name.Value;
            Control<Controls.Footer>()!.Caption = nfo.GetFooter();
        }

        private void Login_Triggering()
        {
            var clMgmt = new ClientManagement();
            clMgmt.RememberToken = Remember.Value;
            clMgmt.LoginByEmail(EMail.Value, Password.Value);
            Client.Reload();
        }
    }
}
