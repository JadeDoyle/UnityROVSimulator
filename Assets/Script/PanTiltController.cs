using UnityEngine;

public class PanTiltController : MonoBehaviour
{
    [Header("Transforms")]
    public Transform panTransform;
    public Transform tiltTransform;

    [Header("Pan Settings")]
    public string panAxis = "Mouse X";
    public bool invertPan = false;
    public float maxPanAngle = 90f;

    [Header("Tilt Settings")]
    public string tiltAxis = "Mouse Y";
    public bool invertTilt = false;
    public float minTiltAngle = -45f;
    public float maxTiltAngle = 90f;

    [Header("Control Settings")]
    public KeyCode controlKey = KeyCode.Mouse0;
    public KeyCode resetKey = KeyCode.Mouse2;
    public float resetSpeed = 2f;
    public float smoothSpeed = 5f; // Speed for smooth transition

    private float currentPanAngle = 0f;
    private float currentTiltAngle = 0f;
    private float targetPanAngle = 0f;
    private float targetTiltAngle = 0f;

    void Update()
    {
        if (Input.GetKey(controlKey))
        {
            float panInput = Input.GetAxis(panAxis) * (invertPan ? -1 : 1);
            float tiltInput = Input.GetAxis(tiltAxis) * (invertTilt ? -1 : 1);

            targetPanAngle = Mathf.Clamp(targetPanAngle + panInput, -maxPanAngle, maxPanAngle);
            targetTiltAngle = Mathf.Clamp(targetTiltAngle + tiltInput, minTiltAngle, maxTiltAngle);
        }

        if (Input.GetKey(resetKey))
        {
            targetPanAngle = 0f;
            targetTiltAngle = 0f;
        }

        currentPanAngle = Mathf.Lerp(currentPanAngle, targetPanAngle, Time.deltaTime * smoothSpeed);
        currentTiltAngle = Mathf.Lerp(currentTiltAngle, targetTiltAngle, Time.deltaTime * smoothSpeed);

        if (panTransform != null)
        {
            panTransform.localRotation = Quaternion.Euler(0f, 0f, currentPanAngle);
        }

        if (tiltTransform != null)
        {
            tiltTransform.localRotation = Quaternion.Euler(0f, currentTiltAngle, 0f);
        }
    }
}
