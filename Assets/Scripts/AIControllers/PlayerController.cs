using UnityEngine;

public class PlayerController : AController
{
    protected Vector3 desiredDirection = Vector3.zero;
    public override Vector3 DesiredDirection {
        get => desiredDirection;
        protected set {
            if(desiredDirection.Equals(value))
            {
                return;
            }

            desiredDirection = value;
            if(desiredDirection != Vector3.zero)
            {
                Owner.Animator.SetTrigger("StartWalking");
            }
        }
    }

    protected override void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            nextDirection = Vector3.forward;
            Game.Instance.GridToWorld(Owner.GridLoc, out Vector3 worldPos);
            DesiredPosition = worldPos + DesiredDirection * Neo.GridComponent.TileDiameter;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            nextDirection = Vector3.back;
            Game.Instance.GridToWorld(Owner.GridLoc, out Vector3 worldPos);
            DesiredPosition = worldPos + DesiredDirection * Neo.GridComponent.TileDiameter;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            nextDirection = Vector3.right;
            Game.Instance.GridToWorld(Owner.GridLoc, out Vector3 worldPos);
            DesiredPosition = worldPos + DesiredDirection * Neo.GridComponent.TileDiameter;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            nextDirection = Vector3.left;
            Game.Instance.GridToWorld(Owner.GridLoc, out Vector3 worldPos);
            DesiredPosition = worldPos + DesiredDirection * Neo.GridComponent.TileDiameter;
        } else
        {
            //nextDirection = Vector3.zero;
            //Game.Instance.GridToWorld(Owner.GridLoc, out Vector3 worldPos);
            //DesiredPosition = worldPos;
        }

        base.Update();
    }

    public override bool OnOwnerMoved() { return false; }

    public override void OnTriggerEnter(Collider other)
    {
    }

    public override void OnCollisionEnter(Collision collision)
    {
    }
}