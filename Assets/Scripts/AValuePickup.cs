using UnityEngine;

public class AValuePickup<T> : APickup
{
    [SerializeField]
    protected T value;

    public T  Value {
        get => value;
        set => this.value = value;
    }
}