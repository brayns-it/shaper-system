namespace Brayns.System
{
    public class PrintSetup : Table<PrintSetup>
    {
        public Fields.Code ProfileCode { get; } = new("Profile code", Label("Profile code"), 10);
        public Fields.Text Description { get; } = new("Description", Label("Description"), 30);
        public Fields.Boolean Default { get; } = new("Default", Label("Default"));
        public Fields.Text PrintServer { get; } = new("Print server", Label("Print server"), 200);
        public Fields.Text AuthToken { get; } = new("Authentication token", Label("Authentication token"), 100);

        public PrintSetup()
        {
            TableName = "Print setup";
            UnitCaption = Label("Print setup");
            TablePrimaryKey.Add(ProfileCode);

            AuthToken.Validating += AuthToken_Validating;
        }

        private void AuthToken_Validating()
        {
            AuthToken.Value = Functions.EncryptString(AuthToken.Value);
        }
    }
}
