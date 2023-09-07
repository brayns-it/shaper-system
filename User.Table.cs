namespace Brayns.System
{
    public class UserType : Option<UserType>
    {
        public static readonly UserType USER = New(0, "User");
        public static readonly UserType DEVICE = New(1, "Device");
    }

    public class User : Table<User>
    {
        public Fields.Code ID { get; } = new("ID", Label("ID"), 50);
        public Fields.Text Name { get; } = new("Name", Label("Name"), 50);
        public Fields.Text EMail { get; } = new("E-Mail", Label("E-Mail"), 100);
        public Fields.Boolean Enabled { get; } = new("Enabled", Label("Enabled"));
        public Fields.Text Password { get; } = new("Password", Label("Password"), 100);
        public Fields.DateTime LastLogin { get; } = new("Last login", Label("Last login"));
        public Fields.Boolean Superuser { get; } = new("Superuser", Label("Superuser"));
        public Fields.Option<UserType> Type { get; } = new("Type", Label("Type"));

        public User()
        {
            UnitName = "User";
            UnitCaption = Label("User");
            TablePrimaryKey.Add(ID);

            Password.Validating += Password_Validating;
        }

        private void Password_Validating()
        {
            Password.Value = HashPassword(Password.Value);
        }

        public string HashPassword(string pwd)
        {
            return Functions.Hash(pwd);
        }
    }
}
