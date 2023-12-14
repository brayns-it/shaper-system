namespace Brayns.System
{
    public class UserRole : Table<UserRole>
    {
        public Fields.Code UserID { get; } = new("User ID", Label("User ID"), 50);
        public Fields.Code RoleCode { get; } = new("Role code", Label("Role code"), 10);

        public UserRole()
        {
            TableName = "User role";
            UnitCaption = Label("User role");
            TablePrimaryKey.Add(UserID, RoleCode);

            AddRelation<User>(UserID);
            AddRelation<Role>(RoleCode);
        }
    }
}
