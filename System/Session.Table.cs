namespace Brayns.System
{
    public class Session : Table<Session>
    {
        public Fields.Guid ID { get; } = new("ID", Label("ID"));
        public Fields.Option<Shaper.SessionTypes> Type { get; } = new("Type", Label("Type"));
        public Fields.Text Address { get; } = new("Address", Label("Address"), 50);
        public Fields.Text Server { get; } = new("Server", Label("Server"), 100);
        public Fields.Text Environment { get; } = new("Environment", Label("Environment"), 100);
        public Fields.Integer ProcessID { get; } = new("Process ID", Label("Process ID"));
        public Fields.Integer ThreadID { get; } = new("Thread ID", Label("Thread ID"));
        public Fields.DateTime CreationDateTime { get; } = new("Creation date/time", Label("Creation date/time"));
        public Fields.DateTime LastDateTime { get; } = new("Last date/time", Label("Last date/time"));
        public Fields.Code UserID { get; } = new("User ID", Label("User ID"), 50);
        public Fields.Integer DatabaseID { get; } = new("Database ID", Label("Database ID"));
        public Fields.Boolean Active { get; } = new("Active", Label("Active"));
        public Fields.Text AccessToken { get; } = new("Access token", Label("Access token"), 2048);

        public Session()
        {
            TableName = "Session";
            UnitCaption = Label("Session");
            TablePrimaryKey.Add(ID);

            TableIndexes.Add("K01", AccessToken);

            AddRelation<User>(UserID);
        }
    }
}
