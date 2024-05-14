using System;
using UnityEngine;

public class AdversaryController_Aggressive : AAdversaryController_Invulnerable
{
    protected override Path PathToTarget() 
    {
        var newPath = PathTo(Player);

        Game.Instance.Player.OnChangedGridLoc += newPath.Invalidate;

        newPath.OnInvalidation += () =>
        {
            Game.Instance.Player.OnChangedGridLoc -= newPath.Invalidate;
            FindNewPath();
        };

        newPath.OnComplete += () => {
            newPath.Invalidate();
            stateMachine.TriggerEvent("OnPathComplete"); 
        };
        return newPath;
    }
}