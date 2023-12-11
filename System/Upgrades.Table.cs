namespace Brayns.System
{
    public class Upgrades : Table<Upgrades>
    {
        public Fields.Code Tag { get; } = new("Tag", Label("Tag"), 100);
        public Fields.DateTime Date_Time { get; } = new("Date/time", Label("Date/time"));

        public Upgrades()
        {
            UnitName = "Upgrades";
            UnitCaption = Label("Upgrades");
            TablePrimaryKey.Add(Tag);

            Inserting += Upgrades_Inserting;
        }

        private void Upgrades_Inserting()
        {
            Date_Time.Value = DateTime.Now;
        }

        public void InsertTag(string tag)
        {
            Init();
            Tag.Value = tag;
            Insert(true);
        }
    }
}
