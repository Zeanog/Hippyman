using Neo.StateMachine.Wrappers;
using UnityEngine;

public abstract class AAdversaryController : AController
{
    private Path path;
    public Path Path {
        get => path;

        protected set {
            if(path != null)
            {
                path.Dispose();
            }
            path = value;
        }
    }

    public Player Player => Game.Instance.Player;

    [SerializeField]
    protected InspectorStateMachine stateMachine;

    [SerializeField]
    protected LayerMask layer;
    protected LayerMask prevLayer;

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

        prevLayer = owner.gameObject.layer;
        owner.gameObject.layer = (int)Mathf.Log( layer, 2f );

        mover.OnChangedGridLoc += OnOwnerMoved;
        rotator.OnRotationComplete += OnOwnerRotationCompleted;

        Game.Instance.AddListener<Game>("OnGameStart", OnGameStart);
        Game.Instance.AddListener<Game>("OnGameOver", OnGameOver);
        Game.Instance.AddListener<EvidenceofCorruption>("OnCollected", OnEvidenceCollected);
        Game.Instance.AddListener<EvidenceofCorruption>("OnLost", OnEvidenceLost);
        Game.Instance.AddListener<Adversary>("OnGoToJail", OnGoToJail);

        FindNewPath();
    }

    public override void OnUnassigned()
    {
        Path = null;

        owner.gameObject.layer = prevLayer;

        mover.OnChangedGridLoc -= OnOwnerMoved;
        rotator.OnRotationComplete -= OnOwnerRotationCompleted;

        Game.Instance.RemoveListener<Game>("OnGameStart", OnGameStart);
        Game.Instance.RemoveListener<Game>("OnGameOver", OnGameOver);
        Game.Instance.RemoveListener<EvidenceofCorruption>("OnCollected", OnEvidenceCollected);
        Game.Instance.RemoveListener<EvidenceofCorruption>("OnLost", OnEvidenceLost);
        Game.Instance.RemoveListener<Adversary>("OnGoToJail", OnGoToJail);

        base.OnUnassigned();
    }

    protected virtual void OnGameStart(object sender, object evtData)
    {
        stateMachine.TriggerEvent("OnGameStart");
    }

    protected virtual void OnGameOver(object sender, object evtData)
    {
        stateMachine.TriggerEvent("OnGameOver", false);
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
            if (Path != null && Path.DetermineNextDirection(mover.GridLoc, out Vector3 nextPos, out Vector3 nextDir))
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

        if (Path == null || !Path.IsValid)
        {
            FindNewPath();
        }
        else
        {
            if (Path.DetermineNextDirection(mover.GridLoc, out Vector3 nextPos, out Vector3 nextDir))
            {
                rotator.DesiredDirection = nextDir;
                desiredWorldPos = nextPos;
            }
        }
    }

    protected virtual bool FindNewPath()
    {
        if(owner == null)
        {
            return false;
        }

        Path = null;
        
        //Debug.LogFormat("{0} is creating new path for {1}", GetType().Name, owner.name);
        var newPath = PathToTarget();
        if (Path != null)
        {
            Path.Dispose();
        }
        Path = newPath;
        if (Path == null)
        {
            mover.Stop(true);
            return false;
        }

        rotator.DesiredDirection = Path.DetermineFirstDirection(out Vector3 startPos);
        if(desiredWorldPos.Equals(startPos))
        {
            Path.DetermineNextDirection(mover.GridLoc, out startPos, out Vector3 desDir);
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
        var player = other.gameObject.GetComponent<Player>();
        if (player != null)
        {
            stateMachine.TriggerEvent("OnTouchedPlayer");
            Game.Instance.BroadcastEvent<Adversary>(this, "OnTouchedPlayer", null);
        }
    }

    public override void OnCollisionEnter(Collision collision)
    {
        var collidee = collision.gameObject.GetComponent<Player>();
        if (collidee != null)
        {
            stateMachine.TriggerEvent("OnTouchedPlayer");
            Game.Instance.BroadcastEvent<Adversary>(this, "OnTouchedPlayer", null);
        }
    }
}