using System;
using UnityEngine;

public class AdversaryController_Aggressive : AAdversaryController_Invulnerable
{
    protected override Path PathToTarget() 
    {
        Path newPath = PathTo(Player);
        if(newPath == null)
        {
            return null;
        }

        var p = newPath;
        Game.Instance.Player.OnChangedGridLoc += p.Invalidate;

#if DEBUG
        Neo.Utility.ExceptionUtility.Verify<ArgumentOutOfRangeException>(Game.Instance.Player.OnChangedGridLoc.GetInvocationList().Length <= Game.Instance.AdversaryCount);
#endif

        newPath.OnDestroy += () => {
            Game.Instance.Player.OnChangedGridLoc -= p.Invalidate;
        };

        newPath.OnInvalidation += () =>
        {
#if DEBUG
            int prevCount = Game.Instance.Player.OnChangedGridLoc?.GetInvocationList().Length ?? 0;
#endif

            Game.Instance.Player.OnChangedGridLoc -= p.Invalidate;

#if DEBUG
            int curCount = Game.Instance.Player.OnChangedGridLoc?.GetInvocationList().Length ?? 0;
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