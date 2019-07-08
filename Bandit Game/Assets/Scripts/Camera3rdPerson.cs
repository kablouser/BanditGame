using UnityEngine;
public class Camera3rdPerson : MonoBehaviour
{
    public Transform cameraCenter;
    public float rotationSpeed;

    public Transform cameraExtent;
    public LayerMask panningLayerMask;

    public Camera targetCamera;

    private float pitchDegrees, yawDegrees;

    private void Update()
    {
        yawDegrees += Input.GetAxis(InputConstants.MouseX) * rotationSpeed;
        pitchDegrees -= Input.GetAxis(InputConstants.MouseY) * rotationSpeed;
        cameraCenter.rotation = Quaternion.Euler(pitchDegrees, yawDegrees, 0);

        Vector3 cameraDirection = cameraExtent.position - cameraCenter.position;

        if (Physics.Raycast(cameraCenter.position, cameraDirection, out RaycastHit hitInfo, cameraDirection.magnitude, panningLayerMask))
        {
            targetCamera.transform.position = hitInfo.point;
        }
        else
        {
            targetCamera.transform.position = cameraExtent.position;
        }
    }
}