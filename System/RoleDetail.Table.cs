namespace Brayns.System
{
    public class RolePermission : OptList
    {
        public const int NONE = 0;

        [Label("Allowed")]
        public const int ALLOWED = 1;

        [Label("Allowed (indirect)")]
        public const int ALLOWED_INDIRECT = 2;

        [Label("Denied")]
        public const int DENIED = 3;
    }

    public class RoleDetail : Table<RoleDetail>
    {
        public Fields.Code RoleCode { get; } = new("Role code", Label("Role code"), 10);
        public Fields.Integer LineNo { get; } = new("Line no.", Label("Line no."));
        public Fields.Option<UnitTypes> ObjectType { get; } = new("Object type", Label("Object type"));
        public Fields.Text ObjectName { get; } = new("Object name", Label("Object name"), 250);
        public Fields.Option<RolePermission> Execution { get; } = new("Execution", Label("Execution"));

        public RoleDetail()
        {
            TableName = "Role detail";
            UnitCaption = Label("Role detail");
            TablePrimaryKey.Add(RoleCode, LineNo);

            AddRelation<Role>(RoleCode);
        }
    }
}
