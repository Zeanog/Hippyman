using System.Collections.Generic;

public class Pellet : AValuePickup<float>
{
    protected static LinkedList<Pellet> pellets = new LinkedList<Pellet>();
    protected static void Register( Pellet p )
    {
        pellets.AddLast(p);
    }

    protected static void Unregister(Pellet p)
    {
        pellets.Remove(p);
    }

    public static int NumPellets => pellets.Count;

    protected override void Awake()
    {
        Register(this);

        onCollected += () => {
            Game.Instance.BroadcastEvent(this, "OnCollected", value);
        };

        base.Awake();
    }

    protected void OnDestroy()
    {
        //TODO: This wont work if we decide to not destroy pellets
        Unregister(this);
    }
}