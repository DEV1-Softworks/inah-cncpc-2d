using UnityEngine;

// The single concrete game clock. Place one in the scene; it registers as
// GameTime.Active and ticks game minutes per real second based on
// _gameSecondsPerRealSecond. Auto-pauses while dialogue or a chest is open
// (toggleable) so time can't sneak past the player during menus.
//
// Sleeping / "end of day" warps are deliberately NOT here — when that system
// arrives it will call into TimeService to jump the clock to the next dawn.
public class TimeService : MonoBehaviour, ITimeService
{
    [Header("Speed")]
    [Tooltip("How many in-game seconds elapse per real second. 60 = 1 real second is 1 in-game minute; 24 real minutes = 1 in-game day.")]
    [SerializeField] private float _gameSecondsPerRealSecond = 60f;

    [Header("Start of run")]
    [SerializeField] private int _startDay    = 1;
    [SerializeField] private int _startHour   = 6;
    [SerializeField] private int _startMinute = 0;

    [Header("Phase boundaries (hour of day)")]
    [SerializeField] private int _morningStart   = 6;
    [SerializeField] private int _afternoonStart = 12;
    [SerializeField] private int _eveningStart   = 18;
    [SerializeField] private int _nightStart     = 22;

    [Header("Pause behaviour")]
    [Tooltip("If on, the clock stops while a blocking overlay (dialogue, chest) is open — same rule the player input handlers use.")]
    [SerializeField] private bool _pauseDuringOverlays = true;

    // Total elapsed in-game seconds since day 0 midnight. double to avoid
    // float drift over long sessions.
    private double _totalSeconds;

    // Last reported values, so we only fire events on real transitions.
    private int      _lastDay;
    private int      _lastHour;
    private DayPhase _lastPhase;

    public int      Day           => (int)(_totalSeconds / 86400.0) + 1;
    public int      Hour          => (int)((_totalSeconds % 86400.0) / 3600.0);
    public int      Minute        => (int)((_totalSeconds %  3600.0) /   60.0);
    public DayPhase Phase         => PhaseAt(Hour);
    public float    DayProgress01 => (float)((_totalSeconds % 86400.0) / 86400.0);

    public bool IsPaused { get; private set; }
    public void Pause()  => IsPaused = true;
    public void Resume() => IsPaused = false;

    private void Awake()
    {
        _totalSeconds = (_startDay - 1) * 86400.0
                      +  _startHour      * 3600.0
                      +  _startMinute    *   60.0;
        _lastDay   = Day;
        _lastHour  = Hour;
        _lastPhase = Phase;
    }

    private void OnEnable()  => GameTime.Register(this);
    private void OnDisable() => GameTime.Unregister(this);

    private void Update()
    {
        bool overlayPause = _pauseDuringOverlays && (Chests.IsOpen || Dialogue.IsShowing);
        if (IsPaused || overlayPause) return;

        _totalSeconds += Time.deltaTime * _gameSecondsPerRealSecond;

        int      d = Day;
        int      h = Hour;
        DayPhase p = Phase;

        if (d != _lastDay)   { _lastDay   = d; GameTime.NotifyDayChanged(d); }
        if (h != _lastHour)  { _lastHour  = h; GameTime.NotifyHourChanged(h); }
        if (p != _lastPhase) { _lastPhase = p; GameTime.NotifyPhaseChanged(p); }
    }

    private DayPhase PhaseAt(int hour)
    {
        if (hour >= _nightStart   || hour < _morningStart) return DayPhase.Night;
        if (hour >= _eveningStart)                          return DayPhase.Evening;
        if (hour >= _afternoonStart)                        return DayPhase.Afternoon;
        return DayPhase.Morning;
    }
}
