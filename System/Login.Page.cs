namespace Brayns.System
{
    public partial class Login : Page<Login>
    {
        protected bool LoginByID { get; set; } = false;
        protected Fields.Text ID { get; } = new(Label("ID"));
        protected Fields.Text EMail { get; } = new(Label("E-Mail"));
        protected Fields.Text Password { get; } = new(Label("Password"));
        protected Fields.Boolean Remember { get; } = new(Label("Remember"));

        protected override void Initialize()
        {
            UnitCaption = Label("Login");
            PageType = PageTypes.Login;

            var content = Controls.ContentArea.Create(this);
            {
                var form = new Controls.Group(content);
                {
                    form.Primary = true;
                    form.LabelStyle = Controls.LabelStyle.Vertical;
                    form.FieldPerRow = Controls.FieldPerRow.One;
                    form.Collapsible = false;

                    new Controls.Field(form, "id", ID);
                    new Controls.Field(form, "email", EMail);
                    new Controls.Field(form, Password) { InputType = Controls.InputType.Password };
                    new Controls.Field(form, Remember);

                    var login = new Controls.Action(form, Label("Login"));
                    login.Triggering += Login_Triggering;
                    login.Shortcut = "Enter";
                }
            }

            Loading += Login_Loading;
        }

        protected override void AfterExtend()
        {
            if (LoginByID)
                Control("email")!.Detach();
            else
                Control("id")!.Detach();
        }

        private void Login_Loading()
        {
            var nfo = new Information();
            nfo.Get();

            Control<Controls.Group>()!.Caption = nfo.Name.Value;
        }

        private void Login_Triggering()
        {
            var clMgmt = new ClientManagement();
            clMgmt.RememberToken = Remember.Value;

            if (LoginByID)
                clMgmt.LoginByID(ID.Value, Password.Value);
            else
                clMgmt.LoginByEmail(EMail.Value, Password.Value);

            Client.Reload();
        }
    }
}
