﻿using UnityEngine;
using System;

public class MovementController : MonoBehaviour
{
    [HideInInspector]
    protected float linearSpeed;
    public float LinearSpeed {
        get => linearSpeed * Game.Instance.SpeedScale;
        set { linearSpeed = value; }
    }

    [HideInInspector]
    public AController              Controller;

    protected Vector3               desiredPosition;
    public virtual Vector3          DesiredPosition { 
        get => desiredPosition;
        
        set {
            if(desiredPosition == value)
            {
                return;
            }

            desiredPosition = Neo.GridComponent.SnapToGrid(value);
            desiredPosition.y = transform.position.y;
            var delta = desiredPosition - transform.position;
            desiredDirection = delta.normalized;

            Debug.LogFormat("{0} has a desired position of {1}", name, desiredPosition);

            IsAtDestination = false;
        }
    }

    protected Vector3               desiredDirection;

    public Vector3                  InitialPosition { get; protected set; }

    public Vector2Int               GridLoc;
    [HideInInspector]
    public Vector2Int               PrevGridLoc;

    public Action                   OnChangedGridLoc;

    public bool                     IsAtDestination { get; protected set; }

    public virtual float DetermineStepLength()
    {
        if(desiredDirection == Vector3.zero)
        {
            return 0f;
        }

        Ray ray = new Ray(transform.position, desiredDirection);
        bool collides = Physics.Raycast(ray, out RaycastHit info, Neo.GridComponent.TileDiameter, Game.Instance.LayerMaskWall);
        float stepLength = 0f;

        if (!collides)
        {
            stepLength = LinearSpeed * Time.deltaTime;
        }
        else
        {
            float maxStepLength = info.distance - Neo.GridComponent.TileRadius;
            stepLength = Mathf.Min(maxStepLength, LinearSpeed * Time.fixedDeltaTime);
        }

        return stepLength;
    }

    protected virtual void Start()
    {
        transform.position = Neo.GridComponent.SnapToGrid(transform.position);
        DesiredPosition = transform.position;
        InitialPosition = transform.position;
        Game.Instance.WorldToGrid(DesiredPosition, out GridLoc);
    }

    protected virtual void FixedUpdate()
    {
        float stepLength = DetermineStepLength();

        var delta = DesiredPosition - transform.position;
        delta.y = 0f;
        var deltaDist = delta.magnitude;
        var dirToDesiredPos = deltaDist <= 0f ? Vector3.zero : delta / deltaDist;
        stepLength = Mathf.Min(stepLength, deltaDist);

        //(LinearSpeed * 0.007f)
        //LinearSpeed = 1 -> 0.007
        //LinearSpeed = 2 -> 0.014
        //LinearSpeed = 3 -> 0.021
        //We scale our min step length based on speed
        IsAtDestination = stepLength < (LinearSpeed * 0.007f) || desiredDirection == Vector3.zero || Vector3.Dot(dirToDesiredPos, desiredDirection) <= 0.1f;
        
        if (IsAtDestination)
        {
            if (!PrevGridLoc.Equals(GridLoc))
            {
                PrevGridLoc = GridLoc;
                OnChangedGridLoc?.Invoke();
            }
        } else
        {
            transform.position += desiredDirection * stepLength;
        }
        Game.Instance.WorldToGrid(transform.position, out GridLoc);
    }

    public virtual void Stop(bool snap = false)
    {
        if(snap) {
            transform.position = DesiredPosition;
        }
        else
        {
            DesiredPosition = transform.position;
        }
    }
}