﻿using UnityEngine;

public class AdversaryController_Flee : AAdversaryController
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected int currentCornerIndex = 0;
    protected int incrementAmount = 0;
    protected override Path PathToTarget()
    {
        incrementAmount = ((incrementAmount + 1) % 2);
        var corners = Game.Instance.Grid.Corners;
        var cornerIndex = (currentCornerIndex + incrementAmount + 1) % corners.Length;
        
        Game.Instance.GridToWorld(corners[cornerIndex], out Vector3 targetPos);
        var newPath = PathTo(targetPos);
        if(newPath == null)
        {
            return null;
        }

        newPath.OnComplete += () =>
        {
            path.Invalidate();
            stateMachine.TriggerEvent("OnPathComplete");
            FindNewPath();
        };

        return newPath;
    }

    public override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Player>() != null)
        {
            stateMachine.TriggerEvent("OnGoToJail");
            stateMachine.TriggerEvent("OnTouchedPlayer");
        }
    }

    public override void OnCollisionEnter(Collision collision)
    {
        var collidee = collision.gameObject.GetComponent<Player>();
        if (collidee != null)
        {
            stateMachine.TriggerEvent("OnGoToJail");
            stateMachine.TriggerEvent("OnTouchedPlayer");
        }
    }
}