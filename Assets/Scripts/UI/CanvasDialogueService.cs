using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Concrete dialogue service that drives the DialogueUI prefab.
//
// Goes on the DialogueUI prefab ROOT. Wires:
//   _visualRoot       -> the Panel GameObject (the visible dialogue box)
//   _text             -> the body TMP_Text
//   _speakerText      -> the name TMP_Text (optional)
//   _speakerPlate     -> the name-plate background GameObject (optional)
//   _advanceIndicator -> the chevron / "press to continue" glyph (optional)
//   _choicesPanel     -> container shown when the current line has choices (optional)
//   _choiceTexts      -> pre-created TMP_Text rows inside _choicesPanel
//                        (extra slots auto-hide when the current line has
//                         fewer choices)
public class CanvasDialogueService : MonoBehaviour, IDialogueService
{
    [Header("Body")]
    [SerializeField] private GameObject _visualRoot;
    [SerializeField] private TMP_Text   _text;

    [Header("Speaker plate")]
    [SerializeField] private TMP_Text   _speakerText;
    [SerializeField] private GameObject _speakerPlate;

    [Header("Advance indicator")]
    [SerializeField] private GameObject _advanceIndicator;

    [Header("Choices")]
    [SerializeField] private GameObject _choicesPanel;
    [SerializeField] private TMP_Text[] _choiceTexts;

    [Tooltip("Prefix the highlighted choice with this string (e.g. \"> \").")]
    [SerializeField] private string _selectedChoicePrefix = "> ";
    [Tooltip("Prefix non-highlighted choices with this (typically spaces to keep alignment).")]
    [SerializeField] private string _unselectedChoicePrefix = "  ";

    // Playback semantics differ between paginated sequences (signs, captions)
    // and conversation graphs (NPCs):
    //   Sequence    -> a no-choice line advances to the next index (page flip).
    //   Conversation-> a no-choice line is a terminal LEAF (the branch ends).
    // Without this split, a "No-path" terminal line in a conversation would
    // fall through into a "Yes-path" line that happens to sit next in the array.
    private enum PlaybackMode { Sequence, Conversation }

    private IList<DialogueLine> _sequence;
    private int _index;
    private int _selectedChoice;
    private PlaybackMode _mode;

    public bool IsShowing => _visualRoot != null && _visualRoot.activeSelf;

    public bool IsAwaitingChoice =>
        IsShowing
        && _sequence != null
        && _index < _sequence.Count
        && HasChoices(_sequence[_index]);

    private void OnEnable()
    {
        Dialogue.Register(this);
        Hide();
    }

    private void OnDisable() => Dialogue.Unregister(this);

    // Single-line callers go through the same sequence playhead.
    public void Show(string text) => ShowSequence(new[] { new DialogueLine { text = text } });

    public void ShowSequence(IList<DialogueLine> lines)
        => StartPlayback(lines, PlaybackMode.Sequence);

    public void PlayConversation(IList<DialogueLine> lines)
        => StartPlayback(lines, PlaybackMode.Conversation);

    private void StartPlayback(IList<DialogueLine> lines, PlaybackMode mode)
    {
        if (lines == null || lines.Count == 0) { Hide(); return; }
        _sequence = lines;
        _index = 0;
        _selectedChoice = 0;
        _mode = mode;
        DisplayCurrent();
        if (_visualRoot       != null) _visualRoot.SetActive(true);
        if (_advanceIndicator != null) _advanceIndicator.SetActive(true);
    }

    public void Advance()
    {
        if (_sequence == null) return;

        var line = _sequence[_index];
        if (HasChoices(line))
        {
            // Confirm the highlighted choice and jump to its target.
            int target = line.choices[_selectedChoice].targetLine;
            if (target < 0 || target >= _sequence.Count) { Hide(); return; }
            _index = target;
            _selectedChoice = 0;
            DisplayCurrent();
            return;
        }

        // No choices: behavior depends on playback mode.
        if (_mode == PlaybackMode.Conversation)
        {
            // Graph traversal: a no-choice line is a leaf. End the branch
            // instead of falling through to whatever's at index+1.
            Hide();
            return;
        }

        // Linear pagination (signs, captions): advance to next page, or close.
        _index++;
        if (_index >= _sequence.Count) { Hide(); return; }
        DisplayCurrent();
    }

    public void MoveSelection(int delta)
    {
        if (_sequence == null) return;
        var line = _sequence[_index];
        if (!HasChoices(line)) return;

        int n = line.choices.Length;
        _selectedChoice = ((_selectedChoice + delta) % n + n) % n; // wrap, safe for negative delta
        RefreshChoiceHighlights();
    }

    public void Hide()
    {
        _sequence = null;
        _index = 0;
        _selectedChoice = 0;
        if (_visualRoot       != null) _visualRoot.SetActive(false);
        if (_speakerPlate     != null) _speakerPlate.SetActive(false);
        if (_advanceIndicator != null) _advanceIndicator.SetActive(false);
        if (_choicesPanel     != null) _choicesPanel.SetActive(false);
    }

    private void DisplayCurrent()
    {
        if (_sequence == null || _index >= _sequence.Count) return;
        var line = _sequence[_index];

        if (_text != null) _text.text = line.text;

        // Speaker plate: hide whole plate when speaker is empty.
        bool hasSpeaker = !string.IsNullOrEmpty(line.speaker);
        if (_speakerText != null) _speakerText.text = line.speaker;

        GameObject speakerToggle = _speakerPlate != null
            ? _speakerPlate
            : (_speakerText != null ? _speakerText.gameObject : null);
        if (speakerToggle != null && speakerToggle.activeSelf != hasSpeaker)
            speakerToggle.SetActive(hasSpeaker);

        // Choices: populate rows and toggle the panel.
        bool hasChoices = HasChoices(line);
        if (_choicesPanel != null && _choicesPanel.activeSelf != hasChoices)
            _choicesPanel.SetActive(hasChoices);

        if (hasChoices) RefreshChoiceTexts(line);
    }

    private void RefreshChoiceTexts(DialogueLine line)
    {
        if (_choiceTexts == null) return;

        for (int i = 0; i < _choiceTexts.Length; i++)
        {
            var slot = _choiceTexts[i];
            if (slot == null) continue;

            bool inUse = i < line.choices.Length;
            if (slot.gameObject.activeSelf != inUse) slot.gameObject.SetActive(inUse);

            if (inUse)
            {
                string prefix = (i == _selectedChoice) ? _selectedChoicePrefix : _unselectedChoicePrefix;
                slot.text = prefix + line.choices[i].text;
            }
        }
    }

    private void RefreshChoiceHighlights()
    {
        if (_sequence == null) return;
        RefreshChoiceTexts(_sequence[_index]);
    }

    private static bool HasChoices(DialogueLine line) =>
        line.choices != null && line.choices.Length > 0;
}
