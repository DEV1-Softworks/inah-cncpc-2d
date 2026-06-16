using System.Collections.Generic;

// Vendor UI contract. A VendorInteractable calls Vendors.Open(...) with the
// configuration of the vendor (sell mode, buy mode, what items are in stock).
// Mirrors the IChestUI / Chests pattern that the rest of the project uses.
public interface IVendorUI
{
    void Open(VendorSession session);
    void Close();
    bool IsOpen { get; }
}

// One opening of the vendor. Bundles everything the UI needs to render the
// modal: the modes enabled, the vendor's stock (for buy mode), and the
// header text. The vendor name doubles as the header.
public class VendorSession
{
    public string         VendorName;
    public bool           CanSell;
    public bool           CanBuy;
    public IList<Item>    Stock; // items the vendor offers when CanBuy is true
}

// Static locator + transition events. Same shape as Chests / Dialogue —
// systems that need to know "is a vendor open?" subscribe here.
public static class Vendors
{
    public static IVendorUI Active { get; private set; }

    public static event System.Action OnOpened;
    public static event System.Action OnClosed;

    public static void Register(IVendorUI ui)   => Active = ui;
    public static void Unregister(IVendorUI ui)
    {
        if (Active == ui) Active = null;
    }

    public static void Open(VendorSession session)
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
