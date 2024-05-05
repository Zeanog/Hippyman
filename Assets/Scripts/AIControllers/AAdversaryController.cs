using Neo.StateMachine.Wrappers;
using UnityEngine;

public abstract class AAdversaryController : AController
{
    protected Path path;

    public Player Player { get => Game.Instance.Player; }

    [SerializeField]
    protected InspectorStateMachine stateMachine;

    protected virtual void Awake()
    {
        
    }

    public override void OnAssigned()
    {
        base.OnAssigned();

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

        Game.Instance.RemoveListener<Game>("OnGameStart", OnGameStart);
        Game.Instance.RemoveListener<EvidenceofCorruption>("OnCollected", OnEvidenceCollected);
        Game.Instance.RemoveListener<EvidenceofCorruption>("OnLost", OnEvidenceLost);
        Game.Instance.RemoveListener<Adversary>("OnGoToJail", OnGoToJail);

        if (path != null)
        {
            path.Invalidate();
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

    public override bool OnOwnerMoved()
    {
        if (!Player.gameObject.activeInHierarchy)
        {
            DesiredDirection = Vector3.zero;
            return true;
        }

        if (path != null && path.IsComplete(Owner.transform.position) && path.IsValid)
        {
            path.Invalidate();
            DesiredDirection = Vector3.zero;
            return true;
        }

        if (path == null || !path.IsValid)
        {
            FindNewPath();
        }
        else
        {
            bool success = path.DetermineNextDirection(Owner.GridLoc, out Vector3 nextDir);
            DesiredDirection = success ? nextDir : Vector3.zero;
        }

        return true;
    }

    protected virtual bool FindNewPath()
    {
        if (path != null)
        {
            path.Invalidate();
        }

        Owner.DesiredDirection = Vector3.zero;//Stop Owner from moving until new path is followed
        path = PathToTarget();
        if (path == null)
        {
            return false;
        }

        DesiredDirection = path.DetermineFirstDirection(out Vector3 startPos);
        return true;
    }

    protected Path PathTo( GameObject go )
    {
        return Game.Instance.FindIfPathExists(Owner.transform.position, go.transform.position);
    }

    protected Path PathTo(MonoBehaviour mb)
    {
        return Game.Instance.FindIfPathExists(Owner.transform.position, mb.transform.position);
    }

    protected Path PathTo(Vector3 pos)
    {
        return Game.Instance.FindIfPathExists(Owner.transform.position, pos);
    }

    protected Path PathTo(Vector2Int loc)
    {
        return Game.Instance.FindIfPathExists(Owner.GridLoc, loc);
    }

    protected abstract Path PathToTarget();

    public override float DetermineStepLength()
    {
        float stepLength = base.DetermineStepLength();

        if (path == null || path.IsComplete())
        {
            return stepLength;
        }
        if (!path.FindNode(Owner.GridLoc, out Path.Node node))
        {
            return 0f;
        }

        Game.Instance.GridToWorld(node.GridLocation, out Vector3 worldPos);
        var deltaPos = worldPos - Owner.transform.position;

        return Mathf.Min(stepLength, deltaPos.magnitude);
    }
}

public abstract class AAdversaryController_Vulnerable : AAdversaryController {
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