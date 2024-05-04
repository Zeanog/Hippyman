using UnityEngine;

public abstract class AController : MonoBehaviour
{
    public virtual Vector3 DesiredDirection { get; protected set; }

    [SerializeField]
    public ControlledAnimatedObject Owner;

    public virtual void OnAssigned() {  }
    public virtual void OnUnassigned() { }

    public abstract bool OnOwnerMoved();

    public virtual float DetermineStepLength()
    {
        Ray ray = new Ray(Owner.transform.position, Owner.DesiredDirection);
        bool collides = Physics.Raycast(ray, out RaycastHit info, Neo.Grid.TileDiameter, Game.Instance.LayerMaskWall);
        float stepLength = 0f;

        if (!collides)
        {
            stepLength = Owner.LinearSpeed * Time.deltaTime;
        }
        else
        {
            float maxStepLength = info.distance - Neo.Grid.TileRadius;
            stepLength = Mathf.Min(maxStepLength, Owner.LinearSpeed * Time.fixedDeltaTime);
        }

        return stepLength;
    }

    public static bool CanChangeDirection(Vector3 pos)
    {
        var snappedPos = Neo.Grid.SnapToGrid(pos);
        var deltaX = Mathf.Abs(snappedPos.x - pos.x);
        var deltaY = Mathf.Abs(snappedPos.z - pos.z);
        return deltaX <= 0.1f && deltaY <= 0.1f;
    }

    public abstract void OnTriggerEnter(Collider other);
    public abstract void OnCollisionEnter(Collision collision);
}