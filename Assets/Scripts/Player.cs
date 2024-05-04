using UnityEngine;

public class Player : ControlledAnimatedObject
{
    [SerializeField]
    protected PlayerController playerController;

    protected override void Awake()
    {
        base.Awake();
        SetController(playerController);
    }
}
