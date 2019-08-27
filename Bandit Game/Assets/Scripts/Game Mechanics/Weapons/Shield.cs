using UnityEngine;

public class Shield : Weapon
{
    [Header("Shield")]
    public float defence;
    public float blockAngle;
    public float staminaDrain;
    public float staminaCost;

    public override float Hit(Collider hitCollider, float incomingAttack)
    {
        float damage = CalulateDamage(incomingAttack, defence);
        if (damage == 0)
        {
            //block successful
        }
        else
        {
            //block unsuccessful, stun the entity holding this shield
        }

        return damage;
    }
}
