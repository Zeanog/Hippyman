using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using UnityEngine.UIElements;

public class GridToWorldTests
{
    protected bool ExecuteTests()
    {
        bool allTestsPassed = true;
        if( !Test1())
        {
            allTestsPassed = false;
            Debug.LogError("Test1 failed!");
        }

        if (!Test2())
        {
            allTestsPassed = false;
            Debug.LogError("Test2 failed!");
        }

        return allTestsPassed;
    }

    protected bool Test1()
    {
        Vector3 worldPos = new Vector3(4.5f, 0f, 0.5f);
        bool isOutOfBounds = !Game.Instance.WorldToGrid(worldPos, out Vector2Int loc);
        isOutOfBounds |= !Game.Instance.GridToWorld(loc, out Vector3 testPos);
        return !isOutOfBounds && worldPos.Equals(testPos);
    }

    protected bool Test2()
    {
        Vector2Int loc = new Vector2Int(4, 4);
        bool isOutOfBounds = !Game.Instance.GridToWorld(loc, out Vector3 pos);
        isOutOfBounds |= !Game.Instance.WorldToGrid(pos, out Vector2Int newLoc);
        return !isOutOfBounds && loc.Equals(newLoc);
    }
}

namespace Neo.Utility
{
    [InitializeOnLoad]
    public class TestingSystem : EditorWindow
    {
        protected static List<Type> testTypes = new List<Type>();

        static TestingSystem()
        {
            testTypes.Add(typeof(GridToWorldTests));
        }

        [MenuItem("Neo/TestingSystem")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            TestingSystem window = (TestingSystem)EditorWindow.GetWindow(typeof(TestingSystem));
            window.Show();
        }

        void Awake()
        {
            
        }

        void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            if (!UnityEngine.Application.isPlaying)
            {
                root.Add(new Label("Please press play, and reopen this window, to allow testing"));
                return;
            }

            Button btn = new Button();
            root.Add(btn);
            btn.name = "";
            btn.text = "Execute Tests";
            btn.clicked += () =>
            {
                foreach (var testType in testTypes)
                {
                    var testInst = Activator.CreateInstance(testType);
                    var methodInfo = testType.GetMethod("ExecuteTests", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    if (methodInfo != null)
                    {
                        var passed = (bool)methodInfo.Invoke(testInst, null);
                    }
                }
            };

            root.Add(new Label("Test Scripts:"));
            root.Add(new Label(""));//Space

            foreach (var type in testTypes)
            {
                Label l = new Label(type.Name);
                root.Add(l);
            }
        }

        void OnGUI()
        {
            
        }

        
    }
}