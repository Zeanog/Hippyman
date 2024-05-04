public class Money : AValuePickup<int>
{
    protected override void Awake()
    {
        onCollected += () => {
            Game.Instance.BroadcastEvent(this, "OnCollected", value);
        };

        base.Awake();
    }
}