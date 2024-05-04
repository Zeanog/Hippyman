using System;
using System.Collections.Generic;
using UnityEngine;
using TCallback = System.Action<object, object>;

public abstract class EventManager : MonoBehaviour
{
    protected Dictionary<Type, Dictionary<string, TCallback>> callbackHandlers = new Dictionary<Type, Dictionary<string, TCallback>>();

    protected virtual void Awake()
    {
        
    }

    public void RemoveListener<T>(string evtName, TCallback listener)
    {
        Type t = typeof(T);
        Dictionary<string, TCallback> dict;
        if (!callbackHandlers.ContainsKey(t))
        {
            return;
        }

        dict = callbackHandlers[t];
        if (dict.ContainsKey(evtName))
        {
            dict[evtName] -= listener;
        }
    }

    public void AddListener<T>(string evtName, TCallback listener)
    {
        AddListener(typeof(T), evtName, listener);
    }

    public void AddListener( Type type, string evtName, TCallback listener)
    {
        if (!callbackHandlers.TryGetValue(type, out Dictionary<string, TCallback> dict))
        {
            dict = new Dictionary<string, TCallback>();
            callbackHandlers.Add(type, dict);
        }

        if (!dict.TryGetValue(evtName, out TCallback action))
        {
            action = new TCallback(listener);
            dict.Add(evtName, action);
        }
        else
        {
            action += listener;
        }

        dict[evtName] = action;
    }

    public virtual void BroadcastEvent(Type type, object sender, string evtName, object evtData)
    {
        while (true)
        {
            if (type.BaseType == null || type == typeof(MonoBehaviour))
            {
                break;
            }

            try
            {
                if (!callbackHandlers.TryGetValue(type, out Dictionary<string, TCallback> dict))
                {
                    return;
                }

                if (!dict.TryGetValue(evtName, out TCallback handler))
                {
                    return;
                }

                handler?.Invoke(sender, evtData);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            type = type.BaseType;
        }
    }

    public virtual void BroadcastEvent(object sender, string evtName, object evtData)
    {
        BroadcastEvent(sender.GetType(), sender, evtName, evtData);
    }

    public virtual void BroadcastEvent<T>(object sender, string evtName, object evtData)
    {
        Type t = typeof(T);
        BroadcastEvent(t , sender, evtName, evtData);
    }
}