using System.Collections.Generic;

// Hire office UI contract. A HireOfficeInteractable calls HireOffices.Open()
// with a session bundling the roster to show. Mirrors the Vendors / Chests /
// ResearchSponsors patterns: locator + transition events.
public interface IHireOfficeUI
{
    void Open(HireOfficeSession session);
    void Close();
    bool IsOpen { get; }
}

// One opening of the hire desk. Bundles the title and the roster the UI
// renders. Other future fields (filter by tag, sort order, etc.) live here.
public class HireOfficeSession
{
    public string                Title;
    public IList<ExpertSpecialist> Experts;
}

public static class HireOffices
{
    public static IHireOfficeUI Active { get; private set; }

    public static event System.Action OnOpened;
    public static event System.Action OnClosed;

    public static void Register(IHireOfficeUI ui)   => Active = ui;
    public static void Unregister(IHireOfficeUI ui)
    {
        if (Active == ui) Active = null;
    }

    public static void Open(HireOfficeSession session)
    {
        if (Active == null || session == null) return;
        bool wasOpen = Active.IsOpen;
        Active.Open(session);
        if (!wasOpen && Active.IsOpen) OnOpened?.Invoke();
    }

    public static void Close()
    {
        if (Active == null) return;
        bool wasOpen = Active.IsOpen;
        Active.Close();
        if (wasOpen && !Active.IsOpen) OnClosed?.Invoke();
    }

    public static bool IsOpen => Active?.IsOpen ?? false;
}
