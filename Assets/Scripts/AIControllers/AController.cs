using UnityEngine;

public abstract class AController : MonoBehaviour
{
    protected Vector3 nextDirection;
    public virtual Vector3 DesiredDirection { get; protected set; }
    public virtual Vector3 DesiredPosition { get; protected set; }

    [SerializeField]
    public ControlledAnimatedObject Owner;

    protected virtual void Awake()
    {
        DesiredPosition = Owner.transform.position;
        DesiredDirection = Vector3.zero;
        nextDirection = Vector3.zero;
    }

    protected virtual void Update()
    {
        if(CanChangeDirection(Owner.transform.position) && !DesiredDirection.Equals(nextDirection))
        {
            DesiredDirection = nextDirection;
        }
    }

    public virtual void OnAssigned() {  }
    public virtual void OnUnassigned() { }

    public abstract bool OnOwnerMoved();

    public virtual float DetermineStepLength()
    {
        Ray ray = new Ray(Owner.transform.position, Owner.DesiredDirection);
        bool collides = Physics.Raycast(ray, out RaycastHit info, Neo.GridComponent.TileDiameter, Game.Instance.LayerMaskWall);
        float stepLength = 0f;

        if (!collides)
        {
            stepLength = Owner.LinearSpeed * Time.deltaTime;
        }
        else
        {
            float maxStepLength = info.distance - Neo.GridComponent.TileRadius;
            stepLength = Mathf.Min(maxStepLength, Owner.LinearSpeed * Time.fixedDeltaTime);
        }

        return stepLength;
    }

    public virtual bool CanChangeDirection(Vector3 pos)
    {
        var snappedPos = Neo.GridComponent.SnapToGrid(pos);
        var deltaPos = snappedPos - pos;

        var dist = deltaPos.magnitude;
        return dist < 0.1f;
    }

    public abstract void OnTriggerEnter(Collider other);
    public abstract void OnCollisionEnter(Collision collision);
}