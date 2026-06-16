using UnityEngine;

// Lives on an always-active GameObject in a scene that hosts hireable
// experts (typically the INAH camp). At scene start it activates any
// ExpertNpc child whose expertId is already in HiredExperts. It also
// listens for the OnHired event so a hire that happens while the player is
// IN the scene immediately materializes the new NPC.
//
// Hierarchy convention:
//   NpcContainer (always active, has SceneExpertActivator)
//   ├── Romina_Speleobuza  (inactive by default; has ExpertNpc + NpcInteractable)
//   ├── Geologo            (inactive by default; future expert)
//   └── ...
//
// The container being always active is what lets this component subscribe
// to events; if SceneExpertActivator were on the NPC itself, the NPC would
// have to start active (and then visibly blink off) to receive Awake.
public class SceneExpertActivator : MonoBehaviour
{
    private void Start()
    {
        // First pass: activate any NPC whose expert is already hired.
        // Iterate children directly so we don't pay a scene-wide search.
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            var expert = child.GetComponent<ExpertNpc>();
            if (expert == null) continue;

            if (HiredExperts.IsHired(expert.ExpertId) && !child.gameObject.activeSelf)
                child.gameObject.SetActive(true);
        }
    }

    private void OnEnable()  => HiredExperts.OnHired += HandleHired;
    private void OnDisable() => HiredExperts.OnHired -= HandleHired;

    private void HandleHired(string expertId)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            var expert = child.GetComponent<ExpertNpc>();
            if (expert == null) continue;

            if (expert.ExpertId == expertId)
            {
                if (!child.gameObject.activeSelf) child.gameObject.SetActive(true);
                return;
            }
        }
    }
}
