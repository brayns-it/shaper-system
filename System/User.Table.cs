﻿namespace Brayns.System
{
    public partial class UserTypes : OptList
    {
        [Label("User")]
        public const int USER = 0;

        [Label("Device")]
        public const int DEVICE = 1;
    }

    public partial class User : Table<User>
    {
        public bool SkipComplexityCheck { get; set; } = false;

        public Fields.Code ID { get; } = new("ID", Label("ID"), 50);
        public Fields.Text Name { get; } = new("Name", Label("Name"), 50);
        public Fields.Text EMail { get; } = new("E-Mail", Label("E-Mail"), 100);
        public Fields.Boolean Enabled { get; } = new("Enabled", Label("Enabled"));
        public Fields.Text Password { get; } = new("Password", Label("Password"), 100);
        public Fields.DateTime LastLogin { get; } = new("Last login", Label("Last login"));
        public Fields.Boolean Superuser { get; } = new("Superuser", Label("Superuser"));
        public Fields.Option<UserTypes> Type { get; } = new("Type", Label("Type"));
        public Fields.Code AuthenticationProvider { get; } = new("Authentication provider", Label("Authentication provider"), 10);
        public Fields.Text AuthenticationID { get; } = new("Authentication ID", Label("Authentication ID"), 100);
        public Fields.Text StartPageName { get; } = new("Start page name", Label("Start page name"), 250);
        public Fields.Boolean ComplexPassword { get; } = new("Complex password", Label("Complex password"));

        public event GenericHandler? PasswordBeforeHashing;

        public User()
        {
            TableName = "User";
            UnitCaption = Label("User");
            TablePrimaryKey.Add(ID);

            AddRelation<AuthenticationProvider>(AuthenticationProvider);

            Password.Validating += Password_Validating;
        }

        private void Password_Validating()
        {
            PasswordBeforeHashing?.Invoke();

            if (ComplexPassword.Value && (!SkipComplexityCheck)) 
            {
                var authMgmt = new AuthenticationManagement();
                if (!authMgmt.IsComplexPassword(Password.Value, 8))
                    throw new Error(Label("Password does not meet complexity requirements"));
            }

            Password.Value = HashPassword(Password.Value);
        }

        public string HashPassword(string pwd)
        {
            return Functions.Hash(pwd);
        }
    }
}
