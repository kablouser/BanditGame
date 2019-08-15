using System.Collections.Generic;
using UnityEngine;

public class EntityTrigger : MonoBehaviour
{
    public List<Entity> blackList;
    public List<Entity> enteredList;
    public bool updated;

    public float GetRange
    {
        get
        {
            return GetComponent<SphereCollider>().radius;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Entity getEntity = other.GetComponent<Entity>();
        if (getEntity && !blackList.Contains(getEntity) && !enteredList.Contains(getEntity))
        {
            updated = true;
            enteredList.Add(getEntity);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Entity getEntity = other.GetComponent<Entity>();
        if (getEntity && enteredList.Contains(getEntity))
        {
            updated = true;
            enteredList.Remove(getEntity);
        }
    }
}
