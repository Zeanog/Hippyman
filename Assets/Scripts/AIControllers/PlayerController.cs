using UnityEngine;

public class PlayerController : AController
{
    protected Vector3 desiredDirection = Vector3.zero;

    protected override void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            desiredDirection = Vector3.forward;
        }
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            desiredDirection = Vector3.back;
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            desiredDirection = Vector3.right;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            desiredDirection = Vector3.left;
        } else
        {
            desiredDirection = Vector3.zero;
        }

        base.Update();

        var nextLoc = new Vector2Int( mover.GridLoc.x + (int)desiredDirection.x, mover.GridLoc.y + (int)desiredDirection.z );
        if ( desiredDirection != Vector3.zero && !Game.Instance.Grid.TileIsObstructed(nextLoc))
        {
            rotator.DesiredDirection = desiredDirection;
            if (CanChangeDirection)
            {
                Game.Instance.GridToWorld(nextLoc, out Vector3 desiredWorldPos);
                mover.DesiredPosition = desiredWorldPos;
                desiredDirection = Vector3.zero;
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