using UnityEngine;
using System.Collections;

public class MagicBook : Weapon
{
    [System.Serializable]
    public struct SpellAnimationData
    {
        public float damageDelay, duration;
        public string channelParameter, projectTrigger;

        public SpellAnimationData(float damageDelay, float duration, string channelParameter, string projectTrigger)
        {
            this.damageDelay = damageDelay;
            this.duration = duration;
            this.channelParameter = channelParameter;
            this.projectTrigger = projectTrigger;
        }
    }

    [Header("Magic Book")]
    public Animator spellBookAnimator;
    public string flickeringParameter = "flickering";
    public string closedParameter = "closed";

    public GameObject projectilePrefab;
    public SpellAnimationData attackData = new SpellAnimationData(0.45f, 1.0f, "spellChannel", "spellProject");
    public float channelDuration = 1.0f;

    public float manaCost;
    public float manaDrain;

    float startChannel;
    bool startedChannel;

    public void StartChannel()
    {
        spellBookAnimator.SetBool(closedParameter, false);
        spellBookAnimator.SetBool(flickeringParameter, true);
        startChannel = Time.time;
        startedChannel = true;
    }

    /// <summary>
    /// Returns true if spell is casted, false if 
    /// </summary>
    /// <param name="handPosition"></param>
    /// <param name="forwardDirection"></param>
    /// <returns></returns>
    public bool StopChannel(Vector3 handPosition, Vector3 forwardDirection, System.Func<bool> finalCheck, params Entity[] ignoreEntities)
    {
        spellBookAnimator.SetBool(closedParameter, true);
        spellBookAnimator.SetBool(flickeringParameter, false);

        if (startedChannel && startChannel < Time.time + channelDuration)
        {
            StartCoroutine(SpellCastRoutine(handPosition, forwardDirection, finalCheck, ignoreEntities));
            return true;
        }
        else
            return false;
    }

    IEnumerator SpellCastRoutine(Vector3 handPosition, Vector3 forwardDirection, System.Func<bool> finalCheck, params Entity[] ignoreEntities)
    {
        yield return new WaitForSeconds(attackData.damageDelay);
        if (finalCheck())
        {
            GameObject projectile = Instantiate(projectilePrefab, handPosition, Quaternion.LookRotation(forwardDirection));
            projectile.GetComponent<Rigidbody>().velocity = forwardDirection;
            //set projectile to ignore friendlies/self = ignoreEntities
        }
    }
}
