namespace Brayns.System
{
    public class Information : Table<Information>
    {
        public Fields.Code PK { get; } = new("PK", Label("PK"), 20);
        public Fields.Text Name { get; } = new("Name", Label("Name"), 50);
        public Fields.Text Description { get; } = new("Description", Label("Description"), 250);
        public Fields.Text Footer { get; } = new("Footer", Label("Footer"), 250);
        public Fields.Text Indicator { get; } = new("Indicator", Label("Indicator"), 20);

        public Information()
        {
            UnitName = "Information";
            UnitCaption = Label("Information");
            TablePrimaryKey.Add(PK);
        }

        public string GetFooter()
        {
            string result = Footer.Value;
            result = result.Replace("%Y", DateTime.Now.Year.ToString());
            return result;
        }
    }
}
