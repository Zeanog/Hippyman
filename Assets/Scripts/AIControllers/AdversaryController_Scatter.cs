using UnityEngine;

public class AdversaryController_Scatter : AAdversaryController_Invulnerable
{
    [SerializeField]
    protected Neo.GridComponent.ECorner TargetCorner;

    protected override Path PathToTarget()
    {
        var newPath = PathTo(Game.Instance.Grid.Corners[(int)TargetCorner]);
        newPath.OnComplete += () =>
        {
            stateMachine.TriggerEvent("OnPathComplete");
        };
        return newPath;
    }
}