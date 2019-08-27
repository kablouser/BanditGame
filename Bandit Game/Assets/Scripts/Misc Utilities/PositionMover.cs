using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionMover : MonoBehaviour
{
    public Vector3 position, rotation;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private void Awake()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }

    public void Move(bool newPosition)
    {
        if(newPosition)
        {
            transform.localPosition = position;
            transform.localRotation = Quaternion.Euler(rotation);
        }
        else
        {
            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;
        }
    }
}
