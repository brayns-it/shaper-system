namespace Brayns.System
{
    [Shaper.Classes.AppModule]
    public class App : Shaper.Classes.AppModule
    {
        private Upgrades upgrades = new();

        public override string Author => "Brayns";
        public override string Name => "System";
        public override Version Version => new Version("1.0.30902.0");
        public override Guid Id => new Guid("{7DE54A0D-1A0B-4A80-930F-EDB727D4EC9C}");

        public override void Install()
        {
            if (!upgrades.Get("APPS-SYSTEM-INSTALL"))
            {
                var nfo = new Information();
                if (!nfo.Get())
                {
                    nfo.Init();
                    nfo.Name.Value = "Application";
                    nfo.Description.Value = "My application";
                    nfo.Footer.Value = "Copyright %Y";
                    nfo.Indicator.Value = "PROD";
                    nfo.Insert();
                }

                var usr = new User();
                if (!usr.Get("ADMIN"))
                {
                    usr.Init();
                    usr.ID.Value = "ADMIN";
                    usr.Name.Value = "Administrator";
                    usr.EMail.Value = "admin@localhost";
                    usr.Password.Validate("admin");
                    usr.Enabled.Value = true;
                    usr.Superuser.Value = true;
                    usr.Type.Value = UserTypes.USER;
                    usr.Insert();
                }

                upgrades.InsertTag("APPS-SYSTEM-INSTALL");
            }
        }

        public static void Load()
        {
            // do nothing, use only to load assembly in the domain
        }
    }
}
