namespace Brayns.System
{
    public class Login : Page<Login>
    {
        protected Controls.Group Form { get; init; }
        protected Controls.Footer Footer { get; init; }
        protected Fields.Text EMail { get; } = new("E-Mail", Label("E-Mail"), 100);
        protected Fields.Text Password { get; } = new("Password", Label("Password"), 100);
        protected Fields.Boolean Remember { get; } = new("Remember", Label("Remember"));

        public Login()
        {
            UnitName = "Login";
            UnitCaption = Label("Login");
            PageType = PageTypes.Login;

            var content = new Controls.ContentArea(this);
            {
                Form = new(content);
                {
                    Form.Primary = true;
                    Form.LabelOrientation = Controls.LabelOrientation.Vertical;
                    Form.FieldPerRow = Controls.FieldPerRow.One;
                    Form.Collapsible = false;

                    new Controls.Field(Form, EMail);
                    new Controls.Field(Form, Password) { InputType = Controls.InputType.Password };
                    new Controls.Field(Form, Remember);

                    var login = new Controls.Action(Form, Label("Login"));
                    login.Triggering += Login_Triggering;
                    login.Shortcut = "Enter";
                }
            }

            Footer = new(this);
        }

        private void Login_Triggering()
        {
            var clMgmt = new ClientManagement();
            clMgmt.RememberToken = Remember.Value;
            clMgmt.LoginByEmail(EMail.Value, Password.Value);
            Client.Reload();
        }

        protected override void OnLoad()
        {
            var nfo = new Information();
            nfo.Get();
            Form.Caption = nfo.Name.Value;
            Footer.Caption = nfo.GetFooter();
            Remember.Value = true;
        }
    }
}
