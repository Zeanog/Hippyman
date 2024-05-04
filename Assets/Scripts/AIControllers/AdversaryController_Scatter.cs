using UnityEngine;

public class AdversaryController_Scatter : AAdversaryController_Vulnerable
{
    [SerializeField]
    protected Neo.Grid.ECorner TargetCorner;

    protected override Path PathToTarget()
    {
        var newPath = PathTo(Game.Instance.Grid.Corners[(int)TargetCorner]);
        newPath.OnComplete += () =>
        {
            path.Invalidate();
            stateMachine.TriggerEvent("OnPathComplete");
        };
        return newPath;
    }
}