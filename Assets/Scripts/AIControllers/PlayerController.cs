using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : AController
{
    protected Vector3 desiredDirection = Vector3.zero;

    protected override void Awake()
    {
        base.Awake();
        Game.Instance.AddListener<Adversary>("OnTouchedPlayer", OnTouched);
    }

    //protected void OnDisable()
    //{
    //    Game.Instance.RemoveListener<Adversary>("OnTouchedPlayer", OnTouched);
    //}

    protected void  OnTouched( object sender, object evtData )
    {
        gameObject.SetActive(false);
        Game.Instance.BroadcastEvent<Game>(this, "OnGameOver", null);//Touched by agressive adversary
    }

    protected override void Update()
    {
        base.Update();

        if (desiredDirection != Vector3.zero)
        {
            Vector2Int nextLoc = new Vector2Int(mover.GridLoc.x + (int)desiredDirection.x, mover.GridLoc.y + (int)desiredDirection.z);
            if (!Game.Instance.Grid.TileIsObstructed(nextLoc))
            {
                rotator.DesiredDirection = desiredDirection;
                if (CanChangeDirection)
                {
                    Game.Instance.GridToWorld(nextLoc, out Vector3 desiredWorldPos);
                    mover.DesiredPosition = desiredWorldPos;
                    //desiredDirection = Vector3.zero;
                }
            }
        }
    }

    protected void OnMove(InputValue input)
    {
        OnMove(input.Get<Vector2>());
    }

    public void OnMove(string encodedDir)
    {
        var parts = encodedDir.Split(',');
        OnMove(new Vector2(float.Parse(parts[0].Trim()), float.Parse(parts[1].Trim())));
    }

    public void OnMove(Vector2 dir)
    {
        if (dir == Vector2.zero)
        {
            return;
        }

        var nextDir = Mathf.Abs(dir.x) > Mathf.Abs(dir.y) ? new Vector3(dir.x, 0f, 0f).normalized : new Vector3(0f, 0f, dir.y).normalized;
        desiredDirection = nextDir;

        //Debug.LogFormat("OnMove: {0}", dir);

        Vector2Int nextLoc = new Vector2Int(mover.GridLoc.x + (int)desiredDirection.x, mover.GridLoc.y + (int)desiredDirection.z);
        if (!Game.Instance.Grid.TileIsObstructed(nextLoc))
        {
            rotator.DesiredDirection = desiredDirection;
            if (CanChangeDirection)
            {
                Game.Instance.GridToWorld(nextLoc, out Vector3 desiredWorldPos);
                mover.DesiredPosition = desiredWorldPos;
                //desiredDirection = Vector3.zero;
            }
        }
    }

    public override void OnTriggerEnter(Collider other)
    {
    }

    public override void OnCollisionEnter(Collision collision)
    {
    }
}