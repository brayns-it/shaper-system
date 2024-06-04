namespace Brayns.System
{
    public class Authentication : Table<Authentication>
    {
        public Fields.Text ID { get; } = new("ID", Label("ID"), 2048);
        public Fields.Code UserID { get; } = new("User ID", Label("User ID"), 50);
        public Fields.DateTime CreationDateTime { get; } = new("Creation date/time", Label("Creation date/time"));
        public Fields.DateTime ExpireDateTime { get; } = new("Expire date/time", Label("Expire date/time"));
        public Fields.Integer Duration { get; } = new("Duration (sec.)", Label("Duration (sec.)"));
        public Fields.Boolean SystemCreated { get; } = new("System created", Label("System created"));
        public Fields.Boolean Session { get; } = new("Session", Label("Session"));

        public Authentication()
        {
            TableName = "Authentication";
            UnitCaption = Label("Authentication");
            TablePrimaryKey.Add(ID);

            AddRelation<User>(UserID);
        }
    }
}
