using UnityEngine;

public class EvidenceofCorruption : APickup
{
    [SerializeField]
    protected float effectDuration = 4f;

    protected override void Awake()
    {
        base.Awake();

        onCollected += () => {
            Game.Instance.BroadcastEvent(this, "OnCollected", effectDuration);
        };
    }
}