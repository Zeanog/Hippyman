using UnityEngine;

public class AdversaryController_Idle : AAdversaryController
{
    protected override Path PathToTarget()
    {
        return null;
    }

    public override void OnTriggerEnter(Collider other)
    {
        
    }

    public override void OnCollisionEnter(Collision collision)
    {
        
    }
}