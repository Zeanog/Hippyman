using Neo.StateMachine.Wrappers;
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovementController))]
[RequireComponent(typeof(RotationController))]
public abstract class ACharacter : AAnimatorEventHander
{
    //[SerializeField]
    protected MovementController mover;

    public Action OnChangedGridLoc { get => mover.OnChangedGridLoc; set => mover.OnChangedGridLoc = value; }
    public Action OnRotationComplete { get => rotator.OnRotationComplete; set => rotator.OnRotationComplete = value; }

    //[SerializeField]
    protected RotationController rotator;

    protected AController controller;

    protected override void Awake()
    {
        base.Awake();

        mover = GetComponent<MovementController>();
        rotator = GetComponent<RotationController>();
    }

    public void SetController(AController c)
    {
        if (controller != null)
        {
            mover.Controller = null;
            controller.OnUnassigned();
        }

        controller = c;

        if (controller != null)
        {
            Debug.LogFormat("Setting {0} on {1}...", controller.GetType().Name, name);
            mover.Controller = controller;
            controller.OnAssigned(this);
        }
    }
}

public class Adversary : ACharacter
{
    protected List<GameObject> bodies = new();
    protected int currentBody = 0;
    public void SetBody(int index)
    {
        if (currentBody >= 0 && currentBody < bodies.Count)
        {
            bodies[currentBody].SetActive(false);
        }

        currentBody = index;

        if (currentBody >= 0 && currentBody < bodies.Count)
        {
            bodies[currentBody].SetActive(true);
        }
    }

    protected override void Awake()
    {
        Game.Instance.RegisterAdversary(this);

        base.Awake();

        foreach( Transform child in transform )
        {
            bodies.Add(child.gameObject);
        }
    }
}
