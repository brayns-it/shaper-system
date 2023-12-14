namespace Brayns.System
{
    public class Notification : Table<Notification>
    {
        public Fields.Integer EntryNo { get; } = new("Entry no.", Label("Entry No.")) { AutoIncrement = true };
        public Fields.Code UserID { get; } = new("User ID", Label("User ID"), 50);
        public Fields.Text Title { get; } = new("Title", Label("Title"), 50);
        public Fields.Text Description { get; } = new("Description", Label("Description"), 250);
        public Fields.Boolean IsRead { get; } = new("Is read", Label("Is read"));
        public Fields.DateTime CreationDateTime { get; } = new("Creation date/time", Label("Creation date/time"));

        public Notification()
        {
            TableName = "Notification";
            UnitCaption = Label("Notification");
            TablePrimaryKey.Add(EntryNo);

            AddRelation<User>(UserID);
        }

        public void Notify(string title, string description)
        {
            Init();
            UserID.Value = CurrentSession.UserId;
            Title.Value = title;
            Description.Value = description;
            CreationDateTime.Value = DateTime.Now;
            Insert();
        }
    }
}
