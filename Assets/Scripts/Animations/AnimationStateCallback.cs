using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;

public interface IAnimatorEventHandler
{
    void OnAnimEvent(string evtName);
}

public abstract class AAnimatorEventHander : MonoBehaviour, IAnimatorEventHandler
{
    public Animator Animator { get; protected set; }
    protected Dictionary<string, Action<string>> animCallbackHandlers = new Dictionary<string, Action<string>>();

    protected virtual void Awake()
    {
        Animator = GetComponentInChildren<Animator>();
    }

    public void AddHandler( string evtName, Action<string> handler )
    {
        animCallbackHandlers.Add( evtName, handler );
    }

    public void RemoveHandler(string evtName, Action<string> handler )
    {
        try
        {
            if (!animCallbackHandlers.TryGetValue(evtName, out Action<string> del))
            {
                return;
            }

            del -= handler;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public virtual void OnAnimEvent( string evtName )
    {
        try
        {
            if(!animCallbackHandlers.TryGetValue(evtName, out Action<string> handler))
            {
                return;
            }

            handler?.Invoke(evtName);
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
        }
    }
} 

public class AnimationStateCallback : StateMachineBehaviour
{
    [SerializeField]
    protected string stateName;

    [SerializeField]
    protected bool onEnter;
    protected string stateEnterEventName = "Enter";

    [SerializeField]
    protected bool onUpdate;
    protected string stateUpdateEventName = "Update";

    [SerializeField]
    protected bool onExit;
    protected string stateExitEventName = "Exit";

    [SerializeField]
    protected bool onMove;
    protected string stateMoveEventName = "Move";

    protected void BroadcastEvent(string evtName, Animator animator)
    {
        if(string.IsNullOrEmpty(evtName) || string.IsNullOrEmpty(stateName))
        {
            return; 
        }

        using (var slip = Neo.Utility.DataStructureLibrary<System.Text.StringBuilder>.Instance.CheckOut())
        {
            slip.Value.Clear();

            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(stateName));
            slip.Value.AppendFormat("{0}.{1}", stateName, evtName);

            var animHandler = animator.gameObject.GetComponent<AAnimatorEventHander>();
            animHandler?.OnAnimEvent(slip.Value.ToString());
        }
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (onEnter)
        {
            BroadcastEvent(stateEnterEventName, animator);
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (onUpdate)
        {
            BroadcastEvent(stateUpdateEventName, animator);
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (onExit)
        {
            BroadcastEvent(stateExitEventName, animator);
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (onMove)
        {
            BroadcastEvent(stateMoveEventName, animator);
        }
    }

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
