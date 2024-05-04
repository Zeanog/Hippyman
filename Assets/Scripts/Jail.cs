using UnityEngine;

public class Jail : MonoBehaviour
{
    public Vector3  Size { get; protected set; }

    protected void Awake()
    {
        var bc = GetComponent<BoxCollider>();
        Size = bc.size;
    }
}