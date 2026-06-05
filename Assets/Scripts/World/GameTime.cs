using System;

// The day's phases — coarse buckets the rest of the game (NPC schedules,
// shops, lighting, music, events) can react to without caring about the
// exact hour. Boundaries are configurable on TimeService.
public enum DayPhase
{
    Morning,   // dawn -> noon
    Afternoon, // noon -> sunset
    Evening,   // sunset -> nightfall
    Night,     // nightfall -> next dawn
}

// Read-only view onto the game clock. Implemented by TimeService.
public interface ITimeService
{
    int      Day          { get; } // 1-based day counter
    int      Hour         { get; } // 0..23
    int      Minute       { get; } // 0..59
    DayPhase Phase        { get; }

    // 0 = midnight, 0.25 = 06:00, 0.5 = noon, 0.75 = 18:00, 1 = next midnight.
    // Useful for driving a Gradient over the whole day in one read.
    float    DayProgress01 { get; }

    bool     IsPaused     { get; }
    void     Pause();
    void     Resume();
}

// Static locator + transition events. TimeService registers in OnEnable.
// Anyone (light driver, HUD clock, NPC schedule, shop opening rules, ...)
// reads GameTime.* and subscribes to its events without serialized wiring.
//
// Events fire ONLY on real transitions, mirroring the rest of the project's
// locator patterns:
//   OnDayChanged   -> day advanced
//   OnHourChanged  -> hour ticked over
//   OnPhaseChanged -> Morning/Afternoon/Evening/Night boundary crossed
public static class GameTime
{
    public static ITimeService Active { get; private set; }

    public static event Action<int>      OnDayChanged;
    public static event Action<int>      OnHourChanged;
    public static event Action<DayPhase> OnPhaseChanged;

    public static void Register(ITimeService service)   => Active = service;
    public static void Unregister(ITimeService service)
    {
        if (Active == service) Active = null;
    }

    // Invoked by the concrete service when a transition happens.
    public static void NotifyDayChanged(int day)      => OnDayChanged?.Invoke(day);
    public static void NotifyHourChanged(int hour)    => OnHourChanged?.Invoke(hour);
    public static void NotifyPhaseChanged(DayPhase p) => OnPhaseChanged?.Invoke(p);

    // Convenience accessors. Safe before any service is registered — return
    // sensible defaults so a Splash scene that queries time doesn't crash.
    public static int      Day            => Active?.Day            ?? 1;
    public static int      Hour           => Active?.Hour           ?? 6;
    public static int      Minute         => Active?.Minute         ?? 0;
    public static DayPhase Phase          => Active?.Phase          ?? DayPhase.Morning;
    public static float    DayProgress01  => Active?.DayProgress01  ?? 0.25f; // dawn
    public static bool     IsPaused       => Active?.IsPaused       ?? true;
}
