using UnityEngine;

public class Camera3rdPerson : MonoBehaviour
{
    public Transform cameraCenter;
    public float rotationSpeed;

    private float pitchDegrees, yawDegrees;

    private void Update()
    {
        yawDegrees += Input.GetAxis("Mouse X") * rotationSpeed;
        pitchDegrees -= Input.GetAxis("Mouse Y") * rotationSpeed;
        cameraCenter.localRotation = Quaternion.Euler(pitchDegrees, yawDegrees, 0);
    }
}