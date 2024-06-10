namespace Brayns.System
{
    public partial class Login : Page<Login>
    {
        protected Fields.Text ID { get; } = new(Label("ID or E-Mail"));
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

                    new Controls.Field(form, ID);
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
        }

        private void Login_Triggering()
        {
            var clMgmt = new ClientManagement();
            if (Remember.Value)
                clMgmt.TokenDurationSec = 30 * 86400;
            else
            {
                clMgmt.TokenDurationSec = 300;
                clMgmt.ForSession = true;
            }
            clMgmt.Login(ID.Value, Password.Value);
            Client.Reload();
        }
    }
}
