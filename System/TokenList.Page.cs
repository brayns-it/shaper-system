namespace Brayns.System
{
    public class TokenList : Page<TokenList, Authentication>
    {
        protected Fields.Text Token { get; init; } = new Fields.Text(Label("Token"));

        public TokenList()
        {
            UnitName = "Token list";
            UnitCaption = Label("Authentication Tokens");
            AllowInsert = false;

            var area = Controls.ContentArea.Create(this);
            {
                var grid = new Controls.Grid(area);
                {
                    new Controls.Field(grid, Token);
                    new Controls.Field(grid, Rec.UserID);
                    new Controls.Field(grid, Rec.CreationDateTime);
                    new Controls.Field(grid, Rec.ExpireDateTime);
                }
            }

            DataReading += TokenList_DataReading;
        }

        private void TokenList_DataReading()
        {
            Token.Value = Rec.ID.Value.ToString().Substring(0, 8);
        }
    }
}
