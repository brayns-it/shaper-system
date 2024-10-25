namespace Brayns.System
{
    public class SessionList : Page<SessionList, Session>
    {
        protected Fields.Text ID { get; init; } = new Fields.Text(Label("Session ID"));

        public SessionList()
        {
            UnitCaption = Label("Sessions");
            AllowInsert = false;
            AllowModify = false;

            var area = Controls.ContentArea.Create(this);
            {
                var grid = new Controls.Grid(area);
                {
                    new Controls.Field(grid, ID);
                    new Controls.Field(grid, Rec.Type);
                    new Controls.Field(grid, Rec.UserID);
                    new Controls.Field(grid, Rec.Address);
                    new Controls.Field(grid, Rec.Server);
                    new Controls.Field(grid, Rec.Environment);
                    new Controls.Field(grid, Rec.ProcessID);
                    new Controls.Field(grid, Rec.DatabaseID);
                    new Controls.Field(grid, Rec.CreationDateTime);
                    new Controls.Field(grid, Rec.LastDateTime);
                    new Controls.Field(grid, Rec.Active);
                }
            }

            var actions = Controls.ActionArea.Create(this);
            {
                var tools = new Controls.Action(actions, Label("Tools"), Icon.FromName("fas fa-gear"));
                {
                    var toggleDbDebug = new Controls.Action(tools, Label("Toggle database debug"));
                    toggleDbDebug.Triggering += () =>
                    {
                        Rec.Reload();
                        Rec.DatabaseDebug.Value = true;
                        Rec.Modify();
                    };
                };
            }

            DataReading += SessionList_DataReading;
        }

        private void SessionList_DataReading()
        {
            ID.Value = Rec.ID.Value.ToString().Substring(0, 8);
        }
    }
}
