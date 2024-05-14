using System;
using UnityEngine;

public abstract class AController : MonoBehaviour
{
    [SerializeField]
    protected MovementController mover;

    [SerializeField]
    protected RotationController rotator;

    protected ACharacter    owner;

    protected virtual void Awake()
    {
        
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        
    }

    public bool     IsFacingForward {
        get => rotator.RotationIsComplete;
    }

    public virtual bool CanChangeDirection
    {
        get => mover.IsAtDestination && rotator.RotationIsComplete;
    }

    public virtual void OnAssigned( ACharacter character ) { owner = character; }
    public virtual void OnUnassigned() { owner = null; }

    public abstract void OnTriggerEnter(Collider other);
    public abstract void OnCollisionEnter(Collision collision);
}