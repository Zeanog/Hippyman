using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class RotationController : MonoBehaviour
{
    [SerializeField]
    protected float rotationalSpeed = 1f;
    public float RotationalSpeed => rotationalSpeed * Game.Instance.SpeedScale;

    protected Vector3   desiredDirection = Vector3.zero;
    public virtual Vector3 DesiredDirection { 
        get => desiredDirection;
        set {
            if(desiredDirection == value) return;

            Debug.LogFormat("{0} has a desired direction of {1}", name, desiredDirection);

            desiredDirection = value;
            RotationIsComplete = false;
        }
    }

    protected float         prevDot = 0f;
    public bool             RotationIsComplete { get; protected set; }
    public Action           OnRotationComplete;

    protected virtual void Awake()
    {
        
    }

    protected virtual void Start()
    {
        DesiredDirection = transform.forward;
    }

    protected virtual Quaternion DetermineRotation(float deltaTime)
    {
        var newDir = Vector3.RotateTowards(transform.forward, DesiredDirection, Mathf.Deg2Rad * RotationalSpeed, 1f);
        return Quaternion.LookRotation(newDir, Vector3.up);
    }

    protected virtual void FixedUpdate()
    {
        transform.rotation = DetermineRotation(Time.fixedDeltaTime);

        var curDot = Vector3.Dot(transform.forward, DesiredDirection);
        RotationIsComplete = curDot > 0.95f;
        if (RotationIsComplete && prevDot <= 0.95f)
        {
            OnRotationComplete?.Invoke();
        }
        prevDot = curDot;
    }
}