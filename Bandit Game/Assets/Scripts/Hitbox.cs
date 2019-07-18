using UnityEngine;

public abstract class Hitbox : MonoBehaviour
{
    /// <summary>
    /// Calculates the damages done. And returns the rebounding force.
    /// </summary>
    /// <param name="hitCollider">collider which you collided with</param>
    /// <param name="incomingForce">force of the attack</param>
    /// <returns>rebounding force</returns>
    public abstract float Hit(Collider hitCollider, float incomingForce);

    protected static float CalculateReboundForce(float incomingForce, float defence)
    {
        return defence - incomingForce;
    }
}
