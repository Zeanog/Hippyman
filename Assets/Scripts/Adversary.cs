﻿using Neo.StateMachine.Wrappers;
using UnityEngine;

public class Adversary : ControlledAnimatedObject
{
    [SerializeField]
    protected InspectorStateMachine stateMachine;

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
