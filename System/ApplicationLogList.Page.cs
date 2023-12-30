namespace Brayns.System
{
    public class ApplicationLogList : Page<ApplicationLogList, ApplicationLog>
    {
        public ApplicationLogList()
        {
            UnitCaption = Label("Application log");
            AllowInsert = false;
            AllowDelete = false;
            AllowModify = false;

            var area = Controls.ContentArea.Create(this);
            {
                var grid = new Controls.Grid(area);
                {
                    new Controls.Field(grid, Rec.EventDateTime);
                    new Controls.Field(grid, Rec.LogType);
                    new Controls.Field(grid, Rec.UserID);
                    new Controls.Field(grid, Rec.Message);
                }
            }

            Loading += ApplicationLogList_Loading;
        }

        private void ApplicationLogList_Loading()
        {
            Rec.TableAscending = false;
        }
    }
}
