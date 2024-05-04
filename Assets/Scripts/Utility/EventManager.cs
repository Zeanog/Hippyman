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
        else
        {
            dict = callbackHandlers[t];
        }

        dict.Remove(evtName);
    }

    public void AddListener<T>(string evtName, TCallback listener)
    {
        Type t = typeof(T);
        Dictionary<string, TCallback> dict;
        if (!callbackHandlers.ContainsKey(t))
        {
            dict = new Dictionary<string, TCallback>();
            callbackHandlers.Add(t, dict);
        }
        else
        {
            dict = callbackHandlers[t];
        }

        if (!dict.ContainsKey(evtName))
        {
            dict.Add(evtName, listener);
        }
        else
        {
            dict[evtName] += listener;
        }
    }

    public void AddListener(string compoundEvtName, TCallback listener)
    {
        var evtComponents =  compoundEvtName.Split(".", StringSplitOptions.RemoveEmptyEntries);
        string evtName = evtComponents[1];
        Type t = Type.GetType(evtComponents[0]);
        Dictionary<string, TCallback> dict;
        if (!callbackHandlers.ContainsKey(t))
        {
            dict = new Dictionary<string, TCallback>();
            callbackHandlers.Add(t, dict);
        }
        else
        {
            dict = callbackHandlers[t];
        }

        if (!dict.ContainsKey(evtName))
        {
            dict.Add(evtName, listener);
        }
        else
        {
            dict[evtName] += listener;
        }
    }

    public virtual void BroadcastEvent(object sender, string evtName, object evtData)
    {
        Type t = sender.GetType();
        while (true)
        {
            if (t.BaseType == null || t == typeof(MonoBehaviour))
            {
                break;
            }

            try
            {
                if (!callbackHandlers.TryGetValue(t, out Dictionary<string, TCallback> dict))
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

            t = t.BaseType;
        }
    }

    public virtual void BroadcastEvent<T>(object sender, string evtName, object evtData)
    {
        Type t = typeof(T);
        while (true)
        {
            if (t.BaseType == null || t == typeof(MonoBehaviour))
            {
                break;
            }

            try
            {
                if (!callbackHandlers.TryGetValue(t, out Dictionary<string, TCallback> dict))
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

            t = t.BaseType;
        }
    }
}