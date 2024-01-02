namespace Brayns.System
{
    public class AuthenticationProviderType
    {
        public const int NONE = 0;

        [Label("Active Directory")]
        public const int ACTIVE_DIRECTORY = 1;
    }

    public class AuthenticationProvider : Table<AuthenticationProvider>
    {
        public Fields.Code Code { get; } = new("Code", Label("Code"), 10);
        public Fields.Text Description { get; } = new("Description", Label("Description"), 30);
        public Fields.Option<AuthenticationProviderType> ProviderType { get; } = new("Provider type", Label("Provider type"));

        public Fields.Text AdServer { get; } = new("AD server", Label("AD server"), 100);

        public AuthenticationProvider()
        {
            TableName = "Authentication provider";
            UnitCaption = Label("Authentication provider");
            TablePrimaryKey.Add(Code);
        }
    }
}
