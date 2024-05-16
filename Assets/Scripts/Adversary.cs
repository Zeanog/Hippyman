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
    //[SerializeField]
    protected RotationController rotator;

    public float LinearSpeed {
        get => mover.LinearSpeed;
        set => mover.LinearSpeed = value;
    }

    public Action OnChangedGridLoc { get => mover.OnChangedGridLoc; set => mover.OnChangedGridLoc = value; }
    public Action OnRotationComplete { get => rotator.OnRotationComplete; set => rotator.OnRotationComplete = value; }

    public AController Controller { get; protected set; }

    protected override void Awake()
    {
        base.Awake();

        mover = GetComponent<MovementController>();
        rotator = GetComponent<RotationController>();
    }

    public void SetController(AController c)
    {
        if (Controller != null)
        {
            mover.Controller = null;
            Controller.OnUnassigned();
        }

        Controller = c;

        if (Controller != null)
        {
            Debug.LogFormat("Setting {0} on {1}...", Controller.GetType().Name, name);
            mover.Controller = Controller;
            Controller.OnAssigned(this);
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        Controller?.OnTriggerEnter(other);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        Controller?.OnCollisionEnter(collision);
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
