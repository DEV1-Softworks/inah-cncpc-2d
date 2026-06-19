// Pause menu contract. The pause menu is an overlay that freezes the game
// (Time.timeScale = 0) and offers Resume / Return-to-title / Quit options.
// Mirrors the Vendors / Chests / HireOffices locator pattern — anyone in the
// game can ask PauseMenu.IsOpen without coupling to the concrete UI.
public interface IPauseMenu
{
    void Open();
    void Close();
    bool IsOpen { get; }
}

public static class PauseMenu
{
    public static IPauseMenu Active { get; private set; }

    public static event System.Action OnOpened;
    public static event System.Action OnClosed;

    public static void Register(IPauseMenu menu)   => Active = menu;
    public static void Unregister(IPauseMenu menu)
    {
        if (Active == menu) Active = null;
    }

    public static void Open()
    {
        if (Active == null) return;
        bool wasOpen = Active.IsOpen;
        Active.Open();
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
