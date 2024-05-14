namespace Brayns.System
{
    public class TokenList : Page<TokenList, Authentication>
    {
        protected Fields.Text Token { get; init; } = new Fields.Text(Label("Token"));

        public TokenList()
        {
            UnitCaption = Label("Authentication Tokens");
            AllowInsert = false;
            AllowModify = false;

            var area = Controls.ContentArea.Create(this);
            {
                var grid = new Controls.Grid(area);
                {
                    new Controls.Field(grid, Token);
                    new Controls.Field(grid, Rec.UserID);
                    new Controls.Field(grid, Rec.CreationDateTime);
                    new Controls.Field(grid, Rec.ExpireDateTime);
                    new Controls.Field(grid, Rec.SystemCreated);
                }
            }

            var acts = Controls.ActionArea.Create(this);
            {
                var actShow = new Controls.Action(acts, Label("View token"), Icon.FromName("fas fa-eye"));
                actShow.Triggering += ActShow_Triggering;
            }

            DataReading += TokenList_DataReading;
        }

        private void ActShow_Triggering()
        {
            Message.Show(Rec.ID.Value);
        }

        private void TokenList_DataReading()
        {
            Token.Value = Rec.ID.Value.ToString().Substring(0, 8);
        }
    }
}
