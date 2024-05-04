using UnityEngine;

public class Shackle : MonoBehaviour
{
    [SerializeField]
    protected Color color = Color.white;

    protected void Awake()
    {
        var renderers = GetComponentsInChildren<MeshRenderer>();
        foreach(var renderer in renderers)
        {
            renderer.material.color = color;
        }
    }
}