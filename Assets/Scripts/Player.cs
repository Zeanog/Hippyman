using UnityEngine;

public class Player : ACharacter
{
    [SerializeField]
    protected PlayerController playerController;

    protected override void Awake()
    {
        base.Awake();
        SetController(playerController);
    }
}
