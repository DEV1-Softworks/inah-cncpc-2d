using UnityEngine;

// Tag component placed on a hireable specialist's NPC GameObject in the
// scene. Carries the expertId that matches a HiredExperts entry; the
// SceneExpertActivator uses it to decide whether to enable this NPC.
//
// The NPC also has its usual NpcInteractable + Conversation so the player
// can talk to it once it's activated. ExpertNpc itself contributes no
// behavior beyond the id; it's data on a component for the activator to
// query.
public class ExpertNpc : MonoBehaviour
{
    [Tooltip("Matches an ExpertSpecialist.ExpertId. Keep unique across the scene.")]
    [SerializeField] private string _expertId;

    public string ExpertId => _expertId;
}
