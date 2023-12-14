namespace Brayns.System
{
    public class Role : Table<Role>
    {
        public Fields.Code Code { get; } = new("Code", Label("Code"), 10);
        public Fields.Text Description { get; } = new("Description", Label("Description"), 50);

        public Role()
        {
            TableName = "Role";
            UnitCaption = Label("Role");
            TablePrimaryKey.Add(Code);

            Deleting += Role_Deleting;
        }

        private void Role_Deleting()
        {
            var detail = new RoleDetail();
            detail.RoleCode.SetRange(Code.Value);
            detail.DeleteAll();
        }
    }
}
