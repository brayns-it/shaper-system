namespace Brayns.System
{
    public class ApplicationLogType : OptList
    {
        [Label("Information")]
        public const int INFORMATION = 0;

        [Label("Warning")]
        public const int WARNING = 1;

        [Label("Error")]
        public const int ERROR = 2;

        [Label("Security")]
        public const int SECURITY = 3;
    }

    public class ApplicationLog : Table<ApplicationLog>
    {
        public Fields.BigInteger EntryNo { get; } = new("Entry no.", Label("Entry no.")) { AutoIncrement = true };
        public Fields.DateTime EventDateTime { get; } = new("Event date/time", Label("Event date/time"));
        public Fields.Option<ApplicationLogType> LogType { get; } = new("Log type", Label("Log type"));
        public Fields.Text Message { get; } = new("Message", Label("Message"), 200);
        public Fields.Code UserID { get; } = new("User ID", Label("User ID"), 50);
        public Fields.Text Address { get; } = new("Address", Label("Address"), 50);
        public Fields.Text Details { get; } = new("Details", Label("Details"), Fields.Text.MAX_LENGTH);

        public ApplicationLog()
        {
            TableName = "Application log";
            UnitCaption = Label("Application log");
            TablePrimaryKey.Add(EntryNo);

            AddRelation<User>(UserID);
        }

        public void Add(Opt<ApplicationLogType> type, string message, string details = "")
        {
            Init();
            EventDateTime.Value = DateTime.Now;
            LogType.Value = type;
            Message.Value = message.Truncate(Message.Length);
            UserID.Value = CurrentSession.UserId;
            Address.Value = CurrentSession.Address;
            Details.Value = details;
            Insert();
        }
    }
}
