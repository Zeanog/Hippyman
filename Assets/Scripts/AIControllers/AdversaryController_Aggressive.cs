using System;
using UnityEngine;

public class AdversaryController_Aggressive : AAdversaryController_Invulnerable
{
    protected override Path PathToTarget() 
    {
        Path newPath = PathTo(Player);

        Game.Instance.Player.OnChangedGridLoc += newPath.Invalidate;

#if DEBUG
        Neo.Utility.ExceptionUtility.Verify<ArgumentOutOfRangeException>(Game.Instance.Player.OnChangedGridLoc.GetInvocationList().Length <= 3);
#endif

        newPath.OnInvalidation += () =>
        {
            int prevCount = Game.Instance.Player.OnChangedGridLoc?.GetInvocationList().Length ?? 0;
            Game.Instance.Player.OnChangedGridLoc -= newPath.Invalidate;
            int curCount = Game.Instance.Player.OnChangedGridLoc?.GetInvocationList().Length ?? 0;

#if DEBUG
            Neo.Utility.ExceptionUtility.Verify<ArgumentOutOfRangeException>(curCount < prevCount || (curCount == 0 && prevCount == 0));
#endif

            FindNewPath();
        };

        newPath.OnComplete += () => {
            stateMachine.TriggerEvent("OnPathComplete"); 
        };
        return newPath;
    }
}