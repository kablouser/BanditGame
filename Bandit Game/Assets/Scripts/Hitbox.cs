using UnityEngine;

public abstract class Hitbox : MonoBehaviour
{
    public GameObject hitParticle;

    /// <summary>
    /// Calculates the damages done. And returns the rebounding force.
    /// </summary>
    /// <param name="hitCollider">collider which you collided with</param>
    /// <param name="incomingForce">force of the attack</param>
    /// <returns>rebounding force</returns>
    public abstract float Hit(Collider hitCollider, float incomingForce);

    public float HitPosition(Collider hitCollider, float incomingForce, Vector3 hitPoint, Transform hitParent)
    {
        if(hitParticle)
            Instantiate(hitParticle, hitPoint, Quaternion.identity, hitParent);
        return Hit(hitCollider, incomingForce);
    }

    protected static float CalculateReboundForce(float incomingForce, float defence)
    {
        return defence - incomingForce;
    }
}
