namespace Brayns.System
{
    public class AuthenticationProviderList : Page<AuthenticationProviderList, AuthenticationProvider>
    {
        public AuthenticationProviderList()
        {
            UnitCaption = Label("Authentication providers");
            Card = typeof(AuthenticationProviderCard);

            var area = Controls.ContentArea.Create(this);
            {
                var grid = new Controls.Grid(area);
                {
                    new Controls.Field(grid, Rec.Code);
                    new Controls.Field(grid, Rec.Description);
                    new Controls.Field(grid, Rec.ProviderType);
                }
            }
        }
    }
}
