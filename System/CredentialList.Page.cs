namespace Brayns.System
{
    public class CredentialList : Page<CredentialList, Credential>
    {
        public CredentialList()
        {
            UnitCaption = Label("Credentials");
            Card = typeof(CredentialCard);

            var content = Controls.ContentArea.Create(this);
            {
                var grid = new Controls.Grid(content);
                {
                    new Controls.Field(grid, Rec.Code);
                    new Controls.Field(grid, Rec.Description);
                    new Controls.Field(grid, Rec.Host);
                    new Controls.Field(grid, Rec.Username);
                    new Controls.Field(grid, Rec.Type);
                }
            }
        }
    }
}
