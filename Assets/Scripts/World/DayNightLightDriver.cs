using UnityEngine;
using UnityEngine.Rendering.Universal;

// Drives a URP 2D global Light2D's color and intensity over the in-game day.
// Reads GameTime.DayProgress01 (0 at midnight, 1 at the next midnight) and
// samples the configured Gradient + AnimationCurve once per frame.
//
// Wire (Inspector):
//   _light     -> the Global Light2D affecting the world (auto-grabbed from
//                 this GameObject if you put the script on the light itself)
//   _color     -> Gradient sampled over t = DayProgress01.
//                 Suggested stops: 0.00 cool blue, 0.25 warm orange (dawn),
//                 0.50 near-white (noon), 0.75 warm orange (dusk),
//                 1.00 cool blue (back to night).
//   _intensity -> AnimationCurve sampled over the same t.
//                 Suggested: 0.0 -> 0.3, 0.25 -> 0.9, 0.5 -> 1.0,
//                            0.75 -> 0.7, 1.0 -> 0.3
//
// Requires sprites to use a Lit shader material to actually respond to the
// light. World tiles painted with the default Unlit sprite material will
// still ignore it.
public class DayNightLightDriver : MonoBehaviour
{
    [SerializeField] private Light2D         _light;
    [SerializeField] private Gradient        _color;
    [SerializeField] private AnimationCurve  _intensity = AnimationCurve.Linear(0f, 0.3f, 1f, 0.3f);

    private void Awake()
    {
        if (_light == null) _light = GetComponent<Light2D>();
    }

    private void Update()
    {
        if (_light == null) return;
        float t = GameTime.DayProgress01;
        _light.color     = _color.Evaluate(t);
        _light.intensity = _intensity.Evaluate(t);
    }
}
