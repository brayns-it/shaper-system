namespace Brayns.System
{
    public class Confirm : Page<Confirm>
    {
        public Controls.Html Html { get; init; }
        public Controls.ContentArea ContentArea { get; init; }
        public Controls.ActionArea ActionArea { get; init; }
        public Controls.Action ActionYes { get; init; }
        public Controls.Action ActionNo { get; init; }

        public Confirm(string text = "", Controls.ActionTriggerHandler? onYes = null, Controls.ActionTriggerHandler? onNo = null)
        {
            UnitName = "Confirm";
            UnitCaption = Label("Confirm");
            PageType = PageTypes.Normal;

            ContentArea = new(this);
            {
                Html = new(ContentArea);
                if (text != null) Html.Content = text;
            }

            ActionArea = new(this);
            {
                ActionYes = new(ActionArea) { Caption = Label("Yes") };
                ActionYes.Triggering += Actions_Triggering;
                if (onYes != null) ActionYes.Triggering += onYes;

                ActionNo = new(ActionArea) { Caption = Label("No") };
                ActionNo.Triggering += Actions_Triggering;
                ActionNo.Shortcut = "Escape";
                if (onNo != null) ActionNo.Triggering += onNo;
            }
        }

        private void Actions_Triggering()
        {
            Close();
        }
    }
}
