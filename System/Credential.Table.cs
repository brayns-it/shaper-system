namespace Brayns.System
{
    public class CredentialType : OptList
    {
        public const int GENERIC = 0;

        [Label("Windows")]
        public const int WINDOWS = 1;
    }

    public class Credential : Table<Credential>
    {
        public Fields.Code Code { get; } = new("Code", Label("Code"), 20);
        public Fields.Text Description { get; } = new("Description", Label("Description"),50);
        public Fields.Text Host { get; } = new("Host", Label("Host"), 100);
        public Fields.Text Username { get; } = new("Username", Label("Username"), 100);
        public Fields.Text Password { get; } = new("Password", Label("Password"), 100);
        public Fields.Text Domain { get; } = new("Domain", Label("Domain"), 100);
        public Fields.Option<CredentialType> Type { get; } = new("Type", Label("Type"));

        public Credential()
        {
            TableName = "Credential";
            UnitCaption = Label("Credential");
            TablePrimaryKey.Add(Code);

            Password.Validating += Password_Validating;
        }

        private void Password_Validating()
        {
            Password.Value = Functions.EncryptString(Password.Value);
        }
    }
}
