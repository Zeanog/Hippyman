using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Neo.Grid))]
public class GridEditor : Editor
{
    protected void Awake()
    {
        
    }

    protected virtual void OnEnable()
    {

    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();
        if( GUILayout.Button("Rebuild") )
        {
            var grid = target as Neo.Grid;
            grid.DetermineGridInfo();
        }
    }
}
