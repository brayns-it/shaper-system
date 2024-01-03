namespace Brayns.System
{
    public class DeviceList : Page<DeviceList, User>
    {
        public DeviceList()
        {
            UnitCaption = Label("Devices");
            Card = typeof(UserCard);

            var area = Controls.ContentArea.Create(this);
            {
                var grid = new Controls.Grid(area);
                {
                    new Controls.Field(grid, Rec.ID);
                    new Controls.Field(grid, Rec.Name);
                    new Controls.Field(grid, Rec.LastLogin);
                    new Controls.Field(grid, Rec.Enabled);
                    new Controls.Field(grid, Rec.Superuser);
                }
            }

            Loading += DeviceList_Loading;
        }

        private void DeviceList_Loading()
        {
            Rec.TableFilterLevel = Shaper.Fields.FilterLevel.Private;
            Rec.Type.SetRange(UserTypes.DEVICE);
        }
    }
}
