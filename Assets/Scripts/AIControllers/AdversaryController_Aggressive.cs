using UnityEngine;

public class AdversaryController_Aggressive : AAdversaryController_Vulnerable
{
    protected override Path PathToTarget() 
    {
        var path = PathTo(Player);
        path.OnComplete += () => { path.Invalidate(); stateMachine.TriggerEvent("OnPathComplete"); };
        return path;
    }
}