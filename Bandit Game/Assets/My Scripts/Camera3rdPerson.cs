using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera3rdPerson : MonoBehaviour
{
    [SerializeField]
    [Range(0,10)]
    private float smoothingValue;
    [SerializeField]
    private Transform cameraCenter;

    private struct SimpleRotation
    {
        private Vector2 values;
        public float x
        {
            get
            {
                return values.x;
            }
            set
            {
                values.x = value;
                if (values.x > 180)
                    values.x -= 360;
            }
        }
        public float y
        {
            get
            {
                return values.y;
            }
            set
            {
                values.y = value;
                if (values.y > 180)
                    values.y -= 360;
            }
        }
    }

    private SimpleRotation cameraRotation, modelRotation;
    private float maxDistance;

    /// <summary>
    /// Sets the camera rotation
    /// </summary>
    /// <param name="x">x degrees</param>
    public void RotateCamera(float x)
    {
        cameraCenter.localRotation = Quaternion.Euler(x, 0, 0);

        cameraRotation.x = x;
    }

    private void Awake()
    {
        cameraRotation.x = transform.rotation.eulerAngles.x;
        cameraRotation.y = transform.rotation.eulerAngles.y;

        modelRotation = cameraRotation;
    }

    private void Update()
    {
        cameraRotation.y = transform.rotation.eulerAngles.y;

        Vector2 newVector = Vector2.Lerp(new Vector2(modelRotation.x, modelRotation.y), new Vector2(cameraRotation.x, cameraRotation.y), smoothingValue * Time.deltaTime);
        modelRotation.x = newVector.x;
        modelRotation.y = newVector.y;

        RotatePlayerModel(modelRotation.x, modelRotation.y);
    }

    private void RotatePlayerModel(float x, float y)
    {
        //Change your model rotation here
    }
}
