using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField]
    protected Transform followTarget;
    
    [SerializeField]
    protected float speed;

    protected Vector3 offset;
    
    protected void Awake()
    {
        offset = transform.position -followTarget.position;
    }

    protected void Update()
    {
        var prejectedPos = followTarget.position + offset;
        var delta = prejectedPos - transform.position;

        transform.position += delta * speed * Time.deltaTime;
    }
}