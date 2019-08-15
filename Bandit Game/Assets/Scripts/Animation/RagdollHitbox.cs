using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollHitbox : MonoBehaviour
{
    public RagdollController controller;
    public Hitbox GetHitbox
    {
        get
        {
            return controller.ragdollHitbox;
        }
    }
}
