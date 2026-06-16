using System.Collections.Generic;
using UnityEngine;

// World-side vendor (commerciante). When interacted with, opens the vendor
// UI in the configured mode. Drop on the comerciante NPC's GameObject (or on
// a separate stall) with a trigger collider so the PlayerInteractor picks it
// up.
//
// One vendor can be SellOnly (player only sells goods), BuyOnly (vendor only
// offers stock to buy), or Both (typical case — comerciante that takes your
// firewood AND sells you seeds).
public class VendorInteractable : MonoBehaviour, IInteractable
{
    public enum VendorMode { SellOnly, BuyOnly, Both }

    [Tooltip("Shown in the modal header. Usually the NPC's name (\"Don Hilario\") or the stall (\"Tienda del pueblo\").")]
    [SerializeField] private string _vendorName = "Comerciante";

    [SerializeField] private VendorMode _mode = VendorMode.Both;

    [Tooltip("Items offered by this vendor for the player to buy. Only used when mode is BuyOnly or Both. Each item must have BuyPrice > 0 to appear.")]
    [SerializeField] private List<Item> _stock = new();

    public void Interact()
    {
        var session = new VendorSession
        {
            VendorName = _vendorName,
            CanSell    = _mode == VendorMode.SellOnly || _mode == VendorMode.Both,
            CanBuy     = _mode == VendorMode.BuyOnly  || _mode == VendorMode.Both,
            Stock      = _stock,
        };

        Vendors.Open(session);
    }
}
