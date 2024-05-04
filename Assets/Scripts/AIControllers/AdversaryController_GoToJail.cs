using UnityEngine;

public class AdversaryController_GoToJail : AAdversaryController
{
    protected override Path PathToTarget()
    {
        var path = PathTo(Owner.InitialPosition);
        path.OnComplete += () => { 
            path.Invalidate(); 
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