using UnityEngine;

public class AdversaryController_GoToJail : AAdversaryController
{
    protected override Path PathToTarget()
    {
        var path = PathTo(mover.InitialPosition);
        path.OnComplete += () => { 
            stateMachine.TriggerEvent("OnPathComplete"); 
        };
        return path;
    }

    public override void OnTriggerEnter(Collider other)
    {
        
    }

    public override void OnCollisionEnter(Collision collision)
    {
        
    }
}