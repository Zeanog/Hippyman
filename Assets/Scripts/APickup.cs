using UnityEngine;
using System;

public abstract class APickup : MonoBehaviour
{
    public enum EPickupBehavior
    {
        Hide,
        Destroy
    }

    [SerializeField]
    protected EPickupBehavior onPickupBehavior = EPickupBehavior.Hide;

    [SerializeField]
    protected Vector3 rotationSpeed;

    [SerializeField]
    protected AudioClip pickupSoundFx;

    protected Vector3 initialPosition;

    protected Action onCollected;

    protected virtual void Awake()
    {
        initialPosition = transform.position;

        if (pickupSoundFx != null)
        {
            onCollected += () =>
            {
                AudioManager.Instance.Play(pickupSoundFx);
            };
        }
        onCollected += Cleanup;
    }

    protected void Cleanup()
    {
        if (onPickupBehavior == EPickupBehavior.Destroy)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    protected void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }

    protected void OnTriggerEnter(Collider other)
    {
        var collidee = other.gameObject.GetComponent<Player>();
        if (collidee != null)
        {
            onCollected?.Invoke();
        }
    }

    protected void OnCollisionEnter(Collision collision)
    {
        var collidee = collision.gameObject.GetComponent<Player>();
        if (collidee != null)
        {
            onCollected?.Invoke();
        }
    }
}