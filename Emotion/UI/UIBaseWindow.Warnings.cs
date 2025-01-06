namespace Emotion.UI;

public partial class UIBaseWindow
{
    public class UIWarning
    {
        public string Warning;
        public string Suggest;

        public UIWarning(string warning, string suggest, UIBaseWindow window)
        {
            Warning = warning;
            Suggest = suggest;
        }
    }

    public List<UIWarning> GetWarnings()
    {
        List<UIWarning> warnings = new List<UIWarning>();
        WarningCheckCenterFill(warnings);

        return warnings;
    }

    public const string WARN_CENTER_FILL = "Window is centered via anchor, but also has fill enabled in the same axis. This seems a bit sus, but there are such valid cases.";
    public const string WARN_CENTER_FILL_SUGGEST = "Set FillX or FillY (depending on which direction you're hoping to be centered) to false.";

    private void WarningCheckCenterFill(List<UIWarning> list)
    {
        if (!AnchorsInsideParent(ParentAnchor, Anchor)) return;

        if (FillY)
        {
            if (ParentAnchor == UIAnchor.CenterLeft || ParentAnchor == UIAnchor.CenterRight || ParentAnchor == UIAnchor.CenterCenter)
            {
                list.Add(new UIWarning(WARN_CENTER_FILL, WARN_CENTER_FILL_SUGGEST, this));
            }
        }

        if (FillX)
        {
            if (ParentAnchor == UIAnchor.TopCenter || ParentAnchor == UIAnchor.BottomCenter || ParentAnchor == UIAnchor.CenterCenter)
            {
                list.Add(new UIWarning(WARN_CENTER_FILL, WARN_CENTER_FILL_SUGGEST, this));
            }
        }
    }
}
