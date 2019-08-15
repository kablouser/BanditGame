using UnityEngine;

public abstract class Hitbox : MonoBehaviour
{
    public GameObject hitParticle;

    /// <summary>
    /// Calculates the damages done. And returns the rebounding force.
    /// </summary>
    /// <param name="hitCollider">collider which you collided with</param>
    /// <param name="incomingAttack">force of the attack</param>
    /// <returns>rebounding force</returns>
    public abstract float Hit(Collider hitCollider, float incomingAttack);

    public virtual float HitPosition(Collider hitCollider, float incomingAttack, Weapon useWeapon)
    {
        if (hitParticle)
        {                        
            Instantiate(hitParticle, hitCollider.ClosestPointOnBounds(useWeapon.attackBox.transform.position), Quaternion.identity, hitCollider.transform);
        }
        return Hit(hitCollider, incomingAttack);
    }

    /// <summary>
    /// Outputs damage after defence application.
    /// </summary>
    protected static float CalulateDamage(float attack, float defence)
    {
        float damage = attack - defence;
        if (damage < 0)
            damage = 0;
        return damage;
    }
}
