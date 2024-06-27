using System.Diagnostics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HydrodynamicsScript : MonoBehaviour
{
    [Header("ROV Properties")]
    [SerializeField] private float mass = 1.0f;
    [SerializeField] private bool isSubmerged = true;
    [SerializeField] private bool isNeutrallyBuoyant = true;
    [SerializeField] private float specifiedBuoyancy;
    [SerializeField] private Vector3 centerOfBuoyancyOffset;

    [Header("Fluid Properties")]
    [SerializeField] private float fluidDensity = 1025f;

    [Header("Rigidbody Properties")]
    [SerializeField] private float linearDamping = 1.0f;
    [SerializeField] private float angularDamping = 1.0f;

    [Header("Debugging")]
    [SerializeField] private bool showGUI = true;

    private float volume;
    private Vector3 buoyantForce;
    private Vector3 gravityForce;
    private Rigidbody rb;

    private const float initialNudgeForce = 0.0f;
    private const float initialNudgeTorque = 100.0f;

    private float deltaTime = 0.0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            UnityEngine.Debug.LogError("Rigidbody component not found. Ensure a Rigidbody is attached to the ROV.");
            return;
        }

        InitializeRigidbody();
        CalculateVolume();
        CalculateForces();

        ApplyInitialNudge();
    }

    private void FixedUpdate()
    {
        ApplyGravityForce();
        if (isSubmerged)
        {
            ApplyBuoyantForce();
        }
    }

    private void Update()
    {
        HandleUserInput();
        UpdateRigidbodyDamping();
        CalculateFPS();
    }

    private void OnGUI()
    {
        if (showGUI)
        {
            DisplayDebugInfo();
        }
    }

    private void OnDrawGizmos()
    {
        if (rb != null)
        {
            DrawBuoyancyGizmos();
        }
    }

    private void InitializeRigidbody()
    {
        mass = Mathf.Max(0.1f, mass);
        rb.mass = mass;
        rb.useGravity = false;
        rb.linearDamping = linearDamping;
        rb.angularDamping = angularDamping;
    }

    private void CalculateVolume()
    {
        if (isNeutrallyBuoyant)
        {
            volume = mass / fluidDensity;
        }
        else
        {
            specifiedBuoyancy = Mathf.Max(0.1f, specifiedBuoyancy);
            volume = specifiedBuoyancy / fluidDensity;
        }
    }

    private void CalculateForces()
    {
        gravityForce = new Vector3(0, mass * Physics.gravity.y, 0);
        buoyantForce = new Vector3(0, fluidDensity * volume * -Physics.gravity.y, 0);
    }

    private void ApplyInitialNudge()
    {
        rb.AddForce(Vector3.up * initialNudgeForce, ForceMode.Impulse);
        rb.AddTorque(new Vector3(initialNudgeTorque, 0, initialNudgeTorque), ForceMode.Impulse);
    }

    private void ApplyGravityForce()
    {
        rb.AddForce(gravityForce);
    }

    private void ApplyBuoyantForce()
    {
        Vector3 worldCenterOfBuoyancy = rb.position + rb.rotation * centerOfBuoyancyOffset;
        rb.AddForceAtPosition(buoyantForce, worldCenterOfBuoyancy);
    }

    private void HandleUserInput()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isSubmerged = !isSubmerged;
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            isNeutrallyBuoyant = !isNeutrallyBuoyant;
            CalculateVolume();
        }
    }

    private void UpdateRigidbodyDamping()
    {
        if (rb.linearDamping != linearDamping)
        {
            rb.linearDamping = linearDamping;
        }
        if (rb.angularDamping != angularDamping)
        {
            rb.angularDamping = angularDamping;
        }
    }

    private void CalculateFPS()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void DisplayDebugInfo()
    {
        int fps = Mathf.CeilToInt(1.0f / deltaTime);
        GUI.Label(new Rect(10, 10, 300, 20), "Mass: " + mass + " kg");
        GUI.Label(new Rect(10, 30, 300, 20), "Volume: " + volume + " m^3");
        GUI.Label(new Rect(10, 50, 300, 20), "Fluid Density: " + fluidDensity + " kg/m^3");
        GUI.Label(new Rect(10, 70, 300, 20), "Buoyant Force: " + buoyantForce + " N");
        GUI.Label(new Rect(10, 90, 300, 20), "Gravity Force: " + gravityForce + " N");
        GUI.Label(new Rect(10, 110, 300, 20), "FPS: " + fps);
    }

    private void DrawBuoyancyGizmos()
    {
        Gizmos.color = Color.blue;
        Vector3 worldCenterOfBuoyancy = transform.position + transform.rotation * centerOfBuoyancyOffset;
        Gizmos.DrawSphere(worldCenterOfBuoyancy, 0.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.TransformPoint(rb.centerOfMass), 0.1f);
    }
}
