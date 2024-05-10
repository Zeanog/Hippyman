using System;
using System.Collections.Generic;
using UnityEngine;

public class ControlledAnimatedObject : AAnimatorEventHander
{
    [SerializeField]
    protected float linearSpeed = 1f; // in meters per second
    public float LinearSpeed => linearSpeed;

    [SerializeField]
    protected float rotationalSpeed = 1f;
    public float RotationalSpeed => rotationalSpeed;

    public bool ReachedAWall { get; protected set; }

    public AController Controller {
        get;
        protected set;
    } = null;

    public Vector3 InitialPosition { get; protected set; }

    public virtual Vector3 DesiredDirection => Controller?.DesiredDirection ?? Vector3.zero;

    public virtual Vector3  DesiredPosition => Controller?.DesiredPosition ?? Vector3.zero;

    public Vector2Int GridLoc;
    [HideInInspector]
    public Vector2Int PrevGridLoc;

    public Action OnMoved;

    protected List<GameObject> bodies = new();
    protected int currentBody = 0;
    public void SetBody(int index)
    {
        if(currentBody >= 0 && currentBody < bodies.Count)
        {
            bodies[currentBody].SetActive(false);
        }

        currentBody = index;

        if (currentBody >= 0 && currentBody < bodies.Count)
        {
            bodies[currentBody].SetActive(true);
        }
    }

    public void SetController(AController c)
    {
        if (Controller != null)
        {
            Controller.OnUnassigned();
        }

        Controller = c;

        if (Controller != null)
        {
            Controller.OnAssigned();
        }
    }

    protected override void Awake()
    {
        base.Awake();

        OnMoved += () => {
            if (Controller != null && Controller.OnOwnerMoved())
            {
            }
        };

        AddHandler("Walking.Enter", OnWalkingEnter);
        AddHandler("WalkingTurn180.Enter", OnTurnAroundEnter);
        AddHandler("RunningTurn180.Enter", OnTurnAroundEnter);
        AddHandler("WalkingTurn180.Exit", OnTurnAroundExit);
        AddHandler("RunningTurn180.Exit", OnTurnAroundExit);
    }

    protected virtual void Start()
    {
        InitialPosition = transform.position;
    }

    protected virtual Quaternion DetermineRotation(float deltaTime)
    {
        var newDir = Vector3.RotateTowards(transform.forward, DesiredDirection, Mathf.Deg2Rad * RotationalSpeed, 1f);
        return Quaternion.LookRotation(newDir, Vector3.up);
    }

    protected void OnWalkingEnter(string evtName)
    {
        //DesiredDirection = targetDesiredDirection;
    }

    protected void OnTurnAroundEnter(string evtName)
    {
        //DesiredDirection = Vector3.zero;
    }

    protected void  OnTurnAroundExit(string evtName)
    {
        Animator.SetBool("TurningAround", false);
        //DesiredDirection = targetDesiredDirection;
        transform.rotation = Quaternion.LookRotation(DesiredDirection, Vector3.up);
        //DesiredDirection = Vector3.zero;

    }

    protected void Update()
    {
        //bool forceDirectionUpdate = DesiredDirection == Vector3.zero && !targetDesiredDirection.Equals(DesiredDirection);
        //bool canUpdate = Controller.CanChangeDirection(transform.position) && !targetDesiredDirection.Equals(DesiredDirection)/* && !Animator.GetBool("TurningAround")*/;
        //if(forceDirectionUpdate || canUpdate)
        //{
        //    Vector3 newDir = targetDesiredDirection;
        //    newDir.y = 0.0f;

        //    if (newDir.x != 0 && newDir.z != 0)
        //    {
        //        Debug.LogError("Must move only on cardinal directions. Desired direction was ignored");
        //        newDir = Vector3.zero;
        //    }

        //    DesiredDirection = newDir;
        //}
    }

    protected virtual void FixedUpdate()
    {
        float stepLength = Controller?.DetermineStepLength() ?? 0f;

        ReachedAWall = stepLength <= Neo.GridComponent.TileEpsilon;

        if (DesiredDirection != Vector3.zero)
        {
            //if (!this.Animator.GetBool("TurningAround"))
            //{
            transform.rotation = DetermineRotation(Time.fixedDeltaTime);
            //}
            //transform.rotation = Quaternion.LookRotation(DesiredDirection, Vector3.up);
            if (Vector3.Dot(transform.forward, DesiredDirection) >= 0.999f)
            {
                transform.rotation = Quaternion.LookRotation(DesiredDirection, Vector3.up);//Avoid any floating pt error
                transform.position += DesiredDirection * stepLength;
            }
        }

        Game.Instance.WorldToGrid(transform.position, out GridLoc);
        if (!PrevGridLoc.Equals(GridLoc) && Controller != null && Controller.CanChangeDirection(transform.position))
        {
            PrevGridLoc = GridLoc;
            OnMoved?.Invoke();
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        Controller?.OnTriggerEnter(other);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        Controller?.OnCollisionEnter(collision);
    }
}