using TMPro;
using UnityEngine;

// Small HUD readout that displays the player's current peso balance and
// updates live when the wallet changes. Pure listener: it reads through the
// Wallet locator and never writes.
//
// Place on a TMP_Text inside a HUD canvas (under Persistent so it survives
// scene transitions). The format string can be customized in the inspector;
// the default "🪙 {0:N0} pesos" prints "🪙 1,250 pesos" once the locale uses
// thousand separators.
public class WalletHudView : MonoBehaviour
{
    [SerializeField] private TMP_Text _label;

    [Tooltip("How the value is rendered. {0} is the peso amount. Use {0:N0} for thousand separators (\"1,250\").")]
    [SerializeField] private string _format = "{0:N0} pesos";

    private void OnEnable()
    {
        Wallet.OnChanged += HandleChanged;
        // Paint the current value once on enable in case Wallet was set
        // before this view woke up.
        Repaint(Wallet.Pesos);
    }

    private void OnDisable()
    {
        Wallet.OnChanged -= HandleChanged;
    }

    private void HandleChanged(int newBalance) => Repaint(newBalance);

    private void Repaint(int amount)
    {
        if (_label == null) return;
        _label.text = string.Format(_format, amount);
    }
}
