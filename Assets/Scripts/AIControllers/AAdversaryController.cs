using Neo.StateMachine.Wrappers;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class AAdversaryController : AController
{
    protected Path path;

    public Player Player { get => Game.Instance.Player; }

    [SerializeField]
    protected InspectorStateMachine stateMachine;

    protected override void Start()
    {
        base.Start();
    }

    protected Vector3 desiredWorldPos;

    protected override void Update()
    {
        base.Update();

        if (desiredWorldPos != Vector3.zero && desiredWorldPos != mover.DesiredPosition && CanChangeDirection)
        {
            mover.DesiredPosition = desiredWorldPos;
            desiredWorldPos = Vector3.zero;
        }
    }

    public override void OnAssigned(ACharacter character)
    {
        base.OnAssigned(character);

        mover.OnChangedGridLoc += OnOwnerMoved;
        rotator.OnRotationComplete += OnOwnerRotationCompleted;

        if (path != null)
        {
            path.Invalidate();
            path = null;
        }

        Game.Instance.AddListener<Game>("OnGameStart", OnGameStart);
        Game.Instance.AddListener<EvidenceofCorruption>("OnCollected", OnEvidenceCollected);
        Game.Instance.AddListener<EvidenceofCorruption>("OnLost", OnEvidenceLost);
        Game.Instance.AddListener<Adversary>("OnGoToJail", OnGoToJail);

        FindNewPath();
    }

    public override void OnUnassigned()
    {
        base.OnUnassigned();

        mover.OnChangedGridLoc -= OnOwnerMoved;
        rotator.OnRotationComplete -= OnOwnerRotationCompleted;

        Game.Instance.RemoveListener<Game>("OnGameStart", OnGameStart);
        Game.Instance.RemoveListener<EvidenceofCorruption>("OnCollected", OnEvidenceCollected);
        Game.Instance.RemoveListener<EvidenceofCorruption>("OnLost", OnEvidenceLost);
        Game.Instance.RemoveListener<Adversary>("OnGoToJail", OnGoToJail);

        if (path != null)
        {
            path.Destroy();
            path = null;
        }
    }

    protected virtual void OnGameStart(object sender, object evtData)
    {
        stateMachine.TriggerEvent("OnGameStart");
    }

    protected virtual void OnGoToJail(object sender, object evtData)
    {
        stateMachine.TriggerEvent("OnGoToJail");
    }

    protected virtual void OnEvidenceLost(object sender, object evtData)
    {
        stateMachine.TriggerEvent("OnEvidenceLost");
    }

    protected virtual void OnEvidenceCollected(object sender, object evtData)
    {
        stateMachine.TriggerEvent("OnEvidenceCollected");
    }

    protected void OnOwnerRotationCompleted()
    {
        if(mover.IsAtDestination)
        {
            if (path != null && path.DetermineNextDirection(mover.GridLoc, out Vector3 nextPos, out Vector3 nextDir))
            {
                rotator.DesiredDirection = nextDir;
                desiredWorldPos = nextPos;
            }
        }
    }

    protected void OnOwnerMoved()
    {
        if (!Player.gameObject.activeInHierarchy)
        {
            //rotator.DesiredDirection = Vector3.zero;
            desiredWorldPos = Vector3.zero;
            return;
        }

        //if (path != null && path.IsComplete(mover.transform.position) && path.IsValid)
        //{
        //    path.Invalidate();
        //    //rotator.DesiredDirection = Vector3.zero;
        //    desiredWorldPos = Vector3.zero;
        //    return;
        //}

        if (path == null || !path.IsValid)
        {
            FindNewPath();
        }
        else
        {
            if (path.DetermineNextDirection(mover.GridLoc, out Vector3 nextPos, out Vector3 nextDir))
            {
                rotator.DesiredDirection = nextDir;
                desiredWorldPos = nextPos;
            }
        }
    }

    protected virtual bool FindNewPath()
    {
        if (path != null && path.IsValid)
        {
            path.Destroy();
        }

        if(owner == null)
        {
            return false;
        }

        Debug.LogFormat("Creating new path for {0}", owner.name);
        path = PathToTarget();
        if (path == null)
        {
            return false;
        }

        rotator.DesiredDirection = path.DetermineFirstDirection(out Vector3 startPos);
        if(desiredWorldPos.Equals(startPos))
        {
            path.DetermineNextDirection(mover.GridLoc, out startPos, out Vector3 desDir);
            rotator.DesiredDirection = desDir;
        }
        desiredWorldPos = startPos;
        return true;
    }

    protected Path PathTo( GameObject go )
    {
        return Game.Instance.FindIfPathExists(mover.transform.position, go.transform.position);
    }

    protected Path PathTo(MonoBehaviour mb)
    {
        return Game.Instance.FindIfPathExists(mover.transform.position, mb.transform.position);
    }

    protected Path PathTo(Vector3 pos)
    {
        return Game.Instance.FindIfPathExists(mover.transform.position, pos);
    }

    protected Path PathTo(Vector2Int loc)
    {
        return Game.Instance.FindIfPathExists(mover.GridLoc, loc);
    }

    protected abstract Path PathToTarget();
}

public abstract class AAdversaryController_Invulnerable : AAdversaryController {
    public override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Player>() != null)
        {
            // We caught the player
            other.gameObject.SetActive(false);
            stateMachine.TriggerEvent("OnTouchedPlayer");
        }
    }

    public override void OnCollisionEnter(Collision collision)
    {
        var collidee = collision.gameObject.GetComponent<Player>();
        if (collidee != null)
        {
            // We caught the player
            collidee.gameObject.SetActive(false);
            stateMachine.TriggerEvent("OnTouchedPlayer");
        }
    }
}