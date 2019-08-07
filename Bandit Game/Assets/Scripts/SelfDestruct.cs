using UnityEngine;
public class SelfDestruct : MonoBehaviour
{
    public float time;
    void Start()
    {
        Destroy(gameObject, time);
    }
}
