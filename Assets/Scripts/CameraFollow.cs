using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField]
    protected Transform m_FollowTarget;
    
    [SerializeField]
    protected float m_Speed;

    protected Vector3 m_Offset;
    
    protected void Awake()
    {
        m_Offset = transform.position - m_FollowTarget.position;
    }

    protected void Update()
    {
        var prejectedPos = m_FollowTarget.position + m_Offset;
        var delta = prejectedPos - transform.position;

        transform.position += delta * m_Speed * Time.deltaTime;
    }
}