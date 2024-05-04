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

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            DesiredDirection = Vector3.forward;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            DesiredDirection = Vector3.back;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            DesiredDirection = Vector3.right;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            DesiredDirection = Vector3.left;
        } else
        {
            //DesiredDirection = Vector3.zero;
        }
    }

    public override bool OnOwnerMoved() { return false; }

    public override void OnTriggerEnter(Collider other)
    {         
    }

    public override void OnCollisionEnter(Collision collision)
    {
    }
}